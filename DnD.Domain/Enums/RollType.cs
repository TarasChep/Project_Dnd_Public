namespace DnD.Domain.Enums;

public enum RollType
{
    Custom = 0, // Наприклад, просто кинути 8d6 для Fireball
    StatCheck = 1, // Перевірка характеристики (Strength, Dex...)
    SavingThrow = 2, // Рятівний кидок
    SkillCheck = 3, // Перевірка навички (Acrobatics, Stealth...)
    Attack = 4, // Кидок атаки
    Initiative = 5, // Кидок ініціативи
}
