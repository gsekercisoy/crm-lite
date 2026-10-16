using CrmApi.Auth;
using CrmApi.Data;
using CrmApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AppDbContext db, JwtService jwt) : ControllerBase
{
    public record RegisterRequest(string Email, string Password, string? Role);
    public record LoginRequest(string Email, string Password);

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest(new { error = "Email and password are required." });
        var role = string.IsNullOrWhiteSpace(req.Role) ? "Sales" : req.Role;
        if (role is not ("Admin" or "Sales"))
            return BadRequest(new { error = "Role must be Admin or Sales." });
        var email = req.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(u => u.Email == email))
            return Conflict(new { error = "Email already registered." });

        var salt = PasswordHasher.GenerateSalt();
        var user = new User
        {
            Email = email,
            PasswordSalt = salt,
            PasswordHash = PasswordHasher.Hash(req.Password, salt),
            Role = role
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return Ok(new { token = jwt.CreateToken(user), email = user.Email, role = user.Role });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest req)
    {
        var email = (req.Email ?? "").Trim().ToLowerInvariant();
        var user = await db.Users.SingleOrDefaultAsync(u => u.Email == email);
        if (user is null || !PasswordHasher.Verify(req.Password ?? "", user.PasswordSalt, user.PasswordHash))
            return Unauthorized(new { error = "Invalid credentials." });
        return Ok(new { token = jwt.CreateToken(user), email = user.Email, role = user.Role });
    }
}
