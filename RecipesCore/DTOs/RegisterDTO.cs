using System.ComponentModel.DataAnnotations;

namespace RecipesCore.DTOs;

public class RegisterDTO
{
    [Required]
    [EmailAddress]
    public string? Email { get; set; }

    [Required]
    public string? Password { get; set; }  
    
    [Required]
    [Compare("Password")]
    public string? ConfirmPassword { get; set; }
}
