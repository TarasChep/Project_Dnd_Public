namespace DnD.Application.DTOs;

public class ResourceTrackerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CurrentValue { get; set; }
    public int MaxValue { get; set; }
    public string ResetCondition { get; set; } = string.Empty; // Передаємо Enum як текст
}
