using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RecipesCore.DTOs;
using RecipesCore.Identity;

namespace RecipesApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class AccountController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public AccountController(UserManager<ApplicationUser> userManager, 
        SignInManager<ApplicationUser> signInManager, 
        RoleManager<ApplicationRole> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
    }


    [HttpPost]
    public async Task<ActionResult<ApplicationUser>> PostRegister(RegisterDTO registerDTO)
    {
        if (!ModelState.IsValid)
        {
            string errorMessage = string.Join(" | ", 
                ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));
            return Problem(errorMessage);
        }

        ApplicationUser user = new()
        {
            Email = registerDTO.Email,
            UserName = registerDTO.Email,
        };

        IdentityResult result = await _userManager.CreateAsync(user, registerDTO.Password!);

        if (result.Succeeded)
        {
            await _signInManager.SignInAsync(user, isPersistent: false);
            return Ok(user);
        }
        else
        {
            string errorMessage = string.Join(" | ",
                result.Errors.Select(e => e.Description));
            return Problem(errorMessage);
        }
    }
}
