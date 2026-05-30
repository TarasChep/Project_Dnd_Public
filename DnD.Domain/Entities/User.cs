using Microsoft.AspNetCore.Identity;
using Microsoft.VisualBasic;

namespace DnD.Domain.Entities;

public class User : IdentityUser<Guid>
{
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // OAuth provider information
    public string? GoogleId { get; set; }
    public string? DiscordId { get; set; }
    public string? OAuthProvider { get; set; } // "Google" or "Discord"

    public List<Character> Characters { get; set; } = new();
}
