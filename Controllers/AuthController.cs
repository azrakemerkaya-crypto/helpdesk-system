using System.Security.Claims;
using HelpdeskSystem.Business.Services;
using HelpdeskSystem.Data;
using HelpdeskSystem.DTOs;
using HelpdeskSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HelpdeskSystem.Controllers;

[ApiController, Route("api/auth")]
public class AuthController(AppDbContext db, IPasswordHasher<User> hasher, IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var user = await authService.RegisterAsync(request);
        if (user is null)
            return Conflict("Bu e-posta zaten kayıtlı.");

        return Ok(new { user.Id, user.FullName, user.Email, role = user.Role.ToString() });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await authService.LoginAsync(request);
        if (result.User is null || result.Token is null)
            return Unauthorized("E-posta veya şifre hatalı.");

        return Ok(new
        {
            token = result.Token,
            user = new { result.User.Id, result.User.FullName, result.User.Email, role = result.User.Role.ToString() }
        });
    }
}
