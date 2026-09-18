namespace ConferenceBooking.Application.Rooms;

public sealed record AmenityDto(Guid Id, string Name, decimal Price);

public sealed record CreateAmenityRequest(string Name, decimal Price);

/// <summary>Id = null означає нову послугу; послуги, відсутні у списку, деактивуються.</summary>
public sealed record UpsertAmenityRequest(Guid? Id, string Name, decimal Price);

public sealed record CreateRoomRequest(
    string Name,
    int Capacity,
    decimal BasePricePerHour,
    IReadOnlyList<CreateAmenityRequest>? Amenities);

public sealed record UpdateRoomRequest(
    string Name,
    int Capacity,
    decimal BasePricePerHour,
    IReadOnlyList<UpsertAmenityRequest>? Amenities);

public sealed record RoomDto(
    Guid Id,
    string Name,
    int Capacity,
    decimal BasePricePerHour,
    IReadOnlyList<AmenityDto> Amenities);