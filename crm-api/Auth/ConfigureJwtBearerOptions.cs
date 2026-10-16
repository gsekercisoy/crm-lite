using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CrmApi.Auth;

/// <summary>
/// Resolves JWT validation parameters from <see cref="IConfiguration"/> at
/// options-resolve time (fully merged config), instead of capturing
/// WebApplicationBuilder.Configuration at startup. This keeps signing
/// (JwtService) and validation consistent under test-host config overrides.
/// </summary>
public sealed class ConfigureJwtBearerOptions(IConfiguration config)
    : IConfigureNamedOptions<JwtBearerOptions>
{
    public void Configure(string? name, JwtBearerOptions options)
    {
        if (name == JwtBearerDefaults.AuthenticationScheme)
            Configure(options);
    }

    public void Configure(JwtBearerOptions options)
    {
        var key = config["Jwt:Key"] ?? "dev-only-key-change-me-0123456789abcdef-0123456789";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = config["Jwt:Issuer"] ?? "crm-lite",
            ValidAudience = config["Jwt:Audience"] ?? "crm-lite",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
    }
}
