namespace DnD.Application.DTOs;

public class SpellSlotDto
{
    public Guid Id { get; set; }
    public int Level { get; set; }
    public int MaxValue { get; set; }
    public int CurrentValue { get; set; }
}