using ConferenceBooking.Application.Abstractions;
using ConferenceBooking.Application.Common;
using ConferenceBooking.Domain.Bookings;
using ConferenceBooking.Domain.Common;
using ConferenceBooking.Domain.Exceptions;
using ConferenceBooking.Domain.Pricing;
using ConferenceBooking.Domain.Rooms;
using Microsoft.EntityFrameworkCore;

namespace ConferenceBooking.Application.Bookings;

public sealed class BookingService : IBookingService
{
    private readonly IApplicationDbContext _db;
    private readonly IPricingPolicy _pricingPolicy;
    private readonly IDateTimeProvider _clock;

    public BookingService(IApplicationDbContext db, IPricingPolicy pricingPolicy, IDateTimeProvider clock)
    {
        _db = db;
        _pricingPolicy = pricingPolicy;
        _clock = clock;
    }

    public async Task<IReadOnlyList<AvailableRoomDto>> FindAvailableRoomsAsync(
        AvailabilityRequest request, CancellationToken ct = default)
    {
        var period = BuildPeriod(request.StartsAt, request.DurationMinutes);

        var candidates = await _db.Rooms
            .AsNoTracking()
            .Include(r => r.Amenities)
            .Where(r => r.Capacity >= request.Capacity)
            .Where(r => !_db.Bookings.Any(b =>
                b.RoomId == r.Id &&
                b.Status == BookingStatus.Confirmed &&
                b.StartsAt < period.End &&
                b.EndsAt > period.Start))
            .OrderBy(r => r.BasePricePerHour)
            .ToListAsync(ct);

        return candidates.Select(room => BuildAvailability(room, period)).ToList();
    }

    public async Task<PriceQuoteDto> GetQuoteAsync(QuoteRequest request, CancellationToken ct = default)
    {
        var period = BuildPeriod(request.StartsAt, request.DurationMinutes);
        var room = await LoadRoomAsync(request.RoomId, ct);

        var amenities = (request.AmenityIds ?? [])
            .Select(room.RequireActiveAmenity)
            .ToList();

        return _pricingPolicy.Calculate(room, period, amenities).ToDto();
    }

    public async Task<BookingDto> CreateAsync(CreateBookingRequest request, CancellationToken ct = default)
    {
        var period = BuildPeriod(request.StartsAt, request.DurationMinutes);

        await using var transaction = await _db.BeginSerializableTransactionAsync(ct);

        var room = await LoadRoomAsync(request.RoomId, ct);

        await EnsureRoomIsFreeAsync(request.RoomId, period, ct);

        var booking = Booking.Create(
            room,
            period,
            request.Attendees,
            request.CustomerName,
            request.CustomerEmail,
            request.AmenityIds ?? [],
            _pricingPolicy,
            _clock.Now);

        _db.Bookings.Add(booking);

        await _db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return booking.ToDto();
    }

    public async Task<BookingDto> GetAsync(Guid bookingId, CancellationToken ct = default)
    {
        var booking = await _db.Bookings
            .AsNoTracking()
            .Include(b => b.Amenities)
            .FirstOrDefaultAsync(b => b.Id == bookingId, ct)
            ?? throw new NotFoundException("Бронювання", bookingId);

        return booking.ToDto();
    }

    public async Task CancelAsync(Guid bookingId, CancellationToken ct = default)
    {
        var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId, ct)
            ?? throw new NotFoundException("Бронювання", bookingId);

        booking.Cancel(_clock.Now);

        await _db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Перевірка виконується всередині Serializable-транзакції. Без неї два одночасні
    /// запити на один зал обидва побачили б «вільно» і обидва створили б бронь.
    /// </summary>
    private async Task EnsureRoomIsFreeAsync(Guid roomId, TimeRange period, CancellationToken ct)
    {
        var hasConflict = await _db.Bookings.AnyAsync(b =>
            b.RoomId == roomId &&
            b.Status == BookingStatus.Confirmed &&
            b.StartsAt < period.End &&
            b.EndsAt > period.Start, ct);

        if (hasConflict)
            throw new BookingConflictException(roomId, period.Start, period.End);
    }

    private AvailableRoomDto BuildAvailability(Room room, TimeRange period)
    {
        try
        {
            var quote = _pricingPolicy.Calculate(room, period, []);

            return new AvailableRoomDto(room.Id, room.Name, room.Capacity, room.BasePricePerHour, quote.ToDto(), null);
        }
        catch (DomainException ex)
        {
            // Зал вільний, але інтервал не тарифікується (напр. поза робочими годинами) —
            // повертаємо причину, а не ховаємо зал від клієнта.
            return new AvailableRoomDto(room.Id, room.Name, room.Capacity, room.BasePricePerHour, null, ex.Message);
        }
    }

    private async Task<Room> LoadRoomAsync(Guid roomId, CancellationToken ct) =>
        await _db.Rooms.Include(r => r.Amenities).FirstOrDefaultAsync(r => r.Id == roomId, ct)
        ?? throw new NotFoundException("Зал", roomId);

    private static TimeRange BuildPeriod(DateTime startsAt, int durationMinutes)
    {
        // JSON може прийти із суфіксом Z або зі зсувом. Нормалізуємо в локальний час бізнесу,
        // інакше SQL Server відхилить значення, а бізнес отримає зсунуті на кілька годин броні.
        var start = DateTime.SpecifyKind(startsAt, DateTimeKind.Unspecified);

        return TimeRange.FromDuration(start, TimeSpan.FromMinutes(durationMinutes));
    }
}