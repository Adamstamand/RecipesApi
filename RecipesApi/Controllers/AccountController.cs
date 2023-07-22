using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipesCore.DTOs;
using RecipesCore.Identity;
using RecipesCore.Models;
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
    public async Task<IActionResult> PostRegister(Register registerDTO)
    {
        bool doesEmailExist = await _userManager.Users
            .AnyAsync(user => user.Email == registerDTO.Email);
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

            NewToken newToken = _jwtService.CreateJwtToken(user);
            user.RefreshToken = newToken.RefreshToken;
            user.RefreshTokenExpiration = newToken.RefreshTokenExpiration;
            await _userManager.UpdateAsync(user);

            AuthenticationResponse authenticationResponse = new()
            {
                UserName = newToken.UserName,
                Token = newToken.Token,
                RefreshToken = newToken.RefreshToken
            };

            return Ok(authenticationResponse);
        }

        Array errorMessage = result.Errors.Select(e => e.Description).ToArray();
        return BadRequest(errorMessage);
    }


    [HttpPost("login")]
    public async Task<IActionResult> PostLogIn(LogIn logInDTO)
    {
        var result = await _signInManager.PasswordSignInAsync(logInDTO.Email!,
            logInDTO.Password!, false, false);

        if (result.Succeeded)
        {
            ApplicationUser? user = await _userManager
                .FindByEmailAsync(logInDTO.Email!);
            if (user is null) return BadRequest("User not found");

            NewToken newToken = _jwtService.CreateJwtToken(user);
            user.RefreshToken = newToken.RefreshToken;
            user.RefreshTokenExpiration = newToken.RefreshTokenExpiration;
            await _userManager.UpdateAsync(user);

            AuthenticationResponse authenticationResponse = new()
            {
                UserName = newToken.UserName,
                Token = newToken.Token,
                RefreshToken = newToken.RefreshToken
            };

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
    public async Task<IActionResult> GenerateNewToken(TokenRequest tokenRequest)
    {
        ClaimsPrincipal? principal = _jwtService.GetPrincipalFromJwtToken(tokenRequest.Token!);
        if (principal is null) return BadRequest("Invalid Jwt access token");

        string? userName = principal.FindFirstValue(JwtRegisteredClaimNames.Name);
        if (userName is null) return BadRequest("UserName not found in token");

        ApplicationUser? user = await _userManager.FindByNameAsync(userName);
        if (user is null || user.RefreshToken != tokenRequest.RefreshToken || 
            user.RefreshTokenExpiration <= DateTime.Now)
        {
            return BadRequest("Invalid refresh token");
        }

        NewToken newToken = _jwtService.CreateJwtToken(user);
        user.RefreshToken = newToken.RefreshToken;
        user.RefreshTokenExpiration = newToken.RefreshTokenExpiration;
        await _userManager.UpdateAsync(user);

        AuthenticationResponse authenticationResponse = new()
        {
            UserName = newToken.UserName,
            Token = newToken.Token,
            RefreshToken = newToken.RefreshToken
        };

        return Ok(authenticationResponse);
    }
}