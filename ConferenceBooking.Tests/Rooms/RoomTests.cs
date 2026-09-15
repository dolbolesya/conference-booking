using ConferenceBooking.Domain.Rooms;

namespace ConferenceBooking.Tests.Rooms;

public class RoomTests
{
    private static Room CreateRoom() => new("Зал А", 50, 2000m);

    [Fact]
    public void Constructor_CreatesRoom_WithGivenParameters()
    {
        var room = CreateRoom();

        Assert.Equal("Зал А", room.Name);
        Assert.Equal(50, room.Capacity);
        Assert.Equal(2000m, room.BasePricePerHour);
        Assert.False(room.IsDeleted);
        Assert.Empty(room.Amenities);
    }

    [Fact]
    public void AddAmenity_Throws_WhenNameDiffersOnlyByCase()
    {
        var room = CreateRoom();
        room.AddAmenity("Проєктор", 500m);

        Assert.Throws<ArgumentException>(() => room.AddAmenity("проєктор", 700m));
    }

    [Fact]
    public void RemoveAmenity_DeactivatesButKeepsInCollection()
    {
        var room = CreateRoom();
        var amenity = room.AddAmenity("Проєктор", 500m);

        room.RemoveAmenity(amenity.Id);

        Assert.Single(room.Amenities);
        Assert.False(amenity.IsActive);
    }

    [Fact]
    public void ChangeCapacity_Throws_WhenCapacityIsZero()
    {
        var room = CreateRoom();

        Assert.Throws<ArgumentException>(() => room.ChangeCapacity(0));
    }
    
    [Fact]
    public void ChangeBasePrice_Throws_WhenPriceIsZero()
    {
        var room = CreateRoom();

        Assert.Throws<ArgumentException>(() => room.ChangeBasePrice(0m));
    }

    [Fact]
    public void AddAmenity_AddsAmenityToCollection()
    {
        var room = CreateRoom();
        var amenity = room.AddAmenity("Проєктор", 500m);

        Assert.Contains(amenity, room.Amenities);
    }

    [Fact]
    public void UpdateAmenity_ReactivatesDeactivatedAmenity()
    {
        var room = CreateRoom();
        var amenity = room.AddAmenity("Проєктор", 500m);
        room.RemoveAmenity(amenity.Id);

        room.UpdateAmenity(amenity.Id, "Проєктор", 700m);

        Assert.True(amenity.IsActive);
    }

    [Fact]
    public void CanHost_ReturnsTrue_WhenAttendeesEqualCapacity()
    {
        var room = CreateRoom();

        Assert.True(room.CanHost(50));
    }

    [Fact]
    public void CanHost_ReturnsFalse_WhenAttendeesExceedCapacity()
    {
        var room = CreateRoom();

        Assert.False(room.CanHost(51));
    }   

    [Fact]
    public void Delete_SetsFlagAndTimestamp()
    {
        var room = CreateRoom();
        room.Delete();

        Assert.True(room.IsDeleted);
        Assert.NotNull(room.DeletedAtUtc);
    }
    
    [Fact]
    public void Delete_KeepsOriginalTimestamp_WhenCalledTwice()
    {
        var room = CreateRoom();
        room.Delete();
        var firstDeletionTime = room.DeletedAtUtc;

        room.Delete();

        Assert.Equal(firstDeletionTime, room.DeletedAtUtc);
    }
}