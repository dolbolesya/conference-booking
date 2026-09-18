namespace ConferenceBooking.Application.Abstractions;

/// <summary>
/// Поточний користувач із токена. Прикладний шар не знає про HttpContext.
/// </summary>
public interface ICurrentUser
{
    Guid? UserId { get; }
    bool IsAdmin { get; }
}