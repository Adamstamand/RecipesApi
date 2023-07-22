using System.ComponentModel.DataAnnotations;

namespace RecipesCore.DTOs;

public class Register
{
    [Required, EmailAddress, StringLength(254)]
    public required string Email { get; set; }
    [Required, StringLength(30, MinimumLength = 8)]
    public required string Password { get; set; }  
    
    [Required, Compare("Password"), StringLength(30, MinimumLength = 8)]
    public required string ConfirmPassword { get; set; }
}
