using Api.Data;
using Api.Dtos;
using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

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
            .Select(e => new GroceryEntryDto(
                e.Id,
                e.ItemId,
                e.Item != null ? e.Item.Name : null,
                e.Item != null ? e.Item.Area : null,
                e.Amount,
                e.Note,
                e.IsDone,
                e.CreatedAt))
            .ToListAsync();

        var comparer = StringComparer.Create(new CultureInfo("da-DK"), ignoreCase: true);
        var ordered = entries
            .OrderBy(e => e.IsDone)
            .ThenBy(e => e.ItemId is null ? "Noter" : (e.ItemArea ?? "Andet"), comparer)
            .ThenBy(e => e.ItemId is null ? (e.Note ?? string.Empty) : (e.ItemName ?? string.Empty), comparer)
            .ToList();

        return Ok(ordered);
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
                e.Item != null ? e.Item.Name : null,
                e.Item != null ? e.Item.Area : null,
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
        var amount = request.Amount?.Trim();
        var note = request.Note?.Trim();

        if (request.ItemId is null)
        {
            if (string.IsNullOrWhiteSpace(note))
            {
                return ValidationProblem("Either an item must be selected or a note must be provided.");
            }

            var noteEntry = new GroceryEntry
            {
                ItemId = null,
                Amount = string.IsNullOrWhiteSpace(amount) ? null : amount,
                Note = note,
                IsDone = false
            };

            db.GroceryEntries.Add(noteEntry);
            await db.SaveChangesAsync();

            var noteDto = new GroceryEntryDto(noteEntry.Id, null, null, null, noteEntry.Amount, noteEntry.Note, noteEntry.IsDone, noteEntry.CreatedAt);
            return CreatedAtAction(nameof(GetById), new { id = noteEntry.Id }, noteDto);
        }

        var item = await db.Items.FindAsync(request.ItemId.Value);
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
            Amount = string.IsNullOrWhiteSpace(amount) ? null : amount,
            Note = string.IsNullOrWhiteSpace(note) ? null : note,
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

        var amount = request.Amount?.Trim();
        var note = request.Note?.Trim();

        if (request.ItemId is null)
        {
            if (string.IsNullOrWhiteSpace(note))
            {
                return ValidationProblem("A note is required when no item is selected.");
            }

            entry.ItemId = null;
            entry.Amount = string.IsNullOrWhiteSpace(amount) ? null : amount;
            entry.Note = note;
            entry.IsDone = request.IsDone;

            await db.SaveChangesAsync();

            var noteDto = new GroceryEntryDto(entry.Id, null, null, null, entry.Amount, entry.Note, entry.IsDone, entry.CreatedAt);
            return Ok(noteDto);
        }

        var item = await db.Items.FindAsync(request.ItemId.Value);
        if (item is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Item not found.",
                Status = StatusCodes.Status404NotFound
            });
        }

        entry.ItemId = item.Id;
        entry.Amount = string.IsNullOrWhiteSpace(amount) ? null : amount;
        entry.Note = string.IsNullOrWhiteSpace(note) ? null : note;
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
