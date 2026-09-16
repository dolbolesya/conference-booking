using ConferenceBooking.Application.Abstractions;
using ConferenceBooking.Application.Common;
using ConferenceBooking.Domain.Exceptions;
using ConferenceBooking.Domain.Rooms;
using Microsoft.EntityFrameworkCore;

namespace ConferenceBooking.Application.Rooms;

public sealed class RoomService : IRoomService
{
    private readonly IApplicationDbContext _db;

    public RoomService(IApplicationDbContext db) => _db = db;

    public async Task<RoomDto> CreateAsync(CreateRoomRequest request, CancellationToken ct = default)
    {
        var room = new Room(request.Name, request.Capacity, request.BasePricePerHour);

        foreach (var amenity in request.Amenities ?? [])
            room.AddAmenity(amenity.Name, amenity.Price);

        _db.Rooms.Add(room);
        await _db.SaveChangesAsync(ct);

        return room.ToDto();
    }

    public async Task<RoomDto> GetAsync(Guid roomId, CancellationToken ct = default) =>
        (await LoadRoomAsync(roomId, ct)).ToDto();

    public async Task<IReadOnlyList<RoomDto>> ListAsync(CancellationToken ct = default)
    {
        var rooms = await _db.Rooms
            .AsNoTracking()
            .Include(r => r.Amenities)
            .OrderBy(r => r.Name)
            .ToListAsync(ct);

        return rooms.Select(r => r.ToDto()).ToList();
    }

    public async Task<RoomDto> UpdateAsync(Guid roomId, UpdateRoomRequest request, CancellationToken ct = default)
    {
        var room = await LoadRoomAsync(roomId, ct);

        room.Rename(request.Name);
        room.ChangeCapacity(request.Capacity);
        room.ChangeBasePrice(request.BasePricePerHour);

        SyncAmenities(room, request.Amenities ?? []);

        await _db.SaveChangesAsync(ct);

        return room.ToDto();
    }

    public async Task DeleteAsync(Guid roomId, CancellationToken ct = default)
    {
        var room = await LoadRoomAsync(roomId, ct);
        room.Delete();

        await _db.SaveChangesAsync(ct);
    }
    
    private async Task<Room> LoadRoomAsync(Guid roomId, CancellationToken ct) =>
        await _db.Rooms.Include(r => r.Amenities).FirstOrDefaultAsync(r => r.Id == roomId, ct)
        ?? throw new NotFoundException("Зал", roomId);

    /// <summary>
    /// PUT-семантика: список послуг у запиті стає повним станом залу.
    /// Знімок наявних робиться ДО змін — інакше щойно додані послуги
    /// одразу потрапили б під деактивацію.
    /// </summary>
    private static void SyncAmenities(Room room, IReadOnlyList<UpsertAmenityRequest> requested)
    {
        var existingActiveIds = room.Amenities.Where(a => a.IsActive).Select(a => a.Id).ToList();
        var keptIds = requested.Where(a => a.Id.HasValue).Select(a => a.Id!.Value).ToHashSet();

        foreach (var amenityId in existingActiveIds.Where(id => !keptIds.Contains(id)))
            room.RemoveAmenity(amenityId);

        foreach (var amenity in requested)
        {
            if (amenity.Id is { } id)
                room.UpdateAmenity(id, amenity.Name, amenity.Price);
            else
                room.AddAmenity(amenity.Name, amenity.Price);
        }
    }
}