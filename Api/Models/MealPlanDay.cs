namespace Api.Models;

public class MealPlanDay
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public string? Dinner { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

