using System.ComponentModel.DataAnnotations;

namespace Indkob.Data;

public class MealPlanEntry
{
    public int Id { get; set; }

    public DateOnly Day { get; set; }

    public MealSlot Slot { get; set; }

    public int? MealId { get; set; }
    public Meal? Meal { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }
}
