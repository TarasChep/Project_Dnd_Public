namespace DnD.Domain.Enums;

/// <summary>
/// The level of Proficiency of the skill like a [Athleticks]
/// <summary>
public enum SkillProficiency
{
    None = 0, // roll + (0 * bonus)
    Proficient = 1, // roll + (1 * bonus )
    Expertise = 2, // roll + (2 * bonus )
}
