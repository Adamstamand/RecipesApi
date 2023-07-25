using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using RecipesApi.Controllers;
using RecipesCore.Entities;
using RecipesCore.Identity;
using RecipesCore.RepositoryContracts;

namespace RecipesApi.Tests.Repository;

public class RecipeControllerTests
{
    private readonly RecipeController _recipeController;
    private readonly IRecipesRepository _recipesRepository;
    private readonly IUserRepository _userRepository;
    private readonly Fixture _fixture;

    private readonly Mock<IRecipesRepository> _recipesRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;

    public RecipeControllerTests()
    {
        _recipesRepositoryMock = new Mock<IRecipesRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();

        _fixture = new Fixture();
        _recipesRepository = _recipesRepositoryMock.Object;
        _userRepository = _userRepositoryMock.Object;

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Authorization"] = "randomValue";

        _recipeController = new RecipeController(
            _recipesRepository, _userRepository)
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext,
            }
        };
    }

    [Fact]
    public async Task GetAllRecipes_ShouldReturnRecipeArray_WhenRecipesExist()
    {
        var recipes = _fixture.CreateMany<Recipe>(5).ToArray();
        _recipesRepositoryMock.Setup(m => m.AllRecipes()).ReturnsAsync(recipes);

        var result = await _recipeController.GetAllRecipes();

        result.Should().BeEquivalentTo(recipes).And.HaveCount(5);
    }

    [Fact]
    public async Task GetAllRecipes_ShouldReturnEmptyArray_WhenNoRecipesExist()
    {
        _recipesRepositoryMock.Setup(m => m.AllRecipes()).ReturnsAsync(Array.Empty<Recipe>());

        var result = await _recipeController.GetAllRecipes();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSpecificRecipe_ShouldReturnNotFound_WhenRecipeDoesNotExist()
    {
        _recipesRepositoryMock
            .Setup(m => m.SpecificRecipe(It.IsAny<int>()))
            .ReturnsAsync(default(Recipe));

        var result = await _recipeController.GetSpecificRecipe(It.IsAny<int>());

        result.Result.Should()
            .BeOfType<NotFoundObjectResult>();
    }
    
    [Fact]
    public async Task GetSpecificRecipe_ShouldReturnRecipe_WhenRecipeExistsAndIsPublic()
    {
        var recipe = _fixture.Build<Recipe>().With(m => m.Privacy, "public").Create();
        _recipesRepositoryMock
            .Setup(m => m.SpecificRecipe(It.IsAny<int>()))
            .ReturnsAsync(recipe);

        var result = await _recipeController.GetSpecificRecipe(It.IsAny<int>());

        result.Value.Should().BeEquivalentTo(recipe);
    }
    
    [Fact]
    public async Task GetSpecificRecipe_ShouldReturnUnauthorized_WhenRecipeIsPrivateAndHttpHeadersDoNotHaveAuthorization()
    {
        var recipe = _fixture.Build<Recipe>().With(m => m.Privacy, "private").Create();
        _recipesRepositoryMock
            .Setup(m => m.SpecificRecipe(It.IsAny<int>()))
            .ReturnsAsync(recipe);
        _recipeController.HttpContext.Request.Headers.Remove("Authorization");

        var result = await _recipeController.GetSpecificRecipe(It.IsAny<int>());

        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }
    
    [Fact]
    public async Task GetSpecificRecipe_ShouldReturnUnauthorized_WhenRecipeIsPrivateAndUserIsNull()
    {
        var recipe = _fixture.Build<Recipe>().With(m => m.Privacy, "private").Create();
        _recipesRepositoryMock
            .Setup(m => m.SpecificRecipe(It.IsAny<int>()))
            .ReturnsAsync(recipe);
        _userRepositoryMock
            .Setup(m => m.FindUserFromJwtHeader(It.IsAny<StringValues>()))
            .ReturnsAsync(default(ApplicationUser?));

        var result = await _recipeController.GetSpecificRecipe(It.IsAny<int>());

        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }
    
    
    [Fact]
    public async Task GetSpecificRecipe_ShouldReturnRecipe_WhenRecipeIsPrivateAndUserCreatedRecipe()
    {
        var recipe = _fixture.Build<Recipe>().With(m => m.Privacy, "private").Create();
        var user = _fixture.Create<ApplicationUser>();
        _recipesRepositoryMock
            .Setup(m => m.SpecificRecipe(It.IsAny<int>()))
            .ReturnsAsync(recipe);
        _userRepositoryMock
            .Setup(m => m.FindUserFromJwtHeader(It.IsAny<StringValues>()))
            .ReturnsAsync(user);
        _recipesRepositoryMock
            .Setup(m => m.CheckRecipeAccess(It.IsAny<int>(), user))
            .ReturnsAsync(true);

        var result = await _recipeController.GetSpecificRecipe(It.IsAny<int>());

        result.Value.Should().BeEquivalentTo(recipe);
    }
    [Fact]
    public async Task GetSpecificRecipe_ShouldReturnUnauthorized_WhenRecipeIsPrivateAndUserDidNotCreateRecipe()
    {
        var recipe = _fixture.Build<Recipe>().With(m => m.Privacy, "private").Create();
        var user = _fixture.Create<ApplicationUser>();
        _recipesRepositoryMock
            .Setup(m => m.SpecificRecipe(It.IsAny<int>()))
            .ReturnsAsync(recipe);
        _userRepositoryMock
            .Setup(m => m.FindUserFromJwtHeader(It.IsAny<StringValues>()))
            .ReturnsAsync(user);
        _recipesRepositoryMock
            .Setup(m => m.CheckRecipeAccess(It.IsAny<int>(), user))
            .ReturnsAsync(false);

        var result = await _recipeController.GetSpecificRecipe(It.IsAny<int>());

        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }
}
