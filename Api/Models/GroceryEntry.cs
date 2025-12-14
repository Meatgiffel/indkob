using System.ComponentModel.DataAnnotations;

namespace Api.Models;

public class GroceryEntry
{
    public int Id { get; set; }

    public int? ItemId { get; set; }
    public Item? Item { get; set; }

    [MaxLength(64)]
    public string? Amount { get; set; }

    [MaxLength(512)]
    public string? Note { get; set; }

    public bool IsDone { get; set; }

    public DateTime CreatedAt { get; set; }
}
