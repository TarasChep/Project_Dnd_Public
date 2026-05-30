using System.Data;
using System.Reflection.PortableExecutable;
using DnD.Domain.Entities;
using DnD.Domain.Enums;

namespace DnD.Domain.Services;

public static class DnDCalculator
{
    public static int CalculateModifier(int statValue)
    {
        double res = (statValue - 10) / 2.0;
        return (int)Math.Floor(res);
    }

    public static int CalculateProficiency(int level)
    {
        double res = 1 + (level / 4.0);
        return (int)Math.Ceiling(res);
    }

    public static int GetTotalSkillBonus(int statValue, int level, SkillProficiency prof)
    {
        int statModifier = CalculateModifier(statValue);
        int proficiencyBonus = CalculateProficiency(level);
        return statModifier + (proficiencyBonus * (int)prof);
    }

    public static int CalculateSkillBonus(Character character, RollableSkill skill)
    {
        return skill switch
        {
            // --- Сила (Strength) ---
            RollableSkill.Athletics => GetTotalSkillBonus(
                character.Strength,
                character.Level,
                character.Athletics
            ),

            // --- Спритність (Dexterity) ---
            RollableSkill.Acrobatics => GetTotalSkillBonus(
                character.Dexterity,
                character.Level,
                character.Acrobatics
            ),
            RollableSkill.SleightOfHand => GetTotalSkillBonus(
                character.Dexterity,
                character.Level,
                character.SleightOfHand
            ),
            RollableSkill.Stealth => GetTotalSkillBonus(
                character.Dexterity,
                character.Level,
                character.Stealth
            ),

            // --- Інтелект (Intelligence) ---
            RollableSkill.Arcana => GetTotalSkillBonus(
                character.Intelligence,
                character.Level,
                character.Arcana
            ),
            RollableSkill.History => GetTotalSkillBonus(
                character.Intelligence,
                character.Level,
                character.History
            ),
            RollableSkill.Investigation => GetTotalSkillBonus(
                character.Intelligence,
                character.Level,
                character.Investigation
            ),
            RollableSkill.Nature => GetTotalSkillBonus(
                character.Intelligence,
                character.Level,
                character.Nature
            ),
            RollableSkill.Religion => GetTotalSkillBonus(
                character.Intelligence,
                character.Level,
                character.Religion
            ),

            // --- Мудрість (Wisdom) ---
            RollableSkill.AnimalHandling => GetTotalSkillBonus(
                character.Wisdom,
                character.Level,
                character.AnimalHandling
            ),
            RollableSkill.Insight => GetTotalSkillBonus(
                character.Wisdom,
                character.Level,
                character.Insight
            ),
            RollableSkill.Medicine => GetTotalSkillBonus(
                character.Wisdom,
                character.Level,
                character.Medicine
            ),
            RollableSkill.Perception => GetTotalSkillBonus(
                character.Wisdom,
                character.Level,
                character.Perception
            ),
            RollableSkill.Survival => GetTotalSkillBonus(
                character.Wisdom,
                character.Level,
                character.Survival
            ),

            // --- Харизма (Charisma) ---
            RollableSkill.Deception => GetTotalSkillBonus(
                character.Charisma,
                character.Level,
                character.Deception
            ),
            RollableSkill.Intimidation => GetTotalSkillBonus(
                character.Charisma,
                character.Level,
                character.Intimidation
            ),
            RollableSkill.Performance => GetTotalSkillBonus(
                character.Charisma,
                character.Level,
                character.Performance
            ),
            RollableSkill.Persuasion => GetTotalSkillBonus(
                character.Charisma,
                character.Level,
                character.Persuasion
            ),

            _ => throw new ArgumentOutOfRangeException(
                nameof(skill),
                $"Навичка {skill} не підтримується калькулятором."
            ),
        };
    }

    /// <summary>
    /// Рахує чисту перевірку характеристики (Ability Check). Ніяких бонусів майстерності.
    /// </summary>
    public static int CalculateAbilityCheckBonus(Character character, StatType stat)
    {
        return stat switch
        {
            StatType.Strength => CalculateModifier(character.Strength),
            StatType.Dexterity => CalculateModifier(character.Dexterity),
            StatType.Constitution => CalculateModifier(character.Constitution),
            StatType.Intelligence => CalculateModifier(character.Intelligence),
            StatType.Wisdom => CalculateModifier(character.Wisdom),
            StatType.Charisma => CalculateModifier(character.Charisma),
            _ => throw new ArgumentOutOfRangeException(nameof(stat)),
        };
    }

    public static int CalculateSavingThrowBonus(Character character, StatType stat)
    {
        int pb = character.proficiencyBonus; // Беремо з твого [NotMapped] поля

        return stat switch
        {
            StatType.Strength => CalculateModifier(character.Strength)
                + (character.IsStrengthSaveProficient ? pb : 0),
            StatType.Dexterity => CalculateModifier(character.Dexterity)
                + (character.IsDexteritySaveProficient ? pb : 0),
            StatType.Constitution => CalculateModifier(character.Constitution)
                + (character.IsConstitutionSaveProficient ? pb : 0),
            StatType.Intelligence => CalculateModifier(character.Intelligence)
                + (character.IsIntelligenceSaveProficient ? pb : 0),
            StatType.Wisdom => CalculateModifier(character.Wisdom)
                + (character.IsWisdomSaveProficient ? pb : 0),
            StatType.Charisma => CalculateModifier(character.Charisma)
                + (character.IsCharismaSaveProficient ? pb : 0),
            _ => throw new ArgumentOutOfRangeException(nameof(stat)),
        };
    }

    /// <summary>
    /// Розраховує Initiative для персонажа відповідно до D&D 5e правил.
    /// Initiative = DEX modifier + additionalBonus
    /// </summary>
    public static int CalculateInitiative(int dexterity, int additionalBonus = 0)
    {
        return CalculateModifier(dexterity) + additionalBonus;
    }
}
