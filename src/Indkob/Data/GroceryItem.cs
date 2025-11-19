using System.ComponentModel.DataAnnotations;

namespace Indkob.Data;

public class GroceryItem
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? DefaultUnit { get; set; }

    public int? GroceryCategoryId { get; set; }
    public GroceryCategory? GroceryCategory { get; set; }
}
