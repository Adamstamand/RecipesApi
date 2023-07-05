using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RecipesCore.DTOs;
using RecipesCore.Identity;
using RecipesCore.ServiceContracts;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace RecipesCore.Services;

public class JwtService : IJwtService
{
    public readonly IConfiguration _configuration;
    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public AuthenticationResponseDTO CreateJwtToken(ApplicationUser user)
    {
        DateTime expirationDate = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:Expiration_Minutes"]));

        Claim[] claims = new Claim[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),

            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),

            new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),

            new Claim(JwtRegisteredClaimNames.Name, user.UserName!),
        };

        SymmetricSecurityKey securityKey = new(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

        SigningCredentials signingCredentials = new(securityKey, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken tokenGenerator = new(
            _configuration["Jwt:Issuer"],
            _configuration["Jwt:Audience"],
            claims, 
            expires: expirationDate, 
            signingCredentials: signingCredentials);

        JwtSecurityTokenHandler tokenHandler = new();
        string token = tokenHandler.WriteToken(tokenGenerator);

        return new AuthenticationResponseDTO 
        { 
            Token = token, 
            UserName = user.UserName, 
            Expiration = expirationDate,
            RefreshToken = GenerateRefreshToken(),
            RefreshTokenExpiration = DateTime.Now.AddMinutes(Convert.ToInt32(_configuration["RefreshToken:Expiration_Minutes"]))
        };
    }


    private static string GenerateRefreshToken()
    {
        byte[] bytes = new byte[64];
        var randomNumberGenerator = RandomNumberGenerator.Create();
        randomNumberGenerator.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}
