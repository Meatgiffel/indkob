using System.ComponentModel.DataAnnotations;

namespace Api.Models;

public class MealPlanDay
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public string? Dinner { get; set; }

    // Optional link to a Mealie recipe. Null = free-text dinner only.
    [MaxLength(128)]
    public string? RecipeSlug { get; set; }

    [MaxLength(256)]
    public string? RecipeName { get; set; }

    [MaxLength(64)]
    public string? RecipeId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
