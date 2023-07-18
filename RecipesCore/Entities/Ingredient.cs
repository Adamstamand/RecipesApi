﻿using System.ComponentModel.DataAnnotations;

namespace RecipesCore.Entities;

public class Ingredient
{
    public int Id { get; set; }
    [Required]
    public required string Words { get; set; }
    public int RecipeId { get; set; }
}
