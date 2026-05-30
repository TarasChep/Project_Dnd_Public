using DnD.Application.DTOs;
using DnD.Domain.Entities;

namespace DnD.Application.Interfaces;

public interface IDiscordNotificationService
{
    Task SendRollAsync(Character character, string rollTitle, RollResponseDto rollResult);
    Task SendGenericRollAsync(string webhookUrl, string username, string? avatarUrl, string rollTitle, RollResponseDto rollResult, string? themeColor = null);
}