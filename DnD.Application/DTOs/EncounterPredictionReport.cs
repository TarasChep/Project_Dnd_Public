namespace DnD.Application.DTOs;

public class EncounterPredictionReport
{
    public double ThreatRatio { get; set; }
    public int SurvivalProbabilityPercentage { get; set; }
    public string DifficultyCategory { get; set; } = string.Empty;
    public double AlliesEstimatedTTK { get; set; }
    public double EnemiesEstimatedTTK { get; set; }
    public List<string> WarningMessages { get; set; } = new();
}