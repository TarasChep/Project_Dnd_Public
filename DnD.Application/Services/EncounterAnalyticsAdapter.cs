using DnD.Application.DTOs;
using DnD.Application.Interfaces;
using DnD.Domain.Enums;
using DnD.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DnD.Application.Services;

public class EncounterAnalyticsAdapter : IEncounterAnalyticsAdapter
{
    private readonly ApplicationDbContext _context;

    public EncounterAnalyticsAdapter(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EncounterAnalyticsDto> GetAnalyticsPayloadAsync(Guid encounterId)
    {
        // Fetch the Encounter, eagerly loading all character capabilities to prevent N+1
        var encounter = await _context.Encounters
            .Include(e => e.Participants)
                .ThenInclude(p => p.Character)
                    .ThenInclude(c => c!.Attacks)
                        .ThenInclude(a => a.Damages)
            .Include(e => e.Participants)
                .ThenInclude(p => p.Character)
                    .ThenInclude(c => c!.Attacks)
                        .ThenInclude(a => a.Spell)
            .FirstOrDefaultAsync(e => e.Id == encounterId);

        if (encounter == null)
        {
            throw new Exception("Encounter not found.");
        }

        var payload = new EncounterAnalyticsDto();
        var livingParticipants = encounter.Participants.Where(p => p.CurrentHp > 0);

        foreach (var participant in livingParticipants)
        {
            var participantDto = new ParticipantAnalyticsDto
            {
                ParticipantId = participant.Id,
                Faction = participant.Faction,
                CurrentHp = participant.CurrentHp,
                ArmorClass = participant.Character?.ArmorClass ?? 10
            };

            if (participant.Character != null && participant.Character.Attacks != null)
            {
                int profBonus = 2 + ((participant.Character.Level - 1) / 4);
                
                int GetStatMod(StatType? stat)
                {
                    if (!stat.HasValue) return 0;
                    int statVal = stat.Value switch {
                        StatType.Strength => participant.Character.Strength,
                        StatType.Dexterity => participant.Character.Dexterity,
                        StatType.Constitution => participant.Character.Constitution,
                        StatType.Intelligence => participant.Character.Intelligence,
                        StatType.Wisdom => participant.Character.Wisdom,
                        StatType.Charisma => participant.Character.Charisma,
                        _ => 10
                    };
                    return (statVal - 10) / 2;
                }

                int globalSpellSaveDC = 8 + profBonus + GetStatMod(participant.Character.SpellcastingAbility);

                foreach (var action in participant.Character.Attacks)
                {
                    bool isSpell = action.SpellId.HasValue && action.Spell != null;
                    double avgDamage = 0;

                    // Pre-calculate Average Damage Engine Input: (DiceCount * Average of DiceType) + Modifiers
                    foreach (var dmg in action.Damages)
                    {
                        double diceAvg = ((int)dmg.DiceType + 1) / 2.0; 
                        avgDamage += (dmg.DiceCount * diceAvg) + dmg.FlatDamageBonus;
                        
                        if (dmg.ModifierStat.HasValue)
                        {
                            avgDamage += GetStatMod(dmg.ModifierStat);
                        }
                    }

                    int atkBonus = action.FlatAttackBonus;
                    var effectiveAttackStat = action.AttackStat ?? (isSpell ? participant.Character.SpellcastingAbility : null);
                    atkBonus += GetStatMod(effectiveAttackStat);
                    if (action.IsProficient) atkBonus += profBonus;

                    participantDto.AvailableActions.Add(new CombatActionAnalyticsDto
                    {
                        ActionId = action.Id,
                        Name = action.Name,
                        ActionCost = isSpell ? action.Spell!.CastingTime : action.ActionCost,
                        IsSpell = isSpell,
                        IsSave = isSpell && action.Spell!.RequiresSave,
                        SaveDC = (isSpell && action.Spell!.RequiresSave) ? globalSpellSaveDC : 0,
                        AttackBonus = (isSpell && action.Spell!.RequiresSave) ? 0 : atkBonus,
                        AverageDamage = avgDamage,
                        IsAoE = isSpell && action.Spell!.IsAoE,
                        Shape = isSpell ? action.Spell!.Shape : AoEShape.None,
                        AoESizeFeet = isSpell ? action.Spell!.AoESizeFeet : 0,
                        HalfOnSuccess = isSpell && action.Spell!.HalfOnSuccess,
                        School = isSpell ? action.Spell!.School : null
                    });
                }
            }

            if (participant.Faction == Faction.Player) payload.Allies.Add(participantDto);
            else if (participant.Faction == Faction.Enemy) payload.Enemies.Add(participantDto);
        }

        return payload;
    }
}