using Api.Data;
using Api.Dtos;
using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MealPlanController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MealPlanDayDto>>> GetWeek([FromQuery] DateOnly weekStart)
    {
        var start = weekStart;
        var end = weekStart.AddDays(6);

        var existing = await db.MealPlanDays
            .AsNoTracking()
            .Where(d => d.Date >= start && d.Date <= end)
            .ToDictionaryAsync(d => d.Date, d => d);

        var days = Enumerable.Range(0, 7)
            .Select(i =>
            {
                var date = start.AddDays(i);
                existing.TryGetValue(date, out var day);
                return new MealPlanDayDto(date, day?.Dinner, day?.RecipeSlug, day?.RecipeName, day?.RecipeId);
            })
            .ToList();

        return Ok(days);
    }

    [HttpPut("{date}")]
    public async Task<ActionResult<MealPlanDayDto>> Upsert([FromRoute] DateOnly date, [FromBody] UpdateMealPlanDayRequest request)
    {
        var dinner = request.Dinner?.Trim();
        var recipeSlug = string.IsNullOrWhiteSpace(request.RecipeSlug) ? null : request.RecipeSlug.Trim();
        var recipeName = string.IsNullOrWhiteSpace(request.RecipeName) ? null : request.RecipeName.Trim();
        var recipeId = string.IsNullOrWhiteSpace(request.RecipeId) ? null : request.RecipeId.Trim();

        // An empty dinner with no recipe link means "clear the day".
        if (string.IsNullOrWhiteSpace(dinner) && recipeSlug is null)
        {
            var existing = await db.MealPlanDays.FirstOrDefaultAsync(d => d.Date == date);
            if (existing is null)
            {
                return Ok(new MealPlanDayDto(date, null, null, null, null));
            }

            db.MealPlanDays.Remove(existing);
            await db.SaveChangesAsync();
            return Ok(new MealPlanDayDto(date, null, null, null, null));
        }

        if (dinner is { Length: > 256 })
        {
            return ValidationProblem("Dinner must be 256 characters or less.");
        }

        var day = await db.MealPlanDays.FirstOrDefaultAsync(d => d.Date == date);
        if (day is null)
        {
            day = new MealPlanDay
            {
                Date = date,
                Dinner = dinner,
                RecipeSlug = recipeSlug,
                RecipeName = recipeName,
                RecipeId = recipeId,
                UpdatedAt = DateTime.UtcNow
            };
            db.MealPlanDays.Add(day);
        }
        else
        {
            day.Dinner = dinner;
            day.RecipeSlug = recipeSlug;
            day.RecipeName = recipeName;
            day.RecipeId = recipeId;
            day.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
        return Ok(new MealPlanDayDto(date, day.Dinner, day.RecipeSlug, day.RecipeName, day.RecipeId));
    }
}
