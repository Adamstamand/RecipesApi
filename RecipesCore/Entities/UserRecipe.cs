using RecipesCore.Identity;

namespace RecipesCore.Entities;

public class UserRecipe
{
    public int Id { get; set; }
    public ApplicationUser? User { get; set; }
    public Recipe? Recipe { get; set; }
}
