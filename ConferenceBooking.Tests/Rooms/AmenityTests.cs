using ConferenceBooking.Domain.Rooms;

namespace ConferenceBooking.Tests.Rooms;

public class AmenityTests
{
    [Fact]
    public void Constructor_CreatesActiveAmenity_WithGivenNameAndPrice()
    {
        var amenity = new Amenity("Проєктор", 500m);

        Assert.Equal("Проєктор", amenity.Name);
        Assert.Equal(500m, amenity.Price);
        Assert.True(amenity.IsActive);
    }

    [Fact]
    public void Constructor_Throws_WhenNameIsWhitespace()
    {
        Assert.Throws<ArgumentException>(() => new Amenity("   ", 500m));
    }

    [Fact]
    public void Constructor_Throws_WhenPriceIsNegative()
    {
        Assert.Throws<ArgumentException>(() => new Amenity("Проєктор", -100m));
    }

    [Fact]
    public void Constructor_TrimsName()
    {
        var amenity = new Amenity("  Проєктор  ", 500m);

        Assert.Equal("Проєктор", amenity.Name);
    }

    [Fact]
    public void Constructor_RoundsPrice()
    {
        var amenity = new Amenity("Проєктор", 500.456m);

        Assert.Equal(500.46m, amenity.Price);
    }

    [Fact]
    public void Constructor_AllowsZeroPrice()
    {
        var amenity = new Amenity("Проєктор", 0m);

        Assert.Equal(0m, amenity.Price);
    }

    [Fact]
    public void Deactivate_SetsIsActiveToFalse()
    {
        var amenity = new Amenity("Проєктор", 500m);
        amenity.Deactivate();

        Assert.False(amenity.IsActive);
    }   
}

