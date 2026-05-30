using DnD.Application.DTOs;
using DnD.Application.Interfaces;
using DnD.Domain.Entities;
using DnD.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DnD.Application.Services;

public class CombatService : ICombatService
{
    private readonly ApplicationDbContext _context;

    public CombatService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CombatActionDto>> GetCharacterActions(Guid characterId)
    {
        var character = await _context.Characters
            .Include(c => c.Attacks)
                .ThenInclude(a => a.Spell)
            .Include(c => c.Attacks)
                .ThenInclude(a => a.Damages)
            .FirstOrDefaultAsync(c => c.Id == characterId);

        if (character == null) throw new Exception("Character not found");

        // Підрахунок універсального Spell Save DC (8 + Prof + Highest Mental Mod)
        int profBonus = 2 + (character.Level - 1) / 4;
        
        int spellcastingMod = 0;
        if (character.SpellcastingAbility.HasValue)
        {
            int statVal = character.SpellcastingAbility.Value switch {
                DnD.Domain.Enums.StatType.Strength => character.Strength,
                DnD.Domain.Enums.StatType.Dexterity => character.Dexterity,
                DnD.Domain.Enums.StatType.Constitution => character.Constitution,
                DnD.Domain.Enums.StatType.Intelligence => character.Intelligence,
                DnD.Domain.Enums.StatType.Wisdom => character.Wisdom,
                DnD.Domain.Enums.StatType.Charisma => character.Charisma,
                _ => 10
            };
            spellcastingMod = (statVal - 10) / 2;
        }
        
        int spellSaveDC = 8 + profBonus + spellcastingMod;

        var dtos = new List<CombatActionDto>();

        foreach (var action in character.Attacks)
        {
            var isSpell = action.SpellId.HasValue && action.Spell != null;
            
            int atkBonus = action.FlatAttackBonus;
            var effectiveAttackStat = action.AttackStat ?? (isSpell ? character.SpellcastingAbility : null);
            if (effectiveAttackStat.HasValue)
            {
                int statVal = effectiveAttackStat.Value switch {
                    DnD.Domain.Enums.StatType.Strength => character.Strength,
                    DnD.Domain.Enums.StatType.Dexterity => character.Dexterity,
                    DnD.Domain.Enums.StatType.Constitution => character.Constitution,
                    DnD.Domain.Enums.StatType.Intelligence => character.Intelligence,
                    DnD.Domain.Enums.StatType.Wisdom => character.Wisdom,
                    DnD.Domain.Enums.StatType.Charisma => character.Charisma,
                    _ => 10
                };
                atkBonus += (statVal - 10) / 2;
            }
            if (action.IsProficient) atkBonus += profBonus;

            dtos.Add(new CombatActionDto
            {
                ActionId = action.Id,
                DisplayName = isSpell ? action.Spell!.Name : action.Name,
                IsSpell = isSpell,
                IsSave = isSpell && action.Spell!.RequiresSave,
                SaveDC = spellSaveDC,
                SaveStat = isSpell && action.Spell!.SaveStat.HasValue ? action.Spell.SaveStat.ToString()! : string.Empty,
                AttackBonus = isSpell && action.Spell!.RequiresSave ? 0 : atkBonus,
                DamageDice = isSpell ? action.Spell!.DamageDice : (action.Damages.FirstOrDefault()?.DiceCount + "d" + (int)(action.Damages.FirstOrDefault()?.DiceType ?? DnD.Domain.Enums.DiceType.D8)),
                SpellLevel = isSpell ? action.Spell!.Level : 0,
                ActionCost = isSpell ? action.Spell!.CastingTime : action.ActionCost
            });
        }
        return dtos;
    }

    public async Task CastSpellAction(Guid characterId, Guid actionId)
    {
        // Цю логіку можна використовувати перед кидком кубиків, щоб витратити слот
        var action = await _context.AttackActions.Include(a => a.Spell).FirstOrDefaultAsync(a => a.Id == actionId && a.CharacterId == characterId);
        if (action == null || action.Spell == null) return; // Це не закляття
        if (action.Spell.Level == 0) return; // Cantrip (фокуси) не витрачають слоти

        var slot = await _context.SpellSlots.FirstOrDefaultAsync(s => s.CharacterId == characterId && s.Level == action.Spell.Level);
        if (slot == null || slot.CurrentValue <= 0)
        {
            throw new Exception($"No spell slots remaining for level {action.Spell.Level}!");
        }

        slot.CurrentValue--;
        await _context.SaveChangesAsync();
    }
}