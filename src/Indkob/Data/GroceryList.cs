using System.ComponentModel.DataAnnotations;

namespace Indkob.Data;

public class GroceryList
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public DateOnly? WeekStart { get; set; }

    public List<GroceryListItem> Items { get; set; } = [];
}
