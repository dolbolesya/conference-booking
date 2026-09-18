using ConferenceBooking.Application.Abstractions;
using ConferenceBooking.Domain.Exceptions;
using ConferenceBooking.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace ConferenceBooking.Application.Auth;

public sealed class AuthService : IAuthService
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtService _jwt;

    public AuthService(IApplicationDbContext db, IPasswordHasher hasher, IJwtService jwt)
    {
        _db = db;
        _hasher = hasher;
        _jwt = jwt;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await _db.Users.AnyAsync(u => u.Email == email, ct))
            throw new DomainException("email_taken", "Користувач з такою електронною адресою вже існує.");

        var user = new User(email, _hasher.Hash(request.Password), request.FullName, UserRole.Client);

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        return BuildResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

        // Однакова помилка для неіснуючої пошти й невірного пароля: різні відповіді
        // дозволили б зловмиснику перебором з'ясувати, хто зареєстрований у системі.
        if (user is null || !_hasher.Verify(request.Password, user.PasswordHash))
            throw new DomainException("invalid_credentials", "Невірна електронна адреса або пароль.");

        return BuildResponse(user);
    }

    private AuthResponse BuildResponse(User user)
    {
        var (token, expiresIn) = _jwt.GenerateToken(user);

        return new AuthResponse(token, "Bearer", expiresIn, user.Id, user.Email, user.FullName, user.Role.ToString());
    }
}