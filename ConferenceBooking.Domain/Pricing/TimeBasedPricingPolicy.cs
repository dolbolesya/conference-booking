using ConferenceBooking.Domain.Common;
using ConferenceBooking.Domain.Exceptions;
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
            throw new PricingException("Бронювання не може переходити через добу.");

        var start = TimeOnly.FromDateTime(period.Start);
        var end = TimeOnly.FromDateTime(period.End);

        if (start < _options.OpensAt || end > _options.ClosesAt)
            throw new PricingException($"Зали доступні з {_options.OpensAt:HH\\:mm} до {_options.ClosesAt:HH\\:mm}.");
    }

    private void EnsureAllowedDuration(TimeRange period)
    {
        var minutes = (int)period.Duration.TotalMinutes;

        if (minutes < _options.MinimumBookingMinutes)
            throw new PricingException($"Мінімальна тривалість -{_options.MinimumBookingMinutes} хв.");

        if (minutes % _options.BookingStepMinutes != 0)
            throw new PricingException($"Тривалість має бути кратною {_options.BookingStepMinutes} хв.");
    }

    /// <summary>
    /// Ріже інтервал по всіх межах тарифних вікон, що потрапляють усередину.
    /// SortedSet сам сортує точки й прибирає дублікати -інакше на стику вікон
    /// (18:00 -кінець стандартних і початок вечірніх) виник би відрізок нульової довжини.
    /// </summary>
    private IEnumerable<TimeRange> BuildSegments(TimeRange period)
    {
        var day = period.Start.Date;
        var boundaries = new SortedSet<DateTime> { period.Start, period.End };

        foreach (var window in _options.RateWindows)
        {
            AddIfInside(boundaries, day + window.Start.ToTimeSpan(), period);
            AddIfInside(boundaries, day + window.End.ToTimeSpan(), period);
        }

        var points = boundaries.ToArray();

        for (var i = 0; i < points.Length - 1; i++)
            yield return new TimeRange(points[i], points[i + 1]);
    }

    /// <summary>
    /// Тариф визначаємо за серединою відрізка: відрізок за побудовою не перетинає меж,
    /// тож будь-яка внутрішня точка однозначно ідентифікує вікно.
    /// За накладання вікон виграє більший Priority -так пікові години
    /// перебивають стандартні, всередині яких вони лежать.
    /// </summary>
    private RateWindow ResolveWindow(TimeRange segment)
    {
        var midpoint = TimeOnly.FromDateTime(segment.Start.AddTicks(segment.Duration.Ticks / 2));

        return _options.RateWindows
                   .Where(w => w.Contains(midpoint))
                   .OrderByDescending(w => w.Priority)
                   .FirstOrDefault()
               ?? throw new PricingException($"Для часу {midpoint:HH\\:mm} не налаштовано тариф.");
    }

    private PriceSegment ToPriceSegment(decimal basePricePerHour, TimeRange segment)
    {
        var window = ResolveWindow(segment);
        var hours = segment.TotalHours;
        var amount = decimal.Round(basePricePerHour * hours * window.Multiplier, 2, MidpointRounding.AwayFromZero);

        return new PriceSegment(window.Name, segment.Start, segment.End, hours, window.Multiplier, amount);
    }

    private static void AddIfInside(SortedSet<DateTime> boundaries, DateTime candidate, TimeRange period)
    {
        if (candidate > period.Start && candidate < period.End)
            boundaries.Add(candidate);
    }
}