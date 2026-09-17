using ConferenceBooking.Application.Rooms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceBooking.API.Controllers;

[ApiController]
[Route("api/rooms")]
[Authorize(Roles = "Admin")]

public sealed class RoomsController : ControllerBase
{
    private readonly IRoomService _rooms;

    public RoomsController(IRoomService rooms) => _rooms = rooms;

    /// <summary>Список усіх доступних залів.</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<RoomDto>>> List(CancellationToken ct) =>
        Ok(await _rooms.ListAsync(ct));

    /// <summary>Зал за ідентифікатором.</summary>
    [HttpGet("{roomId:guid}")]
    [AllowAnonymous]

    public async Task<ActionResult<RoomDto>> Get(Guid roomId, CancellationToken ct) =>
        Ok(await _rooms.GetAsync(roomId, ct));

    /// <summary>Додавання конференц-залу.</summary>
    [HttpPost]
    public async Task<ActionResult<RoomDto>> Create(CreateRoomRequest request, CancellationToken ct)
    {
        var room = await _rooms.CreateAsync(request, ct);

        return CreatedAtAction(nameof(Get), new { roomId = room.Id }, room);
    }

    /// <summary>Редагування інформації про зал.</summary>
    [HttpPut("{roomId:guid}")]
    public async Task<ActionResult<RoomDto>> Update(Guid roomId, UpdateRoomRequest request, CancellationToken ct) =>
        Ok(await _rooms.UpdateAsync(roomId, request, ct));

    /// <summary>Видалення конференц-залу.</summary>
    [HttpDelete("{roomId:guid}")]
    public async Task<IActionResult> Delete(Guid roomId, CancellationToken ct)
    {
        await _rooms.DeleteAsync(roomId, ct);

        return NoContent();
    }
}