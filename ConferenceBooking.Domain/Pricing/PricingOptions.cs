namespace ConferenceBooking.Domain.Pricing;

/// <summary>
/// Правила ціноутворення винесені в конфігурацію: бізнес змінює тарифи
/// без правок коду й без релізу.
/// </summary>
public sealed class PricingOptions
{
    public const string SectionName = "Pricing";

    public TimeOnly OpensAt { get; set; } = new(6, 0);
    public TimeOnly ClosesAt { get; set; } = new(23, 0);

    public int MinimumBookingMinutes { get; set; } = 30;
    public int BookingStepMinutes { get; set; } = 30;

    public List<RateWindow> RateWindows { get; set; } =
    [
        new("Ранкові години",    new TimeOnly(6, 0),  new TimeOnly(9, 0),  0.90m, Priority: 1),
        new("Стандартні години", new TimeOnly(9, 0),  new TimeOnly(18, 0), 1.00m, Priority: 1),
        new("Пікові години",     new TimeOnly(12, 0), new TimeOnly(14, 0), 1.15m, Priority: 2),
        new("Вечірні години",    new TimeOnly(18, 0), new TimeOnly(23, 0), 0.80m, Priority: 1)
    ];
}