using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CrmApi.Models;
using Microsoft.IdentityModel.Tokens;

namespace CrmApi.Auth;

public class JwtService(IConfiguration config)
{
    public string CreateToken(User user)
    {
        var key = config["Jwt:Key"] ?? "dev-only-key-change-me-0123456789abcdef";
        var issuer = config["Jwt:Issuer"] ?? "crm-lite";
        var audience = config["Jwt:Audience"] ?? "crm-lite";
        var hours = double.TryParse(config["Jwt:ExpireHours"], out var h) ? h : 12;

        var creds = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        };

        var token = new JwtSecurityToken(issuer, audience, claims,
            expires: DateTime.UtcNow.AddHours(hours), signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
