using ConferenceBooking.Domain.Common;
using ConferenceBooking.Domain.Rooms;

namespace ConferenceBooking.Domain.Pricing;

/// <summary>
/// Розбиває бронювання на відрізки за межами тарифних вікон і тарифікує кожен окремо.
/// 10:00–14:00 = 2 год стандартних (×1.00) + 2 год пікових (×1.15), а не 4 год за одним тарифом.
/// </summary>
public sealed class TimeBasedPricingPolicy : IPricingPolicy
{
    private readonly PricingOptions _options;

    public TimeBasedPricingPolicy(PricingOptions options) => _options = options;

    public PriceBreakdown Calculate(Room room, TimeRange period, IReadOnlyCollection<Amenity> amenities)
    {
        ArgumentNullException.ThrowIfNull(room);
        ArgumentNullException.ThrowIfNull(amenities);

        EnsureWithinWorkingHours(period);
        EnsureAllowedDuration(period);

        var segments = BuildSegments(period)
            .Select(segment => ToPriceSegment(room.BasePricePerHour, segment))
            .ToList();

        var charges = amenities
            .Select(a => new AmenityCharge(a.Id, a.Name, a.Price))
            .ToList();

        return new PriceBreakdown(period, room.BasePricePerHour, segments, charges);
    }

    private void EnsureWithinWorkingHours(TimeRange period)
    {
        if (!period.IsWithinSingleDay)
            throw new ArgumentException("Бронювання не може переходити через добу.");

        var start = TimeOnly.FromDateTime(period.Start);
        var end = TimeOnly.FromDateTime(period.End);

        if (start < _options.OpensAt || end > _options.ClosesAt)
            throw new ArgumentException($"Зали доступні з {_options.OpensAt:HH\\:mm} до {_options.ClosesAt:HH\\:mm}.");
    }

    private void EnsureAllowedDuration(TimeRange period)
    {
        var minutes = (int)period.Duration.TotalMinutes;

        if (minutes < _options.MinimumBookingMinutes)
            throw new ArgumentException($"Мінімальна тривалість — {_options.MinimumBookingMinutes} хв.");

        if (minutes % _options.BookingStepMinutes != 0)
            throw new ArgumentException($"Тривалість має бути кратною {_options.BookingStepMinutes} хв.");
    }

    private IEnumerable<TimeRange> BuildSegments(TimeRange period)
    {
        var day = period.Start.Date;
        var boundaries = new SortedSet<DateTime> { period.Start, period.End };

        foreach (var window in _options.RateWindows)
        {
            var windowStart = day + window.Start.ToTimeSpan();
            var windowEnd = day + window.End.ToTimeSpan();

            if (windowStart > period.Start && windowStart < period.End)
                boundaries.Add(windowStart);

            if (windowEnd > period.Start && windowEnd < period.End)
                boundaries.Add(windowEnd);
        }

        var points = boundaries.ToArray();

        for (var i = 0; i < points.Length - 1; i++)
            yield return new TimeRange(points[i], points[i + 1]);
    }


    private RateWindow ResolveWindow(TimeRange segment)
    {
        var midpoint = TimeOnly.FromDateTime(segment.Start.AddTicks(segment.Duration.Ticks / 2));

        return _options.RateWindows
                   .Where(w => w.Contains(midpoint))
                   .OrderByDescending(w => w.Priority)
                   .FirstOrDefault()
               ?? throw new ArgumentException($"Для часу {midpoint:HH\\:mm} не налаштовано тариф.");
    }


    private PriceSegment ToPriceSegment(decimal basePricePerHour, TimeRange segment)
    {
        var window = ResolveWindow(segment);
        var hours = (decimal)segment.Duration.TotalHours;
        var amount = decimal.Round(basePricePerHour * hours * window.Multiplier, 2, MidpointRounding.AwayFromZero);
        return new PriceSegment(window.Name, segment.Start, segment.End, hours, window.Multiplier, amount);
    }
}