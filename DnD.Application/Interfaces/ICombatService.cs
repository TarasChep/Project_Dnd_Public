using DnD.Application.DTOs;

namespace DnD.Application.Interfaces;

public interface ICombatService
{
    Task<List<CombatActionDto>> GetCharacterActions(Guid characterId);
    Task CastSpellAction(Guid characterId, Guid actionId);
}