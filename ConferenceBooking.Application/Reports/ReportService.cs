using ConferenceBooking.Application.Abstractions;
using ConferenceBooking.Domain.Bookings;
using ConferenceBooking.Domain.Pricing;
using Microsoft.EntityFrameworkCore;

namespace ConferenceBooking.Application.Reports;

/// <summary>
/// Аналітика для бізнесу: де заробляємо, що простоює, які послуги окупаються,
/// чи правильно виставлені пікові години.
/// </summary>
public sealed class ReportService : IReportService
{
    private readonly IApplicationDbContext _db;
    private readonly PricingOptions _pricing;

    public ReportService(IApplicationDbContext db, PricingOptions pricing)
    {
        _db = db;
        _pricing = pricing;
    }

    public async Task<RevenueReportDto> GetRevenueAsync(ReportPeriodRequest request, CancellationToken ct = default)
    {
        var (from, to) = Normalize(request);

        var bookings = await _db.Bookings
            .AsNoTracking()
            .Where(b => b.StartsAt >= from && b.StartsAt < to)
            .Select(b => new
            {
                b.RoomId,
                b.Status,
                b.RoomCharge,
                b.AmenitiesCharge,
                b.TotalPrice
            })
            .ToListAsync(ct);

        var confirmed = bookings.Where(b => b.Status == BookingStatus.Confirmed).ToList();

        var roomNames = await _db.Rooms
            .AsNoTracking()
            .ToDictionaryAsync(r => r.Id, r => r.Name, ct);

        var byRoom = confirmed
            .GroupBy(b => b.RoomId)
            .Select(g => new RevenueByRoomDto(
                g.Key,
                roomNames.GetValueOrDefault(g.Key, "Видалений зал"),
                g.Count(),
                g.Sum(b => b.RoomCharge),
                g.Sum(b => b.AmenitiesCharge),
                g.Sum(b => b.TotalPrice)))
            .OrderByDescending(r => r.Total)
            .ToList();

        return new RevenueReportDto(
            from,
            to.AddDays(-1),
            confirmed.Sum(b => b.TotalPrice),
            confirmed.Sum(b => b.RoomCharge),
            confirmed.Sum(b => b.AmenitiesCharge),
            confirmed.Count,
            bookings.Count - confirmed.Count,
            byRoom);
    }

    public async Task<IReadOnlyList<RoomUtilizationDto>> GetUtilizationAsync(
        ReportPeriodRequest request, CancellationToken ct = default)
    {
        var (from, to) = Normalize(request);

        var workingHoursPerDay = (decimal)(_pricing.ClosesAt - _pricing.OpensAt).TotalHours;
        var days = (decimal)(to - from).TotalDays;
        var availableHours = workingHoursPerDay * days;

        var rooms = await _db.Rooms.AsNoTracking().ToListAsync(ct);

        var booked = await _db.Bookings
            .AsNoTracking()
            .Where(b => b.Status == BookingStatus.Confirmed && b.StartsAt >= from && b.StartsAt < to)
            .Select(b => new { b.RoomId, b.StartsAt, b.EndsAt })
            .ToListAsync(ct);

        return rooms
            .Select(room =>
            {
                var hours = booked
                    .Where(b => b.RoomId == room.Id)
                    .Sum(b => (decimal)(b.EndsAt - b.StartsAt).TotalHours);

                var percent = availableHours == 0
                    ? 0
                    : decimal.Round(hours / availableHours * 100, 2, MidpointRounding.AwayFromZero);

                return new RoomUtilizationDto(room.Id, room.Name, hours, availableHours, percent);
            })
            .OrderByDescending(r => r.UtilizationPercent)
            .ToList();
    }

    public async Task<IReadOnlyList<AmenityPopularityDto>> GetAmenityPopularityAsync(
        ReportPeriodRequest request, CancellationToken ct = default)
    {
        var (from, to) = Normalize(request);

        var amenities = await _db.Bookings
            .AsNoTracking()
            .Where(b => b.Status == BookingStatus.Confirmed && b.StartsAt >= from && b.StartsAt < to)
            .SelectMany(b => b.Amenities)
            .Select(a => new { a.Name, a.Price })
            .ToListAsync(ct);

        return amenities
            .GroupBy(a => a.Name)
            .Select(g => new AmenityPopularityDto(g.Key, g.Count(), g.Sum(a => a.Price)))
            .OrderByDescending(a => a.Revenue)
            .ToList();
    }

    public async Task<IReadOnlyList<HourlyDistributionDto>> GetHourlyDistributionAsync(
        ReportPeriodRequest request, CancellationToken ct = default)
    {
        var (from, to) = Normalize(request);

        var bookings = await _db.Bookings
            .AsNoTracking()
            .Where(b => b.Status == BookingStatus.Confirmed && b.StartsAt >= from && b.StartsAt < to)
            .Select(b => new { b.StartsAt, b.TotalPrice })
            .ToListAsync(ct);

        var grouped = bookings
            .GroupBy(b => b.StartsAt.Hour)
            .ToDictionary(g => g.Key, g => (Count: g.Count(), Revenue: g.Sum(b => b.TotalPrice)));

        // Повертаємо всі робочі години, включно з порожніми -інакше на графіку
        // будуть провали, і не видно, що зал у цей час просто не бронюють.
        return Enumerable
            .Range(_pricing.OpensAt.Hour, _pricing.ClosesAt.Hour - _pricing.OpensAt.Hour)
            .Select(hour =>
            {
                var data = grouped.GetValueOrDefault(hour);

                return new HourlyDistributionDto(hour, data.Count, data.Revenue);
            })
            .ToList();
    }

    /// <summary>
    /// Межі періоду включно за датою: To зсувається на добу вперед,
    /// щоб бронювання останнього дня потрапили у звіт.
    /// </summary>
    private static (DateTime From, DateTime To) Normalize(ReportPeriodRequest request)
    {
        var from = DateTime.SpecifyKind(request.From.Date, DateTimeKind.Unspecified);
        var to = DateTime.SpecifyKind(request.To.Date, DateTimeKind.Unspecified).AddDays(1);

        return (from, to);
    }
}