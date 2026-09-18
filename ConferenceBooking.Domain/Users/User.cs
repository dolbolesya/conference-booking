using ConferenceBooking.Domain.Exceptions;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace ConferenceBooking.Domain.Users;

public enum UserRole
{
    Client = 1,
    Admin = 2
}

/// <summary>
/// Користувач системи. Хешування пароля - інфраструктурна деталь,
/// Користувач системи. Хешування пароля c інфраструктурна деталь,
/// тому сюди приходить уже готовий хеш.
/// </summary>
public sealed partial class User
{
    private User() { Email = string.Empty; PasswordHash = string.Empty; FullName = string.Empty; }

    public User(string email, string passwordHash, string fullName, UserRole role)
    {
        Id = Guid.CreateVersion7();
        SetEmail(email);
        SetPasswordHash(passwordHash);
        SetFullName(fullName);
        Role = role;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string FullName { get; private set; }
    public UserRole Role { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    [MemberNotNull(nameof(Email))]
    public void SetEmail(string email)
    {
        if (!EmailPattern().IsMatch(email ?? string.Empty))
            throw new ArgumentException("Некоректна електронна адреса.", nameof(email));

        Email = email!.Trim().ToLowerInvariant();
    }

    [MemberNotNull(nameof(PasswordHash))]
    public void SetPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Хеш пароля не може бути порожнім.", nameof(passwordHash));

        PasswordHash = passwordHash;
    }

    [MemberNotNull(nameof(FullName))]
    public void SetFullName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Ім'я користувача обов'язкове.", nameof(fullName));

        FullName = fullName.Trim();
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailPattern();
}