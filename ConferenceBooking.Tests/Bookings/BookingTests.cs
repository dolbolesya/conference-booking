using ConferenceBooking.Domain.Bookings;
using ConferenceBooking.Domain.Common;
using ConferenceBooking.Domain.Exceptions;
using ConferenceBooking.Domain.Pricing;
using ConferenceBooking.Domain.Rooms;

namespace ConferenceBooking.Tests.Bookings;

public class BookingTests
{
    private static readonly DateTime Now = new(2026, 9, 15, 8, 0, 0);
    private static readonly Guid OwnerId = Guid.CreateVersion7();
    private static readonly Guid StrangerId = Guid.CreateVersion7();

    private readonly TimeBasedPricingPolicy _policy = new(new PricingOptions());

    private static Room CreateRoom() => new("Зал А", 50, 2000m);

    private static TimeRange Tomorrow(int startHour, int endHour) =>
        new(new DateTime(2026, 9, 16, startHour, 0, 0),
            new DateTime(2026, 9, 16, endHour, 0, 0));

    private Booking CreateBooking(
        Room room,
        TimeRange period,
        int attendees = 10,
        IReadOnlyCollection<Guid>? amenityIds = null) =>
        Booking.Create(room, period, attendees, OwnerId, amenityIds ?? [], _policy, Now);

    [Fact]
    public void Create_CalculatesTotalPrice_FromRoomAndAmenities()
    {
        var room = CreateRoom();
        var projector = room.AddAmenity("Проєктор", 500m);

        var booking = CreateBooking(room, Tomorrow(10, 14), amenityIds: [projector.Id]);

        Assert.Equal(8600m, booking.RoomCharge);
        Assert.Equal(500m, booking.AmenitiesCharge);
        Assert.Equal(9100m, booking.TotalPrice);
    }

    [Fact]
    public void Create_AssignsOwner()
    {
        var booking = CreateBooking(CreateRoom(), Tomorrow(10, 12));

        Assert.Equal(OwnerId, booking.UserId);
    }

    [Fact]
    public void Create_Throws_WhenUserIdIsEmpty()
    {
        var room = CreateRoom();

        Assert.Throws<ArgumentException>(() =>
            Booking.Create(room, Tomorrow(10, 12), 10, Guid.Empty, [], _policy, Now));
    }

    [Fact]
    public void Create_Throws_WhenPeriodIsInThePast()
    {
        var room = CreateRoom();
        var period = new TimeRange(Now.AddHours(-2), Now.AddHours(-1));

        var exception = Assert.Throws<DomainException>(() => CreateBooking(room, period));

        Assert.Equal("invalid_period", exception.Code);
    }

    [Fact]
    public void Create_Throws_WhenAttendeesExceedCapacity()
    {
        var room = CreateRoom();

        var exception = Assert.Throws<DomainException>(() =>
            CreateBooking(room, Tomorrow(10, 12), attendees: 51));

        Assert.Equal("capacity_exceeded", exception.Code);
    }

    [Fact]
    public void Create_Throws_WhenRoomIsDeleted()
    {
        var room = CreateRoom();
        room.Delete();

        var exception = Assert.Throws<DomainException>(() => CreateBooking(room, Tomorrow(10, 12)));

        Assert.Equal("room_unavailable", exception.Code);
    }

    [Fact]
    public void Create_Throws_WhenAmenityIsInactive()
    {
        var room = CreateRoom();
        var projector = room.AddAmenity("Проєктор", 500m);
        room.RemoveAmenity(projector.Id);

        var exception = Assert.Throws<DomainException>(() =>
            CreateBooking(room, Tomorrow(10, 12), amenityIds: [projector.Id]));

        Assert.Equal("amenity_inactive", exception.Code);
    }

    [Fact]
    public void Create_Throws_WhenAmenityIdsContainDuplicates()
    {
        var room = CreateRoom();
        var projector = room.AddAmenity("Проєктор", 500m);

        var exception = Assert.Throws<DomainException>(() =>
            CreateBooking(room, Tomorrow(10, 12), amenityIds: [projector.Id, projector.Id]));

        Assert.Equal("duplicate_amenity", exception.Code);
    }

    [Fact]
    public void Create_StoresAmenitySnapshot_NotReference()
    {
        var room = CreateRoom();
        var projector = room.AddAmenity("Проєктор", 500m);
        var booking = CreateBooking(room, Tomorrow(10, 12), amenityIds: [projector.Id]);

        room.UpdateAmenity(projector.Id, "Проєктор HD", 900m);

        var snapshot = booking.Amenities.Single();

        Assert.Equal("Проєктор", snapshot.Name);
        Assert.Equal(500m, snapshot.Price);
        Assert.Equal(500m, booking.AmenitiesCharge);
    }

    [Fact]
    public void EnsureOwnedBy_DoesNotThrow_ForOwner()
    {
        var booking = CreateBooking(CreateRoom(), Tomorrow(10, 12));

        booking.EnsureOwnedBy(OwnerId, isAdmin: false);
    }

    [Fact]
    public void EnsureOwnedBy_Throws_ForStranger()
    {
        var booking = CreateBooking(CreateRoom(), Tomorrow(10, 12));

        var exception = Assert.Throws<DomainException>(() => booking.EnsureOwnedBy(StrangerId, isAdmin: false));

        Assert.Equal("access_denied", exception.Code);
    }

    [Fact]
    public void EnsureOwnedBy_DoesNotThrow_ForAdmin()
    {
        var booking = CreateBooking(CreateRoom(), Tomorrow(10, 12));

        booking.EnsureOwnedBy(StrangerId, isAdmin: true);
    }

    [Fact]
    public void Cancel_SetsStatusAndTimestamp()
    {
        var booking = CreateBooking(CreateRoom(), Tomorrow(10, 12));

        booking.Cancel(Now);

        Assert.Equal(BookingStatus.Cancelled, booking.Status);
        Assert.False(booking.IsActive);
        Assert.NotNull(booking.CancelledAtUtc);
    }

    [Fact]
    public void Cancel_Throws_WhenBookingAlreadyStarted()
    {
        var booking = CreateBooking(CreateRoom(), Tomorrow(10, 12));

        var afterStart = new DateTime(2026, 9, 16, 11, 0, 0);

        var exception = Assert.Throws<DomainException>(() => booking.Cancel(afterStart));

        Assert.Equal("cancellation_too_late", exception.Code);
    }

    [Fact]
    public void Cancel_Throws_WhenAlreadyCancelled()
    {
        var booking = CreateBooking(CreateRoom(), Tomorrow(10, 12));
        booking.Cancel(Now);

        var exception = Assert.Throws<DomainException>(() => booking.Cancel(Now));

        Assert.Equal("already_cancelled", exception.Code);
    }
}