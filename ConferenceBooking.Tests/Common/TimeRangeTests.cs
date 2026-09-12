using ConferenceBooking.Domain.Common;

namespace ConferenceBooking.Tests.Common;

public class TimeRangeTests
{
    private static DateTime At(int hour, int minute = 0) => new(2026, 9, 15, hour, minute, 0);

    [Fact]
    public void Constructor_Throws_WhenEndEqualsStart()
    {
        Assert.Throws<ArgumentException>(() => new TimeRange(At(10), At(10)));
    }

    [Fact]
    public void Overlaps_ReturnsFalse_WhenRangesTouchAtBoundary()
    {
        var morning = new TimeRange(At(10), At(12));
        var afternoon = new TimeRange(At(12), At(14));

        Assert.False(morning.Overlaps(afternoon));
        Assert.False(afternoon.Overlaps(morning));
    }

    [Fact]
    public void Constructor_Throws_WhenEndIsBeforeStart()
    {
        Assert.Throws<ArgumentException>(() => new TimeRange(At(12), At(10)));
    }

    [Fact]
    public void Overlaps_ReturnsTrue_WhenRangesPartiallyIntersect()
    {
        var first = new TimeRange(At(10), At(13));
        var second = new TimeRange(At(12), At(14));

        Assert.True(first.Overlaps(second));
        Assert.True(second.Overlaps(first));
    }

    [Fact]
    public void Overlaps_ReturnsFalse_WhenRangesAreApart()
    {
        var first = new TimeRange(At(10), At(11));
        var second = new TimeRange(At(15), At(16));

        Assert.False(first.Overlaps(second));
        Assert.False(second.Overlaps(first));
    }

    [Fact]
    public void Overlaps_ReturnsTrue_WhenOneRangeContainsAnother()
    {
        var outer = new TimeRange(At(10), At(18));
        var inner = new TimeRange(At(12), At(14));

        Assert.True(outer.Overlaps(inner));
        Assert.True(inner.Overlaps(outer));
    }

    [Fact]
    public void TotalHours_ReturnsCorrectValue()
    {
        var range = new TimeRange(At(10), At(11, 30));

        Assert.Equal(1.5m, range.TotalHours);
    }

    [Fact]
    public void IsWithinSingleDay_ReturnsTrue_WhenRangeEndsAtMidnight()
    {
        var range = new TimeRange(At(22), new DateTime(2026, 9, 16, 0, 0, 0));

        Assert.True(range.IsWithinSingleDay);
    }
}