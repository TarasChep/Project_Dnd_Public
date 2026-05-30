using DnD.Application.DTOs;

namespace DnD.Application.Interfaces;

public interface IOAuthService
{
    /// <summary>
    /// Gets the OAuth authorization URL for Google
    /// </summary>
    string GetGoogleAuthorizationUrl(string state, string redirectUri);
    
    /// <summary>
    /// Gets the OAuth authorization URL for Discord
    /// </summary>
    string GetDiscordAuthorizationUrl(string state, string redirectUri);
    
    /// <summary>
    /// Exchanges Google authorization code for user info
    /// </summary>
    Task<OAuthResponseDto?> GetGoogleUserInfoAsync(string code, string redirectUri);
    
    /// <summary>
    /// Exchanges Discord authorization code for user info
    /// </summary>
    Task<OAuthResponseDto?> GetDiscordUserInfoAsync(string code, string redirectUri);
    
    /// <summary>
    /// Handles OAuth login or registration
    /// </summary>
    Task<OAuthCallbackResponseDto> HandleOAuthCallbackAsync(OAuthResponseDto oauthUser);
}
