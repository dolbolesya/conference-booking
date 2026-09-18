using ConferenceBooking.Application.Abstractions;

namespace ConferenceBooking.Infrastructure.Services;

/// <summary>
/// BCrypt: сіль генерується автоматично і зберігається всередині хешу,
/// а work factor робить перебір дорогим навіть при витоку бази.
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public bool Verify(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
}