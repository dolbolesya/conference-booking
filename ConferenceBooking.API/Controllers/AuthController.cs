using ConferenceBooking.API.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceBooking.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly TokenService _tokens;

    public AuthController(TokenService tokens) => _tokens = tokens;

    /// <summary>
    /// Отримання JWT для доступу до API.
    /// Демонстраційні облікові дані: admin/admin-secret, client/client-secret.
    /// </summary>
    [HttpPost("token")]
    public ActionResult<TokenResponse> Token(TokenRequest request)
    {
        var token = _tokens.Issue(request);

        // Не уточнюємо, що саме невірно — ідентифікатор чи секрет:
        // це дало б зловмиснику змогу перебирати клієнтів.
        return token is null
            ? Unauthorized(new ProblemDetails { Title = "Невірні облікові дані", Status = 401 })
            : Ok(token);
    }
}