namespace DnD.Application.DTOs;

/// <summary>
/// Response DTO for OAuth login/registration
/// </summary>
public class OAuthCallbackResponseDto
{
    /// <summary>
    /// Indicates if the operation was successful
    /// </summary>
    public bool IsSuccess { get; set; }
    
    /// <summary>
    /// JWT token for authenticated requests
    /// </summary>
    public string? Token { get; set; }
    
    /// <summary>
    /// Indicates if the user is newly created
    /// </summary>
    public bool IsNewUser { get; set; }
    
    /// <summary>
    /// User's email
    /// </summary>
    public string? Email { get; set; }
    
    /// <summary>
    /// User's display name
    /// </summary>
    public string? DisplayName { get; set; }
    
    /// <summary>
    /// Error messages if operation failed
    /// </summary>
    public IEnumerable<string>? Errors { get; set; }
}
