using System.Net.Http.Json;
using DnD.Application.DTOs;
using DnD.Application.DTOs.Integrations;
using DnD.Application.Interfaces;
using DnD.Domain.Entities;

namespace DnD.Application.Services;

public class DiscordNotificationService : IDiscordNotificationService
{
    private readonly HttpClient _httpClient;

    public DiscordNotificationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task SendRollAsync(
        Character character,
        string rollTitle,
        RollResponseDto rollResult
    )
    {
        // 1. Захист: Якщо у персонажа немає вебхука — просто виходимо, нічого не робимо.
        if (string.IsNullOrWhiteSpace(character.DiscordWebhookUrl))
            return;

        // 2. Парсинг кольору з HEX (#FF5733) у десяткове число для Discord
        int embedColor = 3447003; // Дефолтний синій на випадок помилки
        try
        {
            if (!string.IsNullOrWhiteSpace(character.ThemeColor))
            {
                string hex = character.ThemeColor.Replace("#", "");
                embedColor = Convert.ToInt32(hex, 16);
            }
        }
        catch
        { /* Ігноруємо помилки парсингу кольору */
        }

        // 3. Математика кидка і форматування твого рядка
        int diceSum = rollResult.Rolls.Sum(r => r.Value); // Сума кубиків (якщо їх кілька)
        int total = diceSum + rollResult.Modifier;
        int sides = rollResult.Rolls.FirstOrDefault()?.Sides ?? 20;

        // Твій шаблон: "(10 + 7) -- 17 \n *(1d20 + 7)*"
        string description =
            $"**({diceSum} + {rollResult.Modifier}) = {total}**\n*(1d{sides} + {rollResult.Modifier})*";

        // 4. Формуємо JSON Payload
        var payload = new DiscordWebhookPayload
        {
            Username = character.Name, // Бот писатиме від імені персонажа
            // AvatarUrl = ... (Можеш додати URL аватарки, якщо вона є у сутності Character)
            Embeds = new List<DiscordEmbed>
            {
                new DiscordEmbed
                {
                    Title = $"🎲 {rollTitle}",
                    Color = embedColor,
                    Description = description,
                    Thumbnail = !string.IsNullOrWhiteSpace(character.ImageUrl) 
                        ? new DiscordThumbnail { Url = character.ImageUrl }
                        : null,
                    Author = new DiscordAuthor
                    {
                        Name = character.Name
                    },
                },
            },
        };

        // 5. Відправка (Fire-and-Forget)
        try
        {
            await _httpClient.PostAsJsonAsync(character.DiscordWebhookUrl, payload);
        }
        catch (Exception ex)
        {
            // Якщо Discord лежить — ми просто логуємо це в консоль і не ламаємо бекенд
            Console.WriteLine($"[Discord Webhook Error]: {ex.Message}");
        }
    }

    public async Task SendGenericRollAsync(
        string webhookUrl,
        string username,
        string? avatarUrl,
        string rollTitle,
        RollResponseDto rollResult,
        string? themeColor = null
    )
    {
        if (string.IsNullOrWhiteSpace(webhookUrl))
            return;

        int embedColor = 3447003;
        try
        {
            if (!string.IsNullOrWhiteSpace(themeColor))
            {
                string hex = themeColor.Replace("#", "");
                embedColor = Convert.ToInt32(hex, 16);
            }
        }
        catch { }

        int diceSum = rollResult.Rolls.Sum(r => r.Value);
        int total = diceSum + rollResult.Modifier;
        int sides = rollResult.Rolls.FirstOrDefault()?.Sides ?? 20;

        string description = $"**({diceSum} + {rollResult.Modifier}) = {total}**\n*(1d{sides} + {rollResult.Modifier})*";

        var payload = new DiscordWebhookPayload
        {
            Username = username,
            Embeds = new List<DiscordEmbed>
            {
                new DiscordEmbed
                {
                    Title = $"🎲 {rollTitle}",
                    Color = embedColor,
                    Description = description,
                    Thumbnail = !string.IsNullOrWhiteSpace(avatarUrl) ? new DiscordThumbnail { Url = avatarUrl } : null,
                    Author = new DiscordAuthor { Name = username },
                },
            },
        };

        try
        {
            await _httpClient.PostAsJsonAsync(webhookUrl, payload);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Discord Webhook Error]: {ex.Message}");
        }
    }
}
