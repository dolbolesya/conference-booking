using ConferenceBooking.Application.Rooms;
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
}