namespace DnD.Application.DTOs;

public class EncounterDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int CurrentTurnIndex { get; set; }
    public List<ParticipantDetailDto> Participants { get; set; } = new();
}