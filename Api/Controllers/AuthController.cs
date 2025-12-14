using Api.Auth;
using Api.Data;
using Api.Dtos;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AppDbContext db) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthUserDto>> Login([FromBody] LoginRequest request)
    {
        var userName = (request.UserName ?? string.Empty).Trim();
        var password = request.Password ?? string.Empty;

        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrEmpty(password))
        {
            return Unauthorized();
        }

        var normalized = userName.ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(u => u.NormalizedUserName == normalized);
        if (user is null)
        {
            return Unauthorized();
        }

        var hasher = new PasswordHasher<Models.User>();
        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (result == PasswordVerificationResult.Failed)
        {
            return Unauthorized();
        }

        var identity = AuthConstants.CreateIdentity(user.Id, user.UserName, user.IsAdmin);
        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync(AuthConstants.CookieScheme, principal);

        return Ok(new AuthUserDto(user.Id, user.UserName, user.IsAdmin));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(AuthConstants.CookieScheme);
        return NoContent();
    }

    [HttpGet("me")]
    public ActionResult<AuthUserDto> Me()
    {
        if (!(User?.Identity?.IsAuthenticated ?? false))
        {
            return Unauthorized();
        }

        var idClaim = User.FindFirstValue(AuthConstants.ClaimUserId);
        var name = User.Identity?.Name;
        var isAdminClaim = User.FindFirstValue(AuthConstants.ClaimIsAdmin);

        if (!int.TryParse(idClaim, out var id) || string.IsNullOrWhiteSpace(name))
        {
            return Unauthorized();
        }

        var isAdmin = string.Equals(isAdminClaim, "true", StringComparison.OrdinalIgnoreCase);
        return Ok(new AuthUserDto(id, name, isAdmin));
    }
}

