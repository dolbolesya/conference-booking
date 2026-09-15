using ConferenceBooking.Domain.Common;
using ConferenceBooking.Domain.Rooms;

namespace ConferenceBooking.Domain.Pricing;

/// <summary>
/// Абстракція правил ціноутворення (Strategy). Дозволяє додати сезонні тарифи
/// чи корпоративні знижки, не змінюючи агрегат бронювання.
/// </summary>
public interface IPricingPolicy
{
    PriceBreakdown Calculate(Room room, TimeRange period, IReadOnlyCollection<Amenity> amenities);
}