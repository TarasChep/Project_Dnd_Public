using DnD.Domain.Enums;

namespace DnD.Application.DTOs;

public class UpdateTrackerDto
{
    // Назви тепер 1-в-1 з твоїм ResourceTrackerDto
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? MaxValue { get; set; }
    public int? CurrentValue { get; set; }
    public int? AdjustValue { get; set; }

    // Приймаємо Enum. Якщо з фронта прилетить рядок "ShortRest"
    // і в тебе стоїть JsonStringEnumConverter в Program.cs, він сам його розпарсить.
    public ResetCondition? ResetCondition { get; set; }
}
