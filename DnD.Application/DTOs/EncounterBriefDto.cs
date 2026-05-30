namespace DnD.Application.DTOs;

public class EncounterBriefDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int PlayerCount { get; set; }
    public int EnemyCount { get; set; }
}