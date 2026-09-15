namespace ConferenceBooking.Domain.Pricing;

/// <summary>
/// Тарифне вікно: проміжок доби [Start, End) з множником до базової ставки.
/// Priority розв'язує накладання: пікові 12:00–14:00 лежать усередині стандартних 09:00–18:00.
/// </summary>
public sealed record RateWindow(string Name, TimeOnly Start, TimeOnly End, decimal Multiplier, int Priority)
{
    public bool Contains(TimeOnly time) => time >= Start && time < End;
}