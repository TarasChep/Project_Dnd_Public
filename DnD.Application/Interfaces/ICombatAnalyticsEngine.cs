using DnD.Application.DTOs;

namespace DnD.Application.Interfaces;

public interface ICombatAnalyticsEngine
{
    EncounterPredictionReport AnalyzeEncounter(EncounterAnalyticsDto payload);
    double CalculateExpectedDamagePerAttack(int attackBonus, int targetAc, int diceCount, int diceType, int flatBonus);
}