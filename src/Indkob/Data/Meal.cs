using System.ComponentModel.DataAnnotations;

namespace Indkob.Data;

public class Meal
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }
}
