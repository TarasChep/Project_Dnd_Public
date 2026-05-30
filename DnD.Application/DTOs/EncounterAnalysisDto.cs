namespace DnD.Application.DTOs;

public class EncounterAnalysisDto
{
    public Guid EncounterId { get; set; }
    public string EncounterName { get; set; } = string.Empty;
    
    // Аналітика Групи
    public int PartyTotalHp { get; set; }
    public double PartyAvgAc { get; set; }
    public double PartyTotalEdpr { get; set; } // Очікувана шкода за раунд
    public double PartyTtk { get; set; } // Скільки раундів проживе група (Time to Kill)

    // Аналітика Ворогів
    public int EnemyTotalHp { get; set; }
    public double EnemyAvgAc { get; set; }
    public double EnemyTotalEdpr { get; set; }
    public double EnemyTtk { get; set; } // Скільки раундів проживуть вороги

    // Вердикт для Майстра
    public string Verdict { get; set; } = string.Empty;

    public List<ParticipantAnalysisDto> Participants { get; set; } = new();
}

public class ParticipantAnalysisDto
{
    public string Name { get; set; } = string.Empty;
    public string Faction { get; set; } = string.Empty;
    public int Hp { get; set; }
    public int Ac { get; set; }
    public double Edpr { get; set; } // Скільки шкоди генерує саме цей юніт
}