using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RecipesCore.DTOs;
using RecipesCore.Identity;
using RecipesCore.ServiceContracts;

namespace RecipesApi.Controllers;

[Route("api/")]
[ApiController]
[AllowAnonymous]
public class AccountController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IJwtService _jwtService;

    public AccountController(UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<ApplicationRole> roleManager, IJwtService jwtService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _jwtService = jwtService;
        
    }


    [HttpPost("register")]
    public async Task<IActionResult> PostRegister(RegisterDTO registerDTO)
    {
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
        return Unauthorized(errorMessage);
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
}