using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HelpdeskSystem.Data;
using HelpdeskSystem.DTOs;
using HelpdeskSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace HelpdeskSystem.Controllers;

[ApiController, Route("api/auth")]
public class AuthController(AppDbContext db, IPasswordHasher<User> hasher, IConfiguration config) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (await db.Users.AnyAsync(x => x.Email == request.Email.ToLower())) return Conflict("Bu e-posta zaten kayıtlı.");
        var user = new User { FullName = request.FullName, Email = request.Email.ToLower() };
        user.PasswordHash = hasher.HashPassword(user, request.Password);
        db.Users.Add(user); await db.SaveChangesAsync();
        return Ok(new { user.Id, user.FullName, user.Email, role = user.Role.ToString() });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await db.Users.SingleOrDefaultAsync(x => x.Email == request.Email.ToLower());
        if (user is null || hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed) return Unauthorized("E-posta veya şifre hatalı.");
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), new Claim(ClaimTypes.Name, user.FullName), new Claim(ClaimTypes.Role, user.Role.ToString()) };
        var section = config.GetSection("Jwt");
        var token = new JwtSecurityToken(section["Issuer"], section["Audience"], claims, expires: DateTime.UtcNow.AddMinutes(double.Parse(section["ExpiresMinutes"]!)), signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(section["Key"]!)), SecurityAlgorithms.HmacSha256));
        return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token), user = new { user.Id, user.FullName, user.Email, role = user.Role.ToString() } });
    }
}
