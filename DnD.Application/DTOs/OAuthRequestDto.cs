namespace DnD.Application.DTOs;

/// <summary>
/// DTO for OAuth callback with authorization code
/// </summary>
public class OAuthRequestDto
{
    /// <summary>
    /// Authorization code from OAuth provider (Google, Discord)
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// State parameter for CSRF protection
    /// </summary>
    public string? State { get; set; }
    
    /// <summary>
    /// Redirect URI used in the OAuth request
    /// </summary>
    public string RedirectUri { get; set; } = string.Empty;
}
