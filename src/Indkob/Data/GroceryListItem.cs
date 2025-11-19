using System.ComponentModel.DataAnnotations;

namespace Indkob.Data;

public class GroceryListItem
{
    public int Id { get; set; }

    public int GroceryListId { get; set; }
    public GroceryList GroceryList { get; set; } = default!;

    public int? GroceryItemId { get; set; }
    public GroceryItem? GroceryItem { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public decimal? Quantity { get; set; }

    [MaxLength(50)]
    public string? Unit { get; set; }

    public int? GroceryCategoryId { get; set; }
    public GroceryCategory? GroceryCategory { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public bool IsChecked { get; set; }
}
