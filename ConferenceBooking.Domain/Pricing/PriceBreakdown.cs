using ConferenceBooking.Domain.Common;

namespace ConferenceBooking.Domain.Pricing;

/// <summary>Частина вартості, порахована за одним тарифним вікном.</summary>
public sealed record PriceSegment(
    string RateName,
    DateTime Start,
    DateTime End,
    decimal Hours,
    decimal Multiplier,
    decimal Amount);

public sealed record AmenityCharge(Guid AmenityId, string Name, decimal Price);

/// <summary>
/// Прозорий розрахунок: клієнт бачить не лише суму, а й з чого вона складається.
/// Знімає суперечки з клієнтами та спрощує підтримку.
/// </summary>
public sealed record PriceBreakdown(
    TimeRange Period,
    decimal BasePricePerHour,
    IReadOnlyList<PriceSegment> Segments,
    IReadOnlyList<AmenityCharge> Amenities)
{
    public decimal RoomCharge => decimal.Round(Segments.Sum(s => s.Amount), 2, MidpointRounding.AwayFromZero);
    public decimal AmenitiesCharge => decimal.Round(Amenities.Sum(a => a.Price), 2, MidpointRounding.AwayFromZero);
    public decimal Total => RoomCharge + AmenitiesCharge;
}