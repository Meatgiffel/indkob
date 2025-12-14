using System.ComponentModel.DataAnnotations;

namespace Api.Dtos;

public record GroceryEntryDto(
    int Id,
    int? ItemId,
    string? ItemName,
    string? ItemArea,
    string? Amount,
    string? Note,
    bool IsDone,
    DateTime CreatedAt);

public class CreateGroceryEntryRequest
{
    [Range(1, int.MaxValue)]
    public int? ItemId { get; set; }

    [MaxLength(64)]
    public string? Amount { get; set; }

    [MaxLength(512)]
    public string? Note { get; set; }
}

public class UpdateGroceryEntryRequest : CreateGroceryEntryRequest
{
    public bool IsDone { get; set; }
}
