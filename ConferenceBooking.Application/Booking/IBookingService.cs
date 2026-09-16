namespace ConferenceBooking.Application.Bookings;

public interface IBookingService
{
    /// <summary>Пункт 4 ТЗ: пошук доступних залів на інтервал.</summary>
    Task<IReadOnlyList<AvailableRoomDto>> FindAvailableRoomsAsync(
        AvailabilityRequest request, CancellationToken ct = default);

    /// <summary>Розрахунок вартості без створення броні.</summary>
    Task<PriceQuoteDto> GetQuoteAsync(QuoteRequest request, CancellationToken ct = default);

    /// <summary>Пункт 5 ТЗ: бронювання залу з розрахунком вартості.</summary>
    Task<BookingDto> CreateAsync(CreateBookingRequest request, CancellationToken ct = default);

    Task<BookingDto> GetAsync(Guid bookingId, CancellationToken ct = default);

    Task CancelAsync(Guid bookingId, CancellationToken ct = default);
}