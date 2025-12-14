using Api.Data;
using Api.Dtos;
using Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ItemDto>>> GetAll()
    {
        var items = await db.Items
            .AsNoTracking()
            .OrderBy(i => i.Area)
            .ThenBy(i => i.Name)
            .Select(i => new ItemDto(i.Id, i.Name, i.Area))
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ItemDto>> GetById(int id)
    {
        var item = await db.Items
            .AsNoTracking()
            .Where(i => i.Id == id)
            .Select(i => new ItemDto(i.Id, i.Name, i.Area))
            .FirstOrDefaultAsync();

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<ItemDto>> Create([FromBody] CreateItemRequest request)
    {
        var name = request.Name.Trim();
        var area = request.Area.Trim();
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(area))
        {
            return ValidationProblem("Name and area are required.");
        }

        var normalizedName = name.ToLower();
        var exists = await db.Items.AnyAsync(i => i.Name.ToLower() == normalizedName);
        if (exists)
        {
            return Conflict(new ProblemDetails
            {
                Title = "An item with that name already exists.",
                Status = StatusCodes.Status409Conflict
            });
        }

        var item = new Item
        {
            Name = name,
            Area = area
        };

        db.Items.Add(item);
        await db.SaveChangesAsync();

        var dto = new ItemDto(item.Id, item.Name, item.Area);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, dto);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ItemDto>> Update(int id, [FromBody] UpdateItemRequest request)
    {
        var item = await db.Items.FindAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        var name = request.Name.Trim();
        var area = request.Area.Trim();
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(area))
        {
            return ValidationProblem("Name and area are required.");
        }

        var normalizedName = name.ToLower();
        var exists = await db.Items.AnyAsync(i => i.Id != id && i.Name.ToLower() == normalizedName);
        if (exists)
        {
            return Conflict(new ProblemDetails
            {
                Title = "An item with that name already exists.",
                Status = StatusCodes.Status409Conflict
            });
        }

        item.Name = name;
        item.Area = area;

        await db.SaveChangesAsync();

        return Ok(new ItemDto(item.Id, item.Name, item.Area));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await db.Items.Include(i => i.GroceryEntries).FirstOrDefaultAsync(i => i.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        if (item.GroceryEntries.Any())
        {
            return Conflict(new ProblemDetails
            {
                Title = "Cannot delete an item that is used in the grocery list.",
                Status = StatusCodes.Status409Conflict
            });
        }

        db.Items.Remove(item);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
