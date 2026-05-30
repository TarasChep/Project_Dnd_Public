namespace DnD.Application.DTOs;

// Це той самий допоміжний клас, просто живе в цьому ж файлі для зручності
public class RollDetailDto
{
    public int Value { get; set; }
    public int Sides { get; set; }
    public string Formatted => $"{Value}/{Sides}";
}

// Твій старий DTO, але на стероїдах
public class RollResponseDto
{
    public string RollName { get; set; } = string.Empty;
    public List<RollDetailDto> Rolls { get; set; } = new();

    public int Modifier { get; set; }

    // ДОДАЄМО ЦЕ: Пояснення, звідки взявся модифікатор (напр. "2 (STR) + 2 (PROF)")
    public string ModifierBreakdown { get; set; } = string.Empty;

    public int Total => Rolls.Sum(r => r.Value) + Modifier;

    // ЗМІНЮЄМО ЦЕ: Тепер expression показує розбитий модифікатор
    public string Expression =>
        string.IsNullOrWhiteSpace(ModifierBreakdown)
            ? $"{string.Join(" + ", Rolls.Select(r => r.Formatted))} + {Modifier} = {Total}"
            : $"{string.Join(" + ", Rolls.Select(r => r.Formatted))} + {ModifierBreakdown} = {Total}";
}
