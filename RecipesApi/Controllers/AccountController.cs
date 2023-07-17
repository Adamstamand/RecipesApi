using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipesCore.DTOs;
using RecipesCore.Identity;
using RecipesCore.ServiceContracts;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RecipesApi.Controllers;

[Route("api/")]
[ApiController]
[AllowAnonymous]
public class AccountController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtService _jwtService;

    public AccountController(UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtService jwtService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
    }


    [HttpPost("register")]
    public async Task<IActionResult> PostRegister(RegisterDTO registerDTO)
    {
        bool doesEmailExist = await _userManager.Users.AnyAsync(user => user.Email == registerDTO.Email);
        if (doesEmailExist) return BadRequest("That Email already exists");

        ApplicationUser user = new()
        {
            Email = registerDTO.Email,
            UserName = registerDTO.Email
        };

        IdentityResult result = await _userManager.CreateAsync(user, registerDTO.Password!);

        if (result.Succeeded)
        {
            await _signInManager.SignInAsync(user, isPersistent: false);

            var authenticationResponse = _jwtService.CreateJwtToken(user);
            user.RefreshToken = authenticationResponse.RefreshToken;
            user.RefreshTokenExpiration = authenticationResponse.RefreshTokenExpiration;
            await _userManager.UpdateAsync(user);

            return Ok(authenticationResponse);
        }

        Array errorMessage = result.Errors.Select(e => e.Description).ToArray();
        return BadRequest(errorMessage);
    }


    [HttpPost("login")]
    public async Task<IActionResult> PostLogIn(LogInDTO logInDTO)
    {
        var result = await _signInManager.PasswordSignInAsync(logInDTO.Email!,
            logInDTO.Password!, false, false);

        if (result.Succeeded)
        {
            ApplicationUser? user = await _userManager
                .FindByEmailAsync(logInDTO.Email!);
            var authenticationResponse = _jwtService.CreateJwtToken(user!);

            user!.RefreshToken = authenticationResponse.RefreshToken;
            user.RefreshTokenExpiration = authenticationResponse.RefreshTokenExpiration;

            await _userManager.UpdateAsync(user);
            return Ok(authenticationResponse);
        }

        return Unauthorized("Invalid email or password");
    }


    [HttpGet("logout")]
    public async Task<IActionResult> GetLogOut()
    {
        await _signInManager.SignOutAsync();
        return NoContent();
    }


    [HttpPost("new-token")]
    public async Task<IActionResult> GenerateNewToken(TokenModel tokenModel)
    {
        ClaimsPrincipal? principal = _jwtService.GetPrincipalFromJwtToken(tokenModel.Token!);
        if (principal == null) return BadRequest("Invalid Jwt access token");

        string? userName = principal.FindFirstValue(JwtRegisteredClaimNames.Name);
        if (userName is null) return BadRequest("UserName not found in token");

        ApplicationUser? user = await _userManager.FindByNameAsync(userName);
        if (user == null || user.RefreshToken != tokenModel.RefreshToken || user.RefreshTokenExpiration <= DateTime.Now)
        {
            return BadRequest("Invalid refresh token");
        }

        AuthenticationResponseDTO authenticationResponse = _jwtService.CreateJwtToken(user);
        user.RefreshToken = authenticationResponse.RefreshToken;
        user.RefreshTokenExpiration = authenticationResponse.RefreshTokenExpiration;
        await _userManager.UpdateAsync(user);

        return Ok(authenticationResponse);
    }
}