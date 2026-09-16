namespace ConferenceBooking.Domain.Common;

/// <summary>
/// Проміжок часу [Start, End) — кінець не включається.
/// Завдяки цьому бронювання 10:00–12:00 і 12:00–14:00 не конфліктують.
/// </summary>
public readonly record struct TimeRange
{
    public DateTime Start { get; }
    public DateTime End { get; }

    public TimeRange(DateTime start, DateTime end)
    {
        if (end <= start)
            throw new ArgumentException("Кінець інтервалу має бути пізніше за початок.", nameof(end));

        Start = start;
        End = end;
    }

    public TimeSpan Duration => End - Start;

    public decimal TotalHours => (decimal)Duration.TotalHours;

    /// <summary>
    /// Перетин є, якщо кожен інтервал починається раніше, ніж закінчується інший.
    /// Знаки строгі: дотик межами (12:00 = 12:00) перетином не вважається.
    /// </summary>
    public bool Overlaps(TimeRange other) => Start < other.End && End > other.Start;

    /// <summary>
    /// AddTicks(-1) бере останню мить, яка реально зайнята. Без цього
    /// бронювання 18:00–00:00 помилково вважалося б таким, що переходить добу.
    /// </summary>
    public bool IsWithinSingleDay => Start.Date == End.AddTicks(-1).Date;

    public override string ToString() => $"{Start:dd.MM.yyyy HH:mm}–{End:HH:mm}";

    public static TimeRange FromDuration(DateTime start, TimeSpan duration) => new(start, start + duration);
}