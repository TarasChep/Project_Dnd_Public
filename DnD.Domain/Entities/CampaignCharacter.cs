namespace DnD.Domain.Entities;

public class CampaignCharacter
{
    public Guid CampaignId { get; set; }
    public Campaign Campaign { get; set; } = null!;

    public Guid CharacterId { get; set; }
    public Character Character { get; set; } = null!;

    // Може бути null, якщо персонаж лежить у "корені" кампанії
    public Guid? FolderId { get; set; }
    public CampaignFolder? Folder { get; set; }

    // Визначає, чи це гравець (PC) чи NPC/Монстр (якого додав майстер)
    public bool IsPlayerCharacter { get; set; }

    // Визначає, чи бачать всі гравці цього персонажа (наприклад, відкритий NPC)
    public bool IsVisibleToAllPlayers { get; set; } = false;

    // Список конкретних гравців (їх UserId), яким Майстер дав персональний доступ (фамільяр, секретний союзник)
    public List<Guid> VisibleToUserIds { get; set; } = new();
}