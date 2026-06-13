using Api.Data;
using Api.Dtos;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GroceryEntriesController(AppDbContext db, IGroceryChangeNotifier groceryChangeNotifier) : ControllerBase
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
            await groceryChangeNotifier.NotifyCreatedAsync(noteEntry.Id);

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
        await groceryChangeNotifier.NotifyCreatedAsync(entry.Id);

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
            await groceryChangeNotifier.NotifyUpdatedAsync(entry.Id);

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
        await groceryChangeNotifier.NotifyUpdatedAsync(entry.Id);

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

        var entryId = entry.Id;
        db.GroceryEntries.Remove(entry);
        await db.SaveChangesAsync();
        await groceryChangeNotifier.NotifyDeletedAsync(entryId);
        return NoContent();
    }

    [HttpPost("clear")]
    public async Task<IActionResult> Clear()
    {
        await db.GroceryEntries.ExecuteDeleteAsync();
        await groceryChangeNotifier.NotifyClearedAsync();
        return NoContent();
    }

    [HttpPost("from-recipe")]
    public async Task<ActionResult<IEnumerable<GroceryEntryDto>>> AddFromRecipe([FromBody] AddFromRecipeRequest request)
    {
        if (request.Ingredients.Count == 0)
        {
            return ValidationProblem("At least one ingredient is required.");
        }

        var source = string.IsNullOrWhiteSpace(request.Source) ? null : request.Source.Trim();
        if (source is { Length: > 256 })
        {
            source = source[..256];
        }

        var created = new List<GroceryEntry>();

        foreach (var ing in request.Ingredients)
        {
            var name = ing.Name?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            var amount = string.IsNullOrWhiteSpace(ing.Amount) ? null : ing.Amount.Trim();

            int itemId;
            if (ing.ItemId is int existingId)
            {
                var existing = await db.Items.FindAsync(existingId);
                if (existing is null)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = $"Item {existingId} not found.",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                itemId = existing.Id;
            }
            else
            {
                var area = ing.Area?.Trim();
                if (string.IsNullOrWhiteSpace(area))
                {
                    return ValidationProblem($"An area is required to create the new item '{name}'.");
                }

                var normalizedName = name.ToLower();
                var item = await db.Items.FirstOrDefaultAsync(i => i.Name.ToLower() == normalizedName);
                if (item is null)
                {
                    item = new Item { Name = name, Area = area };
                    db.Items.Add(item);
                    await db.SaveChangesAsync();
                }

                itemId = item.Id;
            }

            var entry = new GroceryEntry
            {
                ItemId = itemId,
                Amount = amount,
                Note = source,
                IsDone = false
            };
            db.GroceryEntries.Add(entry);
            created.Add(entry);
        }

        if (created.Count == 0)
        {
            return ValidationProblem("No valid ingredients to add.");
        }

        await db.SaveChangesAsync();

        foreach (var entry in created)
        {
            await groceryChangeNotifier.NotifyCreatedAsync(entry.Id);
        }

        var ids = created.Select(c => c.Id).ToList();
        var dtos = await db.GroceryEntries
            .AsNoTracking()
            .Where(e => ids.Contains(e.Id))
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

        return Ok(dtos);
    }
}
