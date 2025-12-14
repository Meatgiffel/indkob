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
public class GroceryEntriesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GroceryEntryDto>>> GetAll()
    {
        var entries = await db.GroceryEntries
            .AsNoTracking()
            .OrderBy(e => e.IsDone)
            .ThenBy(e => e.Item!.Area)
            .ThenBy(e => e.Item!.Name)
            .Select(e => new GroceryEntryDto(
                e.Id,
                e.ItemId,
                e.Item!.Name,
                e.Item.Area,
                e.Amount,
                e.Note,
                e.IsDone,
                e.CreatedAt))
            .ToListAsync();

        return Ok(entries);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<GroceryEntryDto>> GetById(int id)
    {
        var entry = await db.GroceryEntries
            .AsNoTracking()
            .Where(e => e.Id == id)
            .Select(e => new GroceryEntryDto(
                e.Id,
                e.ItemId,
                e.Item!.Name,
                e.Item.Area,
                e.Amount,
                e.Note,
                e.IsDone,
                e.CreatedAt))
            .FirstOrDefaultAsync();

        return entry is null ? NotFound() : Ok(entry);
    }

    [HttpPost]
    public async Task<ActionResult<GroceryEntryDto>> Create([FromBody] CreateGroceryEntryRequest request)
    {
        var item = await db.Items.FindAsync(request.ItemId);
        if (item is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Item not found.",
                Status = StatusCodes.Status404NotFound
            });
        }

        var entry = new GroceryEntry
        {
            ItemId = item.Id,
            Amount = request.Amount?.Trim(),
            Note = request.Note?.Trim(),
            IsDone = false
        };

        db.GroceryEntries.Add(entry);
        await db.SaveChangesAsync();

        var dto = new GroceryEntryDto(entry.Id, item.Id, item.Name, item.Area, entry.Amount, entry.Note, entry.IsDone, entry.CreatedAt);
        return CreatedAtAction(nameof(GetById), new { id = entry.Id }, dto);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<GroceryEntryDto>> Update(int id, [FromBody] UpdateGroceryEntryRequest request)
    {
        var entry = await db.GroceryEntries.FindAsync(id);
        if (entry is null)
        {
            return NotFound();
        }

        var item = await db.Items.FindAsync(request.ItemId);
        if (item is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Item not found.",
                Status = StatusCodes.Status404NotFound
            });
        }

        entry.ItemId = item.Id;
        entry.Amount = request.Amount?.Trim();
        entry.Note = request.Note?.Trim();
        entry.IsDone = request.IsDone;

        await db.SaveChangesAsync();

        var dto = new GroceryEntryDto(entry.Id, item.Id, item.Name, item.Area, entry.Amount, entry.Note, entry.IsDone, entry.CreatedAt);
        return Ok(dto);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entry = await db.GroceryEntries.FindAsync(id);
        if (entry is null)
        {
            return NotFound();
        }

        db.GroceryEntries.Remove(entry);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
