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

    [Fact]
    public async Task PostRecipe_ShouldReturnUnauthorized_WhenAuthorizationHeaderIsNotPresent()
    {
        var recipe = _fixture.Create<Recipe>();
    _recipeController.HttpContext.Request.Headers.Remove("Authorization");

        var result = await _recipeController.PostRecipe(recipe);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }
    
    [Fact]
    public async Task PostRecipe_ShouldReturnBadRequest_WhenRecipeNameAlreadyExists()
    {
        var recipe = _fixture.Create<Recipe>();
        var user = _fixture.Create<ApplicationUser>();
        _userRepositoryMock
            .Setup(m => m.FindUserFromJwtHeader(It.IsAny<StringValues>()))
            .ReturnsAsync(user);
        _recipesRepositoryMock
            .Setup(m => m.DoesRecipeExistAlready(recipe))
            .ReturnsAsync(true);

        var result = await _recipeController.PostRecipe(recipe);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task PostRecipe_ShouldReturnCreatedAtAction_OnSuccess()
    {
        var recipe = _fixture.Create<Recipe>();
        var user = _fixture.Create<ApplicationUser>();
        _userRepositoryMock
            .Setup(m => m.FindUserFromJwtHeader(It.IsAny<StringValues>()))
            .ReturnsAsync(user);
        _recipesRepositoryMock
            .Setup(m => m.DoesRecipeExistAlready(It.IsAny<Recipe>()))
            .ReturnsAsync(false);
        _recipesRepositoryMock
            .Setup(m => m.AddRecipe(It.IsAny<UserRecipe>()))
            .ReturnsAsync(recipe);

        var result = await _recipeController.PostRecipe(recipe);

        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task DeleteRecipe_ShouldReturnUnauthorized_WhenAuthorizationHeaderIsNotPresent()
    {     
        _recipeController.HttpContext.Request.Headers.Remove("Authorization");

        var result = await _recipeController.DeleteRecipe(It.IsAny<int>());

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task DeleteRecipe_ShouldReturnUnauthorized_WhenUserDoesNotExist()
    {
        _userRepositoryMock
            .Setup(m => m.FindUserFromJwtHeader(It.IsAny<StringValues>()))
            .ReturnsAsync(default(ApplicationUser?));

        var result = await _recipeController.DeleteRecipe(It.IsAny<int>());

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }
    
    [Fact]
    public async Task DeleteRecipe_ShouldReturnUnauthorized_WhenUserDidNotCreateRecipe()
    {
        var user = _fixture.Create<ApplicationUser>();
        _userRepositoryMock
            .Setup(m => m.FindUserFromJwtHeader(It.IsAny<StringValues>()))
            .ReturnsAsync(user);
        _recipesRepositoryMock
            .Setup(m => m.CheckRecipeAccess(It.IsAny<int>(), It.IsAny<ApplicationUser>()))
            .ReturnsAsync(false);

        var result = await _recipeController.DeleteRecipe(It.IsAny<int>());

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }
    
    [Fact]
    public async Task DeleteRecipe_ShouldReturnNoContent_WhenRecipeIsDeleted()
    {
        var user = _fixture.Create<ApplicationUser>();
        _userRepositoryMock
            .Setup(m => m.FindUserFromJwtHeader(It.IsAny<StringValues>()))
            .ReturnsAsync(user);
        _recipesRepositoryMock
            .Setup(m => m.CheckRecipeAccess(It.IsAny<int>(), It.IsAny<ApplicationUser>()))
            .ReturnsAsync(true);
        _recipesRepositoryMock
            .Setup(m => m.RemoveRecipe(It.IsAny<int>()))
            .ReturnsAsync("deleted");

        var result = await _recipeController.DeleteRecipe(It.IsAny<int>());

        result.Should().BeOfType<NoContentResult>();
    }
    
    [Fact]
    public async Task DeleteRecipe_ShouldReturnBadRequest_WhenRecipeFailedToDelete()
    {
        var user = _fixture.Create<ApplicationUser>();
        _userRepositoryMock
            .Setup(m => m.FindUserFromJwtHeader(It.IsAny<StringValues>()))
            .ReturnsAsync(user);
        _recipesRepositoryMock
            .Setup(m => m.CheckRecipeAccess(It.IsAny<int>(), It.IsAny<ApplicationUser>()))
            .ReturnsAsync(true);
        _recipesRepositoryMock
            .Setup(m => m.RemoveRecipe(It.IsAny<int>()))
            .ReturnsAsync(default(string?));

        var result = await _recipeController.DeleteRecipe(It.IsAny<int>());

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task PutRecipe_ShouldReturnBadRequest_IfIdsDoNotMatch()
    {
        int id = 1;
        var recipe = _fixture
            .Build<Recipe>()
            .With(m => m.Id, 2)
            .Create();

        var result = await _recipeController.PutRecipe(id, recipe);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task PutRecipe_ShouldReturnUnauthorized_WhenAuthorizationHeaderIsNotPresent()
    {
        int id = 2;
        var recipe = _fixture
            .Build<Recipe>()
            .With(m => m.Id, 2)
            .Create();
        _recipeController.HttpContext.Request.Headers.Remove("Authorization");

        var result = await _recipeController.PutRecipe(id, recipe);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }
    
    [Fact]
    public async Task PutRecipe_ShouldReturnUnauthorized_WhenUserDoesNotExist()
    {
        int id = 2;
        var recipe = _fixture
            .Build<Recipe>()
            .With(m => m.Id, 2)
            .Create();
        _userRepositoryMock
            .Setup(m => m.FindUserFromJwtHeader(It.IsAny<StringValues>()))
            .ReturnsAsync(default(ApplicationUser?));

        var result = await _recipeController.PutRecipe(id, recipe);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }
    
    [Fact]
    public async Task PutRecipe_ShouldReturnUnauthorized_WhenUserDidNotCreateTheRecipe()
    {
        int id = 2;
        var recipe = _fixture
            .Build<Recipe>()
            .With(m => m.Id, 2)
            .Create();
        var user = _fixture.Create<ApplicationUser>();
        _userRepositoryMock
            .Setup(m => m.FindUserFromJwtHeader(It.IsAny<StringValues>()))
            .ReturnsAsync(user);
        _recipesRepositoryMock.Setup(m => m.CheckRecipeAccess(It.IsAny<int>(), It.IsAny<ApplicationUser>()))
            .ReturnsAsync(false);

        var result = await _recipeController.PutRecipe(id, recipe);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }
    
    [Fact]
    public async Task PutRecipe_ShouldReturnNoContent_OnSuccess()
    {
        int id = 2;
        var recipe = _fixture
            .Build<Recipe>()
            .With(m => m.Id, 2)
            .Create();
        var user = _fixture.Create<ApplicationUser>();
        _userRepositoryMock
            .Setup(m => m.FindUserFromJwtHeader(It.IsAny<StringValues>()))
            .ReturnsAsync(user);
        _recipesRepositoryMock.Setup(m => m.CheckRecipeAccess(It.IsAny<int>(), It.IsAny<ApplicationUser>()))
            .ReturnsAsync(true);
        _recipesRepositoryMock
            .Setup(m => m.UpdateRecipe(It.IsAny<Recipe>()))
            .ReturnsAsync("updated");

        var result = await _recipeController.PutRecipe(id, recipe);

        result.Should().BeOfType<NoContentResult>();
    }
    
    [Fact]
    public async Task PutRecipe_ShouldReturnBadRequest_WhenUpdateRecipeFails()
    {
        int id = 2;
        var recipe = _fixture
            .Build<Recipe>()
            .With(m => m.Id, 2)
            .Create();
        var user = _fixture.Create<ApplicationUser>();
        _userRepositoryMock
            .Setup(m => m.FindUserFromJwtHeader(It.IsAny<StringValues>()))
            .ReturnsAsync(user);
        _recipesRepositoryMock.Setup(m => m.CheckRecipeAccess(It.IsAny<int>(), It.IsAny<ApplicationUser>()))
            .ReturnsAsync(true);
        _recipesRepositoryMock
            .Setup(m => m.UpdateRecipe(It.IsAny<Recipe>()))
            .ReturnsAsync(default(string?));

        var result = await _recipeController.PutRecipe(id, recipe);

        result.Should().BeOfType<BadRequestResult>();
    }
}
