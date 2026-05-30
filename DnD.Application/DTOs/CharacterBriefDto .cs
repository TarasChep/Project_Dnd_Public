namespace DnD.Application.DTOs;

/// <summary>
/// Об'єкт передачі даних для короткого представлення персонажа.
/// Використовується на сторінці вибору героїв.
/// </summary>
public class CharacterBriefDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Race { get; set; } = string.Empty;
    public string Class { get; set; } = string.Empty;
    public int Level { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime LastModifiedAt { get; set; }

    public int CurrentHp { get; set; }
    public int MaxHp { get; set; }
    public int TemporaryHp { get; set; } // ДОДАТИ ЦЕ
}
