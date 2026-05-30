namespace DnD.Domain.Entities;

public static class ExperienceTable
{
    // Єдине джерело істини. Індекс масиву = Рівень персонажа.
    // 0-й індекс ігноруємо для зручності (щоб 1-й рівень був під індексом 1).
    private static readonly int[] Thresholds =
    {
        0, // [0] Заглушка
        0, // [1] Level 1
        300, // [2] Level 2
        900, // [3] Level 3
        2700, // [4] Level 4
        6500, // [5] Level 5
        14000, // [6] Level 6
        23000, // [7] Level 7
        34000, // [8] Level 8
        48000, // [9] Level 9
        64000, // [10] Level 10
        85000, // [11] Level 11
        100000, // [12] Level 12
        120000, // [13] Level 13
        
        140000, // [14] Level 14
        165000, // [15] Level 15
        195000, // [16] Level 16
        225000, // [17] Level 17
        265000, // [18] Level 18
        305000, // [19] Level 19
        355000, // [20] Level 20
    };

    /// <summary>
    /// Твій старий метод, але тепер без switch-костилів.
    /// Повертає мінімальний поріг XP для заданого рівня.
    /// </summary>
    public static int GetThreshold(int level)
    {
        if (level < 1)
            return Thresholds[1];
        if (level > 20)
            return Thresholds[20];

        return Thresholds[level];
    }

    /// <summary>
    /// Новий метод: Визначає Рівень та XP до наступного рівня на основі поточного досвіду.
    /// </summary>
    public static (int Level, int NextLevelXp) CalculateLevelInfo(int currentXp)
    {
        if (currentXp < 0)
            currentXp = 0;

        for (int level = 1; level <= 20; level++)
        {
            // Якщо це максимальний рівень, або поточний XP менший за поріг НАСТУПНОГО рівня
            if (level == 20 || currentXp < Thresholds[level + 1])
            {
                int nextXp = level == 20 ? 0 : Thresholds[level + 1];
                return (level, nextXp);
            }
        }

        return (20, 0); // Запобіжник
    }
}
