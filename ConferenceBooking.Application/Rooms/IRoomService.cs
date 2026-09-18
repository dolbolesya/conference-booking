namespace ConferenceBooking.Application.Rooms;

public interface IRoomService
{
    Task<RoomDto> CreateAsync(CreateRoomRequest request, CancellationToken ct = default);
    Task<RoomDto> UpdateAsync(Guid roomId, UpdateRoomRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid roomId, CancellationToken ct = default);
    Task<RoomDto> GetAsync(Guid roomId, CancellationToken ct = default);
    Task<IReadOnlyList<RoomDto>> ListAsync(CancellationToken ct = default);
}