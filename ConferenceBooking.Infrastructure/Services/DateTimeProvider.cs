using ConferenceBooking.Application.Abstractions;

namespace ConferenceBooking.Infrastructure.Services;

/// <summary>
/// Бізнес працює за київським часом незалежно від того, у якому поясі стоїть сервер.
/// </summary>
public sealed class DateTimeProvider : IDateTimeProvider
{
    private static readonly TimeZoneInfo BusinessTimeZone = GetBusinessTimeZone();

    public DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, BusinessTimeZone);

    public DateTime UtcNow => DateTime.UtcNow;

    private static TimeZoneInfo GetBusinessTimeZone()
    {
        // Windows і Linux використовують різні ідентифікатори часових поясів.
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Europe/Kyiv");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time");
        }
    }
}