using System.ComponentModel.DataAnnotations;

namespace Api.Dtos;

public record RecipeSummaryDto(string Id, string Slug, string Name, string? Description);

public record RecipeIngredientDto(
    string Name,
    string? Amount,
    string Display,
    int? MatchedItemId,
    string? MatchedItemArea,
    bool AlreadyOnList);

public record RecipeIngredientsDto(
    string Slug,
    string Name,
    IReadOnlyList<RecipeIngredientDto> Ingredients);

public class AddFromRecipeRequest
{
    [MaxLength(256)]
    public string? Source { get; set; }

    [Required, MinLength(1)]
    public List<AddFromRecipeIngredient> Ingredients { get; set; } = new();
}

public class AddFromRecipeIngredient
{
    [Required, MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(64)]
    public string? Amount { get; set; }

    // When set, link to this existing catalog item. When null, a new item is
    // created under Area (required in that case).
    [Range(1, int.MaxValue)]
    public int? ItemId { get; set; }

    [MaxLength(128)]
    public string? Area { get; set; }
}
