using ConferenceBooking.Application.Bookings;
using ConferenceBooking.Application.Rooms;
using ConferenceBooking.Domain.Bookings;
using ConferenceBooking.Domain.Pricing;
using ConferenceBooking.Domain.Rooms;

namespace ConferenceBooking.Application.Common;

/// <summary>
/// Ручний мапінг замість AutoMapper: менше магії, видно точно, які поля
/// залишають межі системи — це важливо для безпеки.
/// </summary>
public static class Mapping
{
    public static RoomDto ToDto(this Room room) => new(
        room.Id,
        room.Name,
        room.Capacity,
        room.BasePricePerHour,
        room.Amenities
            .Where(a => a.IsActive)
            .OrderBy(a => a.Name)
            .Select(a => new AmenityDto(a.Id, a.Name, a.Price))
            .ToList());

    public static PriceQuoteDto ToDto(this PriceBreakdown price) => new(
        price.Period.Start,
        price.Period.End,
        price.BasePricePerHour,
        price.RoomCharge,
        price.AmenitiesCharge,
        price.Total,
        price.Segments
            .Select(s => new PriceSegmentDto(s.RateName, s.Start, s.End, s.Hours, s.Multiplier, s.Amount))
            .ToList(),
        price.Amenities
            .Select(a => new AmenityChargeDto(a.AmenityId, a.Name, a.Price))
            .ToList());

    public static BookingDto ToDto(this Booking booking) => new(
        booking.Id,
        booking.RoomId,
        booking.StartsAt,
        booking.EndsAt,
        booking.Attendees,
        booking.CustomerName,
        booking.CustomerEmail,
        booking.Status.ToString(),
        booking.RoomCharge,
        booking.AmenitiesCharge,
        booking.TotalPrice,
        booking.Amenities
            .Select(a => new AmenityChargeDto(a.AmenityId, a.Name, a.Price))
            .ToList(),
        booking.CreatedAtUtc);
}