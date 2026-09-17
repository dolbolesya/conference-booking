using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ConferenceBooking.API.Infrastructure;

public sealed record TokenRequest(string ClientId, string ClientSecret);

public sealed record TokenResponse(string AccessToken, string TokenType, int ExpiresIn);

public sealed class TokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration) => _configuration = configuration;

    public TokenResponse? Issue(TokenRequest request)
    {
        var client = _configuration
            .GetSection("DemoClients")
            .Get<List<DemoClient>>()
            ?.FirstOrDefault(c =>
                c.ClientId == request.ClientId &&
                c.ClientSecret == request.ClientSecret);

        if (client is null)
            return null;

        var jwt = _configuration.GetSection("Jwt");
        var expiryMinutes = jwt.GetValue<int>("ExpiryMinutes");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims:
            [
                new Claim(JwtRegisteredClaimNames.Sub, client.ClientId),
                new Claim(ClaimTypes.Role, client.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString())
            ],
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new TokenResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            "Bearer",
            expiryMinutes * 60);
    }

    private sealed record DemoClient(string ClientId, string ClientSecret, string Role);
}