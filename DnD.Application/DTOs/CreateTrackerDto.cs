using DnD.Domain.Enums;

namespace DnD.Application.DTOs;

public class CreateTrackerDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int MaxValue { get; set; }
    public ResetCondition ResetCondition { get; set; }
}
