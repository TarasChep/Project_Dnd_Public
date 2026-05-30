using System.Text.Json.Serialization;

namespace DnD.Application.DTOs.Integrations;

public class DiscordWebhookPayload
{
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("avatar_url")]
    public string? AvatarUrl { get; set; }

    [JsonPropertyName("embeds")]
    public List<DiscordEmbed> Embeds { get; set; } = new();
}

public class DiscordEmbed
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("color")]
    public int Color { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("thumbnail")]
    public DiscordThumbnail? Thumbnail { get; set; }

    [JsonPropertyName("author")]
    public DiscordAuthor? Author { get; set; }
}

public class DiscordThumbnail
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
}

public class DiscordAuthor
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("icon_url")]
    public string? IconUrl { get; set; }
}
