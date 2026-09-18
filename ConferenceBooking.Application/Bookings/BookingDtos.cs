namespace ConferenceBooking.Application.Bookings;

/// <summary>Пошук вільних залів на конкретний інтервал.</summary>
public sealed record AvailabilityRequest(
    DateTime StartsAt,
    int DurationMinutes,
    int Capacity);

public sealed record PriceSegmentDto(
    string RateName,
    DateTime Start,
    DateTime End,
    decimal Hours,
    decimal Multiplier,
    decimal Amount);

public sealed record AmenityChargeDto(Guid AmenityId, string Name, decimal Price);

/// <summary>Розрахунок вартості без створення бронювання.</summary>
public sealed record PriceQuoteDto(
    DateTime StartsAt,
    DateTime EndsAt,
    decimal BasePricePerHour,
    decimal RoomCharge,
    decimal AmenitiesCharge,
    decimal Total,
    IReadOnlyList<PriceSegmentDto> Segments,
    IReadOnlyList<AmenityChargeDto> Amenities);

/// <summary>Вільний зал разом із попереднім розрахунком саме на цей інтервал.</summary>
public sealed record AvailableRoomDto(
    Guid RoomId,
    string Name,
    int Capacity,
    decimal BasePricePerHour,
    PriceQuoteDto? Quote,
    string? QuoteError);

/// <summary>
/// Дані замовника не передаються в запиті - вони беруться з токена.
/// Інакше будь-хто міг би створити бронь від чужого імені.
/// </summary>
public sealed record CreateBookingRequest(
    Guid RoomId,
    DateTime StartsAt,
    int DurationMinutes,
    int Attendees,
    IReadOnlyList<Guid>? AmenityIds);

public sealed record QuoteRequest(
    Guid RoomId,
    DateTime StartsAt,
    int DurationMinutes,
    IReadOnlyList<Guid>? AmenityIds);

public sealed record BookingDto(
    Guid Id,
    Guid RoomId,
    Guid UserId,
    DateTime StartsAt,
    DateTime EndsAt,
    int Attendees,
    string Status,
    decimal RoomCharge,
    decimal AmenitiesCharge,
    decimal TotalPrice,
    IReadOnlyList<AmenityChargeDto> Amenities,
    DateTime CreatedAtUtc);