using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DnD.Application.DTOs;
using DnD.Application.Interfaces;
using DnD.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DnD.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly IConfiguration _config;

    public AuthService(UserManager<User> userManager, IConfiguration config)
    {
        _userManager = userManager;
        _config = config;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto model)
    {
        var user = new User { UserName = model.Username, Email = model.Email };
        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Errors = result.Errors.Select(e => e.Description),
            };
        }

        return new AuthResponseDto { IsSuccess = true, Token = GenerateJwtToken(user) };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Errors = new[] { "wrong Email or Password" },
            };
        }

        return new AuthResponseDto { IsSuccess = true, Token = GenerateJwtToken(user) };
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? "fallback_key_32_chars_long")
        );
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
