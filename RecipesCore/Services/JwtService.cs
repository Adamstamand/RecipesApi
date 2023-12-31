﻿using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RecipesCore.DTOs;
using RecipesCore.Identity;
using RecipesCore.ServiceContracts;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using RecipesCore.Models;

namespace RecipesCore.Services;

public class JwtService : IJwtService
{
    public readonly IConfiguration _configuration;
    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public NewToken CreateJwtToken(ApplicationUser user)
    {
        DateTime expirationDate = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:Expiration_Minutes"]));

        Claim[] claims = new Claim[]
        {
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

        return new NewToken 
        { 
            Token = token, 
            UserName = user.UserName!,
            RefreshToken = GenerateRefreshToken(),
            RefreshTokenExpiration = DateTime.UtcNow.AddMinutes(Convert.ToInt32(_configuration["RefreshToken:Expiration_Minutes"]))
        };
    }


    private static string GenerateRefreshToken()
    {
        byte[] bytes = new byte[64];
        var randomNumberGenerator = RandomNumberGenerator.Create();
        randomNumberGenerator.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }


    public ClaimsPrincipal? GetPrincipalFromJwtToken(string token)
    {
        TokenValidationParameters tokenValidationParameters = new()
        {
            ValidateAudience = true,
            ValidAudience = _configuration["Jwt:Audience"],
            ValidateIssuer = true,
            ValidIssuer = _configuration["Jwt:Issuer"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)),
            ValidateLifetime = false
        };

        JwtSecurityTokenHandler jwtSecurityTokenHandler = new();
        SecurityToken securityToken;
        ClaimsPrincipal principal;
        try
        {
            principal = jwtSecurityTokenHandler
                .ValidateToken(token,
                tokenValidationParameters,
                out securityToken);
        }
        catch
        {
            return null;
        }

        if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            return null!;
        }

        return principal;
    }
}
