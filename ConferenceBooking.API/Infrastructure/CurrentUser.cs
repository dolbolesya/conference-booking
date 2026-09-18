using ConferenceBooking.Application.Abstractions;
using System.Security.Claims;

namespace ConferenceBooking.API.Infrastructure;

public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUser(IHttpContextAccessor accessor) => _accessor = accessor;

    public Guid? UserId =>
        Guid.TryParse(_accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : null;

    public bool IsAdmin => _accessor.HttpContext?.User.IsInRole("Admin") ?? false;
}