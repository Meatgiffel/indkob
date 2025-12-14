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
            .ToDictionaryAsync(d => d.Date, d => d.Dinner);

        var days = Enumerable.Range(0, 7)
            .Select(i =>
            {
                var date = start.AddDays(i);
                existing.TryGetValue(date, out var dinner);
                return new MealPlanDayDto(date, dinner);
            })
            .ToList();

        return Ok(days);
    }

    [HttpPut("{date}")]
    public async Task<ActionResult<MealPlanDayDto>> Upsert([FromRoute] DateOnly date, [FromBody] UpdateMealPlanDayRequest request)
    {
        var dinner = request.Dinner?.Trim();
        if (string.IsNullOrWhiteSpace(dinner))
        {
            var existing = await db.MealPlanDays.FirstOrDefaultAsync(d => d.Date == date);
            if (existing is null)
            {
                return Ok(new MealPlanDayDto(date, null));
            }

            db.MealPlanDays.Remove(existing);
            await db.SaveChangesAsync();
            return Ok(new MealPlanDayDto(date, null));
        }

        if (dinner.Length > 256)
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
                UpdatedAt = DateTime.UtcNow
            };
            db.MealPlanDays.Add(day);
        }
        else
        {
            day.Dinner = dinner;
            day.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
        return Ok(new MealPlanDayDto(date, dinner));
    }
}

