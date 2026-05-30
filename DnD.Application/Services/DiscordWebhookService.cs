using System.Net.Http.Json;
using DnD.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace DnD.Application.Services;

public class DiscordWebhookService : IDiscordWebhookService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public DiscordWebhookService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task SendInitiativeRollAsync(string characterName, int roll, string? webhookUrl = null)
    {
        var url = webhookUrl ?? _configuration["Discord:InitiativeWebhookUrl"] ?? _configuration["DiscordWebhookUrl"];
        if (string.IsNullOrWhiteSpace(url)) return;

        var payload = new { content = $"🎲 **{characterName}** rolled initiative: **{roll}**" };
        
        try { await _httpClient.PostAsJsonAsync(url, payload); }
        catch { /* Fire and forget, prevent failure from crashing game flow */ }
    }

    public async Task SendMessageAsync(string message, string? webhookUrl = null)
    {
        var url = webhookUrl ?? _configuration["Discord:InitiativeWebhookUrl"] ?? _configuration["DiscordWebhookUrl"];
        if (string.IsNullOrWhiteSpace(url)) return;

        var payload = new { content = message };
        
        try { await _httpClient.PostAsJsonAsync(url, payload); }
        catch { /* Fire and forget, prevent failure from crashing game flow */ }
    }
}