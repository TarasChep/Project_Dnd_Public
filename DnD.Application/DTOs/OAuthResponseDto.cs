namespace DnD.Application.DTOs;

/// <summary>
/// Response DTO containing OAuth user data from external providers
/// </summary>
public class OAuthResponseDto
{
    /// <summary>
    /// Unique identifier from the OAuth provider
    /// </summary>
    public string ProviderId { get; set; } = string.Empty;
    
    /// <summary>
    /// Provider name (Google, Discord)
    /// </summary>
    public string Provider { get; set; } = string.Empty;
    
    /// <summary>
    /// User's email from OAuth provider
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// User's display name from OAuth provider
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
    
    /// <summary>
    /// User's avatar URL from OAuth provider
    /// </summary>
    public string? AvatarUrl { get; set; }
}
