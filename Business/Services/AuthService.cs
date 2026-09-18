using HelpdeskSystem.Data;
using HelpdeskSystem.DTOs;
using HelpdeskSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HelpdeskSystem.Business.Services;

public interface IAuthService
{
    Task<User?> RegisterAsync(RegisterRequest request);
    Task<(User? User, string? Token)> LoginAsync(LoginRequest request);
    string GenerateJwt(User user);
}

public class AuthService(AppDbContext db, IPasswordHasher<User> hasher, IConfiguration config) : IAuthService
{
    public async Task<User?> RegisterAsync(RegisterRequest request)
    {
        if (await db.Users.AnyAsync(x => x.Email == request.Email.Trim().ToLower()))
            return null;

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim().ToLower(),
            Role = UserRole.User
        };

        user.PasswordHash = hasher.HashPassword(user, request.Password);
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    public async Task<(User? User, string? Token)> LoginAsync(LoginRequest request)
    {
        var user = await db.Users.SingleOrDefaultAsync(x => x.Email == request.Email.Trim().ToLower());
        if (user is null)
            return (null, null);

        if (hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
            return (null, null);

        return (user, GenerateJwt(user));
    }

    public string GenerateJwt(User user)
    {
        var jwtSection = config.GetSection("Jwt");
        var key = System.Text.Encoding.UTF8.GetBytes(jwtSection["Key"]!);

        var claims = new[]
        {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id.ToString()),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, user.FullName),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, user.Role.ToString())
        };

        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            jwtSection["Issuer"],
            jwtSection["Audience"],
            claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSection["ExpiresMinutes"]!)),
            signingCredentials: new Microsoft.IdentityModel.Tokens.SigningCredentials(
                new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
                Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256));

        return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
    }
}
