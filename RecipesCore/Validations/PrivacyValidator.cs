using System.ComponentModel.DataAnnotations;

namespace RecipesCore.Validations;

public class PrivacyValidator : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return new ValidationResult("Privacy level is required");
        }
        if (value.ToString()!.ToLower() == "public" || value.ToString()!.ToLower() == "private")
        {
            return ValidationResult.Success;
        }
        return new ValidationResult("Invalid value for privacy level");
    }
}
