namespace DnD.Application.DTOs;

public class HitDiceResultDto
{
    public List<RollDetailDto> Rolls { get; set; } = new();

    public int ConstitutionModifier { get; set; }
    public int TotalHealed { get; set; }
    public int NewCurrentHp { get; set; }
    public int HitDiceRemaining { get; set; }
}
