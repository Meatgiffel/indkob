using System.ComponentModel.DataAnnotations;

namespace Indkob.Data;

public class GroceryCategory
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; }
}
