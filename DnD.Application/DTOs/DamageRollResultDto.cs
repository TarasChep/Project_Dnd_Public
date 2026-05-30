namespace DnD.Application.DTOs;

public class DamageRollResultDto
{
    // Тип шкоди (наприклад, "Slashing" або "Fire")
    public string DamageType { get; set; } = string.Empty;
    
    // Результат кидка кубиків для цього типу шкоди
    public RollResponseDto Roll { get; set; } = null!;
}