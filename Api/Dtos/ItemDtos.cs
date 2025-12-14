using System.ComponentModel.DataAnnotations;

namespace Api.Dtos;

public record ItemDto(int Id, string Name, string Area);

public class CreateItemRequest
{
    [Required, MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(128)]
    public string Area { get; set; } = string.Empty;
}

public class UpdateItemRequest : CreateItemRequest;
