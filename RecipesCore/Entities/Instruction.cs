using System.ComponentModel.DataAnnotations;

namespace RecipesCore.Entities;

public class Instruction
{
    public int Id { get; set; }
    [Required]
    public string? Words { get; set; }
    public int Position { get; set; }
}
