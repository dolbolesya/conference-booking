using ConferenceBooking.Domain.Common;
using ConferenceBooking.Domain.Exceptions;
using ConferenceBooking.Domain.Pricing;
using ConferenceBooking.Domain.Rooms;

namespace ConferenceBooking.Domain.Bookings;

/// <summary>
/// Бронювання - агрегат. Усі інваріанти, які перевіряються без звернення до сховища,
/// живуть тут. Перевірка перетину з чужими бронями потребує запиту до БД
/// і виконується в прикладному шарі під транзакцією.
/// </summary>
public sealed class Booking
{
    private readonly List<BookedAmenity> _amenities = new();

    /// <summary>Конструктор для EF Core. Заповнення відбувається через рефлексію.</summary>
    private Booking() { }

    private Booking(
        Room room,
        TimeRange period,
        int attendees,
        Guid userId,
        PriceBreakdown price)
    {
        Id = Guid.CreateVersion7();
        RoomId = room.Id;
        UserId = userId;
        StartsAt = period.Start;
        EndsAt = period.End;
        Attendees = attendees;
        RoomCharge = price.RoomCharge;
        AmenitiesCharge = price.AmenitiesCharge;
        TotalPrice = price.Total;
        Status = BookingStatus.Confirmed;
        CreatedAtUtc = DateTime.UtcNow;

        _amenities.AddRange(price.Amenities.Select(a => new BookedAmenity(a.AmenityId, a.Name, a.Price)));
    }

    public Guid Id { get; private set; }
    public Guid RoomId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime StartsAt { get; private set; }
    public DateTime EndsAt { get; private set; }
    public int Attendees { get; private set; }
    public decimal RoomCharge { get; private set; }
    public decimal AmenitiesCharge { get; private set; }
    public decimal TotalPrice { get; private set; }
    public BookingStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }

    public IReadOnlyCollection<BookedAmenity> Amenities => _amenities.AsReadOnly();

    public TimeRange Period => new(StartsAt, EndsAt);

    public bool IsActive => Status == BookingStatus.Confirmed;

    /// <summary>
    /// Єдиний спосіб створити бронювання. Перевірки йдуть від найдешевших до найдорожчих:
    /// розрахунок вартості виконується останнім, щоб не рахувати ціну для завідомо невалідної броні.
    /// </summary>
    public static Booking Create(
        Room room,
        TimeRange period,
        int attendees,
        Guid userId,
        IReadOnlyCollection<Guid> amenityIds,
        IPricingPolicy pricingPolicy,
        DateTime now)
    {
        ArgumentNullException.ThrowIfNull(room);
        ArgumentNullException.ThrowIfNull(amenityIds);
        ArgumentNullException.ThrowIfNull(pricingPolicy);

        if (userId == Guid.Empty)
            throw new ArgumentException("Ідентифікатор користувача обов'язковий.", nameof(userId));

        if (room.IsDeleted)
            throw new DomainException("room_unavailable", $"Зал «{room.Name}» більше не здається в оренду.");

        if (period.Start <= now)
            throw new DomainException("invalid_period", "Бронювати можна лише майбутній час.");

        if (attendees <= 0)
            throw new DomainException("invalid_attendees", "Кількість учасників має бути більшою за нуль.");

        if (!room.CanHost(attendees))
            throw new DomainException("capacity_exceeded",
                $"Зал «{room.Name}» вміщує {room.Capacity} осіб, запитано {attendees}.");

        if (amenityIds.Distinct().Count() != amenityIds.Count)
            throw new DomainException("duplicate_amenity", "Послуги в запиті дублюються.");

        var amenities = amenityIds.Select(room.RequireActiveAmenity).ToList();
        var price = pricingPolicy.Calculate(room, period, amenities);

        return new Booking(room, period, attendees, userId, price);
    }

    /// <summary>
    /// Керувати бронню може лише її власник або адміністратор. Перевірка живе в домені,
    /// щоб її не можна було обійти з іншого сценарію.
    /// </summary>
    public void EnsureOwnedBy(Guid userId, bool isAdmin)
    {
        if (!isAdmin && UserId != userId)
            throw new DomainException("access_denied", "Ви можете керувати лише власними бронюваннями.");
    }

    /// <summary>Скасування можливе лише до початку бронювання.</summary>
    public void Cancel(DateTime now)
    {
        if (Status == BookingStatus.Cancelled)
            throw new DomainException("already_cancelled", "Бронювання вже скасоване.");

        if (StartsAt <= now)
            throw new DomainException("cancellation_too_late", "Бронювання, яке вже почалося, скасувати не можна.");

        Status = BookingStatus.Cancelled;
        CancelledAtUtc = DateTime.UtcNow;
    }
}