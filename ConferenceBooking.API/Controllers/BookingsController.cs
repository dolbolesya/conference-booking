using ConferenceBooking.Application.Bookings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ConferenceBooking.API.Controllers;

[ApiController]
[Route("api/bookings")]
[Authorize]
public sealed class BookingsController : ControllerBase
{
    private readonly IBookingService _bookings;

    public BookingsController(IBookingService bookings) => _bookings = bookings;

    /// <summary>Пошук доступних залів на вказаний інтервал.</summary>
    [HttpPost("availability")]
    [AllowAnonymous]

    public async Task<ActionResult<IReadOnlyList<AvailableRoomDto>>> FindAvailable(
        AvailabilityRequest request, CancellationToken ct) =>
        Ok(await _bookings.FindAvailableRoomsAsync(request, ct));

    /// <summary>Розрахунок вартості без створення бронювання.</summary>
    [HttpPost("quote")]
    [AllowAnonymous]

    public async Task<ActionResult<PriceQuoteDto>> Quote(QuoteRequest request, CancellationToken ct) =>
        Ok(await _bookings.GetQuoteAsync(request, ct));

    /// <summary>Бронювання залу з розрахунком загальної вартості.</summary>
    [HttpPost]
    [EnableRateLimiting("bookings")]
    public async Task<ActionResult<BookingDto>> Create(CreateBookingRequest request, CancellationToken ct)
    {
        var booking = await _bookings.CreateAsync(request, ct);

        return CreatedAtAction(nameof(Get), new { bookingId = booking.Id }, booking);
    }

    /// <summary>Бронювання за ідентифікатором.</summary>
    [HttpGet("{bookingId:guid}")]
    public async Task<ActionResult<BookingDto>> Get(Guid bookingId, CancellationToken ct) =>
        Ok(await _bookings.GetAsync(bookingId, ct));

    /// <summary>Скасування бронювання.</summary>
    [HttpDelete("{bookingId:guid}")]
    public async Task<IActionResult> Cancel(Guid bookingId, CancellationToken ct)
    {
        await _bookings.CancelAsync(bookingId, ct);

        return NoContent();
    }
}