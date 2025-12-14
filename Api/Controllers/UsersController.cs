using Api.Auth;
using Api.Data;
using Api.Dtos;
using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthConstants.AdminPolicy)]
public class UsersController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
    {
        var users = await db.Users
            .AsNoTracking()
            .OrderBy(u => u.UserName)
            .Select(u => new UserDto(u.Id, u.UserName, u.IsAdmin, u.CreatedAt))
            .ToListAsync();

        return Ok(users);
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserRequest request)
    {
        var userName = (request.UserName ?? string.Empty).Trim();
        var password = request.Password ?? string.Empty;

        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
        {
            return ValidationProblem("UserName and password are required.");
        }

        if (password.Length < 6)
        {
            return ValidationProblem("Password must be at least 6 characters.");
        }

        var normalized = userName.ToLowerInvariant();
        var exists = await db.Users.AnyAsync(u => u.NormalizedUserName == normalized);
        if (exists)
        {
            return Conflict(new ProblemDetails
            {
                Title = "A user with that username already exists.",
                Status = StatusCodes.Status409Conflict
            });
        }

        var user = new User
        {
            UserName = userName,
            NormalizedUserName = normalized,
            IsAdmin = request.IsAdmin,
            PasswordHash = string.Empty
        };

        var hasher = new PasswordHasher<User>();
        user.PasswordHash = hasher.HashPassword(user, password);

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return Ok(new UserDto(user.Id, user.UserName, user.IsAdmin, user.CreatedAt));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<UserDto>> Update(int id, [FromBody] UpdateUserRequest request)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        var userName = (request.UserName ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(userName))
        {
            return ValidationProblem("UserName is required.");
        }

        var normalized = userName.ToLowerInvariant();
        var exists = await db.Users.AnyAsync(u => u.Id != id && u.NormalizedUserName == normalized);
        if (exists)
        {
            return Conflict(new ProblemDetails
            {
                Title = "A user with that username already exists.",
                Status = StatusCodes.Status409Conflict
            });
        }

        user.UserName = userName;
        user.NormalizedUserName = normalized;
        user.IsAdmin = request.IsAdmin;

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            if (request.Password.Length < 6)
            {
                return ValidationProblem("Password must be at least 6 characters.");
            }
            var hasher = new PasswordHasher<User>();
            user.PasswordHash = hasher.HashPassword(user, request.Password);
        }

        await db.SaveChangesAsync();

        return Ok(new UserDto(user.Id, user.UserName, user.IsAdmin, user.CreatedAt));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        var currentUserIdClaim = User.FindFirst(AuthConstants.ClaimUserId)?.Value;
        if (int.TryParse(currentUserIdClaim, out var currentUserId) && currentUserId == id)
        {
            return Conflict(new ProblemDetails
            {
                Title = "You cannot delete your own user while logged in.",
                Status = StatusCodes.Status409Conflict
            });
        }

        if (user.IsAdmin)
        {
            var adminCount = await db.Users.CountAsync(u => u.IsAdmin);
            if (adminCount <= 1)
            {
                return Conflict(new ProblemDetails
                {
                    Title = "You cannot delete the last admin user.",
                    Status = StatusCodes.Status409Conflict
                });
            }
        }

        db.Users.Remove(user);
        await db.SaveChangesAsync();
        return NoContent();
    }
}

