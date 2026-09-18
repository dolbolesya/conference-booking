namespace ConferenceBooking.Application.Abstractions;

/// <summary>
/// Хешування паролів — інфраструктурна деталь. Абстракція дозволяє змінити
/// алгоритм без правок домену та прикладного шару.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}