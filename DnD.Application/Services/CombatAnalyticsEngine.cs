using System;
using System.Linq;
using DnD.Application.DTOs;
using DnD.Application.Interfaces;
using DnD.Domain.Enums;

namespace DnD.Application.Services;

public class CombatAnalyticsEngine : ICombatAnalyticsEngine
{
    public EncounterPredictionReport AnalyzeEncounter(EncounterAnalyticsDto payload)
    {
        var report = new EncounterPredictionReport();

        if (!payload.Allies.Any() || !payload.Enemies.Any())
        {
            report.WarningMessages.Add("Insufficient participants to calculate a valid Threat Ratio.");
            return report;
        };

        // --- 1. TARGET BASELINES ---
        // Calculate the average Armor Class of the opposing factions.
        // This establishes the baseline difficulty for landing a successful attack.
        double enemyAvgAc = payload.Enemies.Average(e => e.ArmorClass);
        double allyAvgAc = payload.Allies.Average(a => a.ArmorClass);

        // --- 2. PARTICIPANT eDPR CALCULATION ---
        // Calculate the Total Turn eDPR (Expected Damage Per Round) for all participants,
        // respecting the Action Economy (1 Action + 1 Bonus Action max per turn).
        double alliesTotalDpr = 0;
        foreach (var ally in payload.Allies)
        {
            alliesTotalDpr += CalculateParticipantTurnDpr(ally, enemyAvgAc, payload.Enemies.Count);
        }

        double enemiesTotalDpr = 0;
        foreach (var enemy in payload.Enemies)
        {
            enemiesTotalDpr += CalculateParticipantTurnDpr(enemy, allyAvgAc, payload.Allies.Count);
        }

        // --- 3. TIME TO KILL (TTK) CALCULATION ---
        // Based on Lanchester's Laws, we calculate how many rounds it takes 
        // for one faction to completely deplete the HP of the other faction.
        double enemiesTotalHp = payload.Enemies.Sum(e => e.CurrentHp);
        double alliesTotalHp = payload.Allies.Sum(a => a.CurrentHp);

        // Prevent divide-by-zero errors by clamping minimum DPR to 0.001
        report.AlliesEstimatedTTK = enemiesTotalHp / Math.Max(0.001, alliesTotalDpr);
        report.EnemiesEstimatedTTK = alliesTotalHp / Math.Max(0.001, enemiesTotalDpr);

        // --- 4. THREAT RATIO ---
        // A Threat Ratio > 1.0 means enemies die faster than allies (Advantage: Allies).
        // A Threat Ratio < 1.0 means allies die faster than enemies (Advantage: Enemies).
        report.ThreatRatio = report.EnemiesEstimatedTTK / Math.Max(0.001, report.AlliesEstimatedTTK);
        report.SurvivalProbabilityPercentage = (int)Math.Clamp((report.ThreatRatio - 0.5) * 100, 0, 100);

        // --- 5. CATEGORIZE DIFFICULTY ---
        if (report.ThreatRatio > 1.5)
            report.DifficultyCategory = "Easy";
        else if (report.ThreatRatio >= 1.1)
            report.DifficultyCategory = "Balanced";
        else if (report.ThreatRatio >= 0.8)
            report.DifficultyCategory = "Hard";
        else if (report.ThreatRatio >= 0.5)
            report.DifficultyCategory = "Deadly";
        else
            report.DifficultyCategory = "Party Wipe";

        return report;
    }

    private double CalculateParticipantTurnDpr(ParticipantAnalyticsDto participant, double targetAc, int targetCount)
    {
        double maxActionDpr = 0;
        double maxBonusActionDpr = 0;

        foreach (var action in participant.AvailableActions)
        {
            double eDpr;

            // Step A: Calculate single-target eDPR
            if (action.IsSave)
            {
                // Baseline MVP: Saving throw actions hit with a flat 60% probability
                eDpr = action.AverageDamage * 0.60;
            }
            else
            {
                // Standard D20 Hit Probability Formula:
                // Chance = (21 - (TargetAC - AttackBonus)) / 20.0
                // Bounded between 5% (Natural 1 always misses) and 95% (Natural 20 always hits)
                double hitChance = Math.Max(0.05, Math.Min(0.95, (21.0 - (targetAc - action.AttackBonus)) / 20.0));
                eDpr = action.AverageDamage * hitChance;
            }

            // Step B: Apply Area of Effect (AoE) Multipliers
            if (action.IsAoE)
            {
                // Target saturation heuristics: Estimates how many targets are caught in the blast radius
                double gridSquares = action.AoESizeFeet / 5.0;
                double aoeMultiplier = Math.Min((double)targetCount, gridSquares + ((targetCount - gridSquares) / 3.0));
                
                // Multiply the eDPR by the estimated number of targets hit (min 1)
                eDpr *= Math.Max(1.0, aoeMultiplier);
            }

            // Step C: Action Economy Filter
            // Find the most optimal move for the Action and Bonus Action slots.
            if (action.ActionCost == ActionCost.Action && eDpr > maxActionDpr) 
                maxActionDpr = eDpr;
            else if (action.ActionCost == ActionCost.BonusAction && eDpr > maxBonusActionDpr) 
                maxBonusActionDpr = eDpr;
        }

        // Combine the optimal Action and Bonus Action for total expected turn damage
        return maxActionDpr + maxBonusActionDpr;
    }

    public double CalculateExpectedDamagePerAttack(int attackBonus, int targetAc, int diceCount, int diceType, int flatBonus)
    {
        // Calculate the target number to roll on a d20 to hit the AC.
        int targetRoll = targetAc - attackBonus;
        
        // A natural 1 always misses, a natural 20 always hits.
        if (targetRoll < 2) targetRoll = 2;
        if (targetRoll > 20) targetRoll = 20;

        // Calculate probability of a normal hit and a critical hit
        double hitChance = (21 - targetRoll) / 20.0;
        double critChance = 1.0 / 20.0;

        // Subtract crit chance from hit chance to get normal hit chance
        double normalHitChance = hitChance - critChance;

        // Calculate average damage on a normal hit
        double avgDiceDamage = (diceType + 1) / 2.0; // e.g. d8 -> 4.5
        double normalDamage = (diceCount * avgDiceDamage) + flatBonus;

        // Calculate average damage on a critical hit (double dice)
        double critDamage = (diceCount * 2 * avgDiceDamage) + flatBonus;

        // Calculate total expected damage
        return (normalHitChance * normalDamage) + (critChance * critDamage);
    }
}