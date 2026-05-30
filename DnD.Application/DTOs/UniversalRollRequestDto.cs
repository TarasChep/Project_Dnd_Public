using DnD.Domain.Enums;

namespace DnD.Application.DTOs;

public class UniversalRollRequestDto
{
    public RollType Type { get; set; } // Attack, SkillCheck, SavingThrow, Custom

    // Опціональні параметри залежно від типу кидка
    public RollableSkill? Skill { get; set; }
    public StatType? Stat { get; set; }

    // ТІЛЬКИ ENUM! Жодних вільних чисел для граней.
    public DiceType? DiceSides { get; set; }

    // Максимум кубиків за один раз (наприклад, Fireball це 8d6, треба обмежити лімітом, скажімо 50)
    public int? DiceCount { get; set; }

    public int? FlatModifier { get; set; } // Будь-які кастомні бонуси (+2 від меча)
}
