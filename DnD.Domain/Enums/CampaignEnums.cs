namespace DnD.Domain.Enums;

public enum CampaignRole
{
    GM = 1,
    Player = 2
}

public enum CampaignMemberStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3
}

// Зауважте: Якщо Enum Faction (Player, Enemy, Neutral) вже існує в системі, 
// цей файл доповнює його. Якщо ні, він має бути визначений в існуючих файлах.