namespace ConferenceBooking.Domain.Bookings;

public enum BookingStatus
{
    /// <summary>Активне бронювання — блокує зал на свій інтервал.</summary>
    Confirmed = 1,

    /// <summary>Скасоване — звільняє інтервал, але залишається у звітності.</summary>
    Cancelled = 2
}