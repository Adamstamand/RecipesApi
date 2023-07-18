using RecipesCore.Identity;

namespace RecipesCore.Entities;

public class UserRecipe
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public required ApplicationUser User { get; set; }
    public int RecipeId { get; set; }
    public required Recipe Recipe { get; set; }
}
