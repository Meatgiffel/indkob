using Api.Data;
using Api.Dtos;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RecipesController(AppDbContext db, MealieService mealie, ILogger<RecipesController> logger) : ControllerBase
{
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<RecipeSummaryDto>>> Search([FromQuery] string? q, CancellationToken ct)
    {
        if (!mealie.IsConfigured)
        {
            return MealieNotConfigured();
        }

        try
        {
            return Ok(await mealie.SearchRecipesAsync(q, ct));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Mealie recipe search failed for query '{Query}'.", q);
            return MealieUnavailable();
        }
    }

    [HttpGet("{slug}/ingredients")]
    public async Task<ActionResult<RecipeIngredientsDto>> GetIngredients(string slug, CancellationToken ct)
    {
        if (!mealie.IsConfigured)
        {
            return MealieNotConfigured();
        }

        (string Slug, string Name, IReadOnlyList<MealieService.NormalizedIngredient> Ingredients)? recipe;
        try
        {
            recipe = await mealie.GetRecipeAsync(slug, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Mealie recipe fetch failed for slug '{Slug}'.", slug);
            return MealieUnavailable();
        }

        if (recipe is null)
        {
            return NotFound(new ProblemDetails { Title = "Recipe not found.", Status = StatusCodes.Status404NotFound });
        }

        var items = await db.Items.AsNoTracking()
            .Select(i => new { i.Id, i.Name, i.Area })
            .ToListAsync(ct);
        var entries = await db.GroceryEntries.AsNoTracking()
            .Select(e => new { e.ItemId, e.Note })
            .ToListAsync(ct);

        var itemByName = items
            .GroupBy(i => i.Name.Trim().ToLowerInvariant())
            .ToDictionary(g => g.Key, g => g.First());
        var entryItemIds = entries
            .Where(e => e.ItemId != null)
            .Select(e => e.ItemId!.Value)
            .ToHashSet();
        var entryNotes = entries
            .Where(e => e.ItemId == null && !string.IsNullOrWhiteSpace(e.Note))
            .Select(e => e.Note!.Trim().ToLowerInvariant())
            .ToHashSet();

        var ingredients = recipe.Value.Ingredients.Select(ing =>
        {
            var key = ing.Name.Trim().ToLowerInvariant();
            itemByName.TryGetValue(key, out var match);
            var onList = match is not null
                ? entryItemIds.Contains(match.Id)
                : entryNotes.Contains(key);
            return new RecipeIngredientDto(ing.Name, ing.Amount, ing.Display, match?.Id, match?.Area, onList);
        }).ToList();

        return Ok(new RecipeIngredientsDto(recipe.Value.Slug, recipe.Value.Name, ingredients));
    }

    private ObjectResult MealieNotConfigured() => StatusCode(StatusCodes.Status503ServiceUnavailable, new ProblemDetails
    {
        Title = "Mealie er ikke konfigureret.",
        Detail = "Sæt Mealie:BaseUrl og Mealie:ApiToken på serveren.",
        Status = StatusCodes.Status503ServiceUnavailable
    });

    private ObjectResult MealieUnavailable() => StatusCode(StatusCodes.Status502BadGateway, new ProblemDetails
    {
        Title = "Kunne ikke kontakte Mealie.",
        Status = StatusCodes.Status502BadGateway
    });
}
