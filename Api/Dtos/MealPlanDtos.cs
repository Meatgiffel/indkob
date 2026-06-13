using System.ComponentModel.DataAnnotations;

namespace Api.Dtos;

public record MealPlanDayDto(
    DateOnly Date,
    string? Dinner,
    string? RecipeSlug,
    string? RecipeName,
    string? RecipeId);

public class UpdateMealPlanDayRequest
{
    [MaxLength(256)]
    public string? Dinner { get; set; }

    [MaxLength(128)]
    public string? RecipeSlug { get; set; }

    [MaxLength(256)]
    public string? RecipeName { get; set; }

    [MaxLength(64)]
    public string? RecipeId { get; set; }
}
