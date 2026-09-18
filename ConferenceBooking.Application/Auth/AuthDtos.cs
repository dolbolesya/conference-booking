namespace ConferenceBooking.Application.Auth;

public sealed record RegisterRequest(string Email, string Password, string FullName);

public sealed record LoginRequest(string Email, string Password);

public sealed record AuthResponse(
    string AccessToken,
    string TokenType,
    int ExpiresIn,
    Guid UserId,
    string Email,
    string FullName,
    string Role);