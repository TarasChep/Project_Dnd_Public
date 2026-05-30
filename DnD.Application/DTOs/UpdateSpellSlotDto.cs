namespace DnD.Application.DTOs;

public class UpdateSpellSlotDto
{
    public int? Level { get; set; }
    public int? MaxValue { get; set; }
    public int? CurrentValue { get; set; }
    public int? AdjustValue { get; set; }
}