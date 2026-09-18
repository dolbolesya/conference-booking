using ConferenceBooking.Domain.Users;


namespace ConferenceBooking.Application.Abstractions;

public interface IJwtService
{
    /// <summary>Повертає підписаний токен і час його життя в секундах.</summary>
    (string Token, int ExpiresIn) GenerateToken(User user);
}