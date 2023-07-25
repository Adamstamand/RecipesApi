using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using RecipesApi.Controllers;
using RecipesCore.Entities;
using RecipesCore.Identity;
using RecipesCore.RepositoryContracts;

namespace RecipesApiTests.Controllers;

public class DashboardControllerTests
{
    private readonly DashboardController _dashboardController;
    private readonly IRecipesRepository _recipesRepository;
    private readonly IUserRepository _userRepository;
    private readonly Fixture _fixture;

    private readonly Mock<IRecipesRepository> _recipesRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;

    public DashboardControllerTests()
    {
        _recipesRepositoryMock = new Mock<IRecipesRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();

        _fixture = new Fixture();
        _recipesRepository = _recipesRepositoryMock.Object;
        _userRepository = _userRepositoryMock.Object;

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Authorization"] = "randomValue";

        _dashboardController = new DashboardController(
            _recipesRepository, _userRepository)
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext,
            }
        };
    }


    [Fact]
    public async Task GetDashboard_ShouldReturnUnauthorized_WhenAuthorizationHeaderIsNotPresent()
    {
       _dashboardController.HttpContext.Request.Headers.Remove("Authorization");

        var result = await _dashboardController.GetDashboard();

        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }
    
    [Fact]
    public async Task GetDashboard_ShouldReturnUnauthorized_WhenUserDoesNotExist()
    {
        _userRepositoryMock
            .Setup(m => m.FindUserFromJwtHeader(It.IsAny<StringValues>()))
            .ReturnsAsync(default(ApplicationUser?));

        var result = await _dashboardController.GetDashboard();

        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }
    
    [Fact]
    public async Task GetDashboard_ShouldReturnOk_WhenUserHasNoRecipes()
    {
        var user = _fixture.Create<ApplicationUser>();
        _userRepositoryMock
            .Setup(m => m.FindUserFromJwtHeader(It.IsAny<StringValues>()))
            .ReturnsAsync(user);
        _recipesRepositoryMock
            .Setup(m => m.UserRecipes(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(Array.Empty<Recipe>());

        var result = await _dashboardController.GetDashboard();

        result.Result.Should().BeOfType<OkObjectResult>();
    }
    
    [Fact]
    public async Task GetDashboard_ShouldReturnOk_WhenUserHasRecipes()
    {
        var user = _fixture.Create<ApplicationUser>();
        var recipes = _fixture.CreateMany<Recipe>().ToArray();
        _userRepositoryMock
            .Setup(m => m.FindUserFromJwtHeader(It.IsAny<StringValues>()))
            .ReturnsAsync(user);
        _recipesRepositoryMock
            .Setup(m => m.UserRecipes(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(recipes);

        var result = await _dashboardController.GetDashboard();

        result.Result.Should().BeOfType<OkObjectResult>();
    }

}
