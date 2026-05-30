namespace DnD.Application.Interfaces;

public interface IDiscordWebhookService
{
    Task SendInitiativeRollAsync(string characterName, int roll, string? webhookUrl = null);
    Task SendMessageAsync(string message, string? webhookUrl = null);
}