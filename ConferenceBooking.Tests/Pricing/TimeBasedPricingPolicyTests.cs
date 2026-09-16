using ConferenceBooking.Domain.Common;
using ConferenceBooking.Domain.Pricing;
using ConferenceBooking.Domain.Rooms;
using ConferenceBooking.Domain.Exceptions;

namespace ConferenceBooking.Tests.Pricing;

public class TimeBasedPricingPolicyTests
{
    private readonly TimeBasedPricingPolicy _policy = new(new PricingOptions());

    private static Room CreateRoom() => new("Зал А", 50, 2000m);

    private static TimeRange Period(int startHour, int endHour) =>
        new(new DateTime(2026, 9, 15, startHour, 0, 0),
            new DateTime(2026, 9, 15, endHour, 0, 0));

    private decimal RoomChargeFor(TimeRange period) =>
        _policy.Calculate(CreateRoom(), period, []).RoomCharge;

    [Fact]
    public void Calculate_AppliesStandardRate_WhenFullyInStandardHours()
    {
        Assert.Equal(4000m, RoomChargeFor(Period(10, 12)));
    }

    [Fact]
    public void Calculate_SplitsIntoTwoSegments_WhenCrossingPeakBoundary()
    {
        var breakdown = _policy.Calculate(CreateRoom(), Period(10, 14), []);

        Assert.Equal(2, breakdown.Segments.Count);
        Assert.Equal(8600m, breakdown.RoomCharge);
    }

    [Fact]
    public void Calculate_AppliesPeakSurcharge_WhenFullyInPeakHours()
    {
        Assert.Equal(4600m, RoomChargeFor(Period(12, 14)));
    }


    [Fact]
    public void Calculate_AppliesEveningDiscount_WhenCrossingIntoEvening()
    {
        Assert.Equal(5200m, RoomChargeFor(Period(17, 20)));
    }

    [Fact]
    public void Calculate_AppliesMorningDiscount_WhenStartingEarly()
    {
        Assert.Equal(3800m, RoomChargeFor(Period(8, 10)));
    }

    [Fact]
    public void Calculate_AddsAmenitiesCharge_OnceRegardlessOfDuration()
    {
        var amenities = new List<Amenity>
    {
        new("Wi-Fi", 100m),
        new("Проєктор", 200m)
    };

        var shortBooking = _policy.Calculate(CreateRoom(), Period(10, 12), amenities);
        var longBooking = _policy.Calculate(CreateRoom(), Period(14, 18), amenities);

        Assert.Equal(300m, shortBooking.AmenitiesCharge);
        Assert.Equal(300m, longBooking.AmenitiesCharge);
    }

    [Fact]
    public void Calculate_Throws_WhenOutsideWorkingHours()
    {
        Assert.Throws<PricingException>(() => RoomChargeFor(Period(5, 7)));

    }

    [Fact]
    public void Calculate_Throws_WhenDurationIsNotMultipleOfStep()
    {
        var period = new TimeRange(
            new DateTime(2026, 9, 15, 10, 0, 0),
            new DateTime(2026, 9, 15, 10, 45, 0));

        Assert.Throws<PricingException>(() => RoomChargeFor(period));
    }
}