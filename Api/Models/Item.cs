using System.ComponentModel.DataAnnotations;

namespace Api.Models;

public class Item
{
    public int Id { get; set; }

    [MaxLength(128)]
    public required string Name { get; set; }

    [MaxLength(128)]
    public required string Area { get; set; }

    public ICollection<GroceryEntry> GroceryEntries { get; set; } = new List<GroceryEntry>();
}
