using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using DnD.Application.DTOs;
using DnD.Application.Interfaces;
using DnD.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DnD.Application.Services;

public class OAuthService : IOAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly IConfiguration _config;
    private readonly HttpClient _httpClient;

    public OAuthService(UserManager<User> userManager, IConfiguration config, HttpClient httpClient)
    {
        _userManager = userManager;
        _config = config;
        _httpClient = httpClient;
    }

    public string GetGoogleAuthorizationUrl(string state, string redirectUri)
    {
        var clientId = _config["OAuth:Google:ClientId"];
        var scope = "openid profile email";
        var responseType = "code";

        return $"https://accounts.google.com/o/oauth2/v2/auth?"
            + $"client_id={clientId}"
            + $"&redirect_uri={Uri.EscapeDataString(redirectUri)}"
            + $"&response_type={responseType}"
            + $"&scope={Uri.EscapeDataString(scope)}"
            + $"&state={state}";
    }

    public string GetDiscordAuthorizationUrl(string state, string redirectUri)
    {
        var clientId = _config["OAuth:Discord:ClientId"];
        var scope = "identify email";
        var responseType = "code";

        return $"https://discord.com/api/oauth2/authorize?"
            + $"client_id={clientId}"
            + $"&redirect_uri={Uri.EscapeDataString(redirectUri)}"
            + $"&response_type={responseType}"
            + $"&scope={Uri.EscapeDataString(scope)}"
            + $"&state={state}";
    }

    public async Task<OAuthResponseDto?> GetGoogleUserInfoAsync(string code, string redirectUri)
    {
        try
        {
            var clientId = _config["OAuth:Google:ClientId"];
            var clientSecret = _config["OAuth:Google:ClientSecret"];

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                return null;

            var tokenRequest = new HttpRequestMessage(
                HttpMethod.Post,
                "https://oauth2.googleapis.com/token"
            )
            {
                Content = new FormUrlEncodedContent(
                    new Dictionary<string, string>
                    {
                        { "code", code },
                        { "client_id", clientId },
                        { "client_secret", clientSecret },
                        { "redirect_uri", redirectUri },
                        { "grant_type", "authorization_code" },
                    }
                ),
            };

            var tokenResponse = await _httpClient.SendAsync(tokenRequest);
            if (!tokenResponse.IsSuccessStatusCode)
                return null;

            var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
            using var tokenDoc = JsonDocument.Parse(tokenContent);
            var accessToken = tokenDoc.RootElement.GetProperty("access_token").GetString();

            var userRequest = new HttpRequestMessage(
                HttpMethod.Get,
                "https://openidconnect.googleapis.com/v1/userinfo"
            )
            {
                Headers = { { "Authorization", $"Bearer {accessToken}" } },
            };

            var userResponse = await _httpClient.SendAsync(userRequest);
            if (!userResponse.IsSuccessStatusCode)
                return null;

            var userContent = await userResponse.Content.ReadAsStringAsync();
            using var userDoc = JsonDocument.Parse(userContent);
            var root = userDoc.RootElement;

            return new OAuthResponseDto
            {
                ProviderId = root.GetProperty("sub").GetString() ?? string.Empty,
                Provider = "Google",
                Email = root.GetProperty("email").GetString() ?? string.Empty,
                DisplayName = root.GetProperty("name").GetString() ?? string.Empty,
                AvatarUrl = root.TryGetProperty("picture", out var picture)
                    ? picture.GetString()
                    : null,
            };
        }
        catch
        {
            return null;
        }
    }

    public async Task<OAuthResponseDto?> GetDiscordUserInfoAsync(string code, string redirectUri)
    {
        try
        {
            var clientId = _config["OAuth:Discord:ClientId"];
            var clientSecret = _config["OAuth:Discord:ClientSecret"];

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                return null;

            var tokenRequest = new HttpRequestMessage(
                HttpMethod.Post,
                "https://discord.com/api/v10/oauth2/token"
            )
            {
                Content = new FormUrlEncodedContent(
                    new Dictionary<string, string>
                    {
                        { "code", code },
                        { "client_id", clientId },
                        { "client_secret", clientSecret },
                        { "redirect_uri", redirectUri },
                        { "grant_type", "authorization_code" },
                    }
                ),
            };

            var tokenResponse = await _httpClient.SendAsync(tokenRequest);
            if (!tokenResponse.IsSuccessStatusCode)
                return null;

            var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
            using var tokenDoc = JsonDocument.Parse(tokenContent);
            var accessToken = tokenDoc.RootElement.GetProperty("access_token").GetString();

            var userRequest = new HttpRequestMessage(
                HttpMethod.Get,
                "https://discord.com/api/v10/users/@me"
            )
            {
                Headers = { { "Authorization", $"Bearer {accessToken}" } },
            };

            var userResponse = await _httpClient.SendAsync(userRequest);
            if (!userResponse.IsSuccessStatusCode)
                return null;

            var userContent = await userResponse.Content.ReadAsStringAsync();
            using var userDoc = JsonDocument.Parse(userContent);
            var root = userDoc.RootElement;

            var userId = root.GetProperty("id").GetString() ?? string.Empty;
            var avatar = root.TryGetProperty("avatar", out var avatarProp)
                ? avatarProp.GetString()
                : null;
            var avatarUrl = !string.IsNullOrEmpty(avatar)
                ? $"https://cdn.discordapp.com/avatars/{userId}/{avatar}.png"
                : null;

            return new OAuthResponseDto
            {
                ProviderId = userId,
                Provider = "Discord",
                Email = root.GetProperty("email").GetString() ?? string.Empty,
                DisplayName = root.GetProperty("username").GetString() ?? string.Empty,
                AvatarUrl = avatarUrl,
            };
        }
        catch
        {
            return null;
        }
    }

    public async Task<OAuthCallbackResponseDto> HandleOAuthCallbackAsync(OAuthResponseDto oauthUser)
    {
        try
        {
            User? existingUser = null;

            if (oauthUser.Provider == "Google")
            {
                existingUser =
                    await _userManager.FindByEmailAsync(oauthUser.Email)
                    ?? await _userManager.Users.FirstOrDefaultAsync(u =>
                        u.GoogleId == oauthUser.ProviderId
                    );
            }
            else if (oauthUser.Provider == "Discord")
            {
                existingUser =
                    await _userManager.FindByEmailAsync(oauthUser.Email)
                    ?? await _userManager.Users.FirstOrDefaultAsync(u =>
                        u.DiscordId == oauthUser.ProviderId
                    );
            }

            bool isNewUser = false;

            if (existingUser == null)
            {
                isNewUser = true;
                existingUser = new User
                {
                    Email = oauthUser.Email,
                    // Викликаємо оновлений бронебійний метод генерації
                    UserName = GenerateUsername(oauthUser.Email),
                    AvatarUrl = oauthUser.AvatarUrl,
                    EmailConfirmed = true,
                    OAuthProvider = oauthUser.Provider,
                };

                if (oauthUser.Provider == "Google")
                    existingUser.GoogleId = oauthUser.ProviderId;
                else if (oauthUser.Provider == "Discord")
                    existingUser.DiscordId = oauthUser.ProviderId;

                var createResult = await _userManager.CreateAsync(existingUser);
                if (!createResult.Succeeded)
                {
                    return new OAuthCallbackResponseDto
                    {
                        IsSuccess = false,
                        Errors = createResult.Errors.Select(e => e.Description),
                    };
                }
            }
            else
            {
                if (oauthUser.Provider == "Google" && string.IsNullOrEmpty(existingUser.GoogleId))
                {
                    existingUser.GoogleId = oauthUser.ProviderId;
                }
                else if (
                    oauthUser.Provider == "Discord"
                    && string.IsNullOrEmpty(existingUser.DiscordId)
                )
                {
                    existingUser.DiscordId = oauthUser.ProviderId;
                }

                if (
                    !string.IsNullOrEmpty(oauthUser.AvatarUrl)
                    && string.IsNullOrEmpty(existingUser.AvatarUrl)
                )
                {
                    existingUser.AvatarUrl = oauthUser.AvatarUrl;
                }

                var updateResult = await _userManager.UpdateAsync(existingUser);
                if (!updateResult.Succeeded)
                {
                    return new OAuthCallbackResponseDto
                    {
                        IsSuccess = false,
                        Errors = updateResult.Errors.Select(e => e.Description),
                    };
                }
            }

            var token = GenerateJwtToken(existingUser);

            return new OAuthCallbackResponseDto
            {
                IsSuccess = true,
                Token = token,
                IsNewUser = isNewUser,
                Email = existingUser.Email,
                DisplayName = existingUser.UserName,
            };
        }
        catch (Exception ex)
        {
            return new OAuthCallbackResponseDto
            {
                IsSuccess = false,
                Errors = new[] { $"OAuth callback processing failed: {ex.Message}" },
            };
        }
    }

    // ОНОВЛЕНО: Жорстка фільтрація для Identity
    private string GenerateUsername(string email)
    {
        // Беремо префікс пошти
        var baseUsername = email.Split('@')[0];

        // Залишаємо виключно англійські літери та цифри (ніякої кирилиці чи пробілів)
        var validChars = baseUsername.Where(char.IsAsciiLetterOrDigit).ToArray();
        var sanitizedBase = new string(validChars);

        // Якщо пошта складалася лише зі спецсимволів (що майже неможливо, але ми маємо бути параноїками)
        if (string.IsNullOrEmpty(sanitizedBase))
        {
            sanitizedBase = "user";
        }

        // Додаємо 8 символів унікального хешу
        return $"{sanitizedBase}_{Guid.NewGuid().ToString()[..8]}";
    }

    // ОНОВЛЕНО: Захист від відсутності ключа
    private string GenerateJwtToken(User user)
    {
        var jwtKey = _config["Jwt:Key"];
        if (string.IsNullOrEmpty(jwtKey) || jwtKey.Length < 32)
        {
            throw new InvalidOperationException(
                "CRITICAL ERROR: Jwt:Key is missing in configuration or is less than 32 characters long."
            );
        }

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName!),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
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
