using DnD.Application.DTOs;

namespace DnD.Application.Interfaces;

public interface ICharacterService
{
    Task<IEnumerable<CharacterBriefDto>> GetMyCharactersAsync(Guid userId, bool isAdmin);
    Task<CharacterFullDto?> GetByIdAsync(Guid id, Guid userId, bool isAdmin);
    Task<CharacterFullDto> CreateAsync(CharacterCreateDto dto, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid userId, bool isAdmin);

    // Roll method with Fog of War security

    // Універсальний кидок (використовує оновлений RollResponseDto з деталями)
    Task<RollResponseDto> PerformUniversalRollAsync(
        Guid characterId,
        Guid userId,
        bool isAdmin,
        UniversalRollRequestDto dto
    );

    // Update progression with Admin override
    Task<CharacterFullDto> UpdateProgressionAsync(
        Guid id,
        Guid userId,
        bool isAdmin,
        UpdateProgressionDto dto
    );

    // PATCH methods for combat and experience
    Task<CharacterFullDto> UpdateHealthAsync(
        Guid id,
        Guid userId,
        bool isAdmin,
        UpdateValueDto dto
    );

    // COMBINED: Limits and XP (Replaces UpdateXpAsync)
    Task<CharacterFullDto> UpdateVitalsAsync(
        Guid id,
        Guid userId,
        bool isAdmin,
        UpdateVitalsDto dto
    );
    Task<CharacterFullDto> UpdateWalletAsync(
        Guid id,
        Guid userId,
        bool isAdmin,
        UpdateWalletDto dto
    );

    // COMBINED: Limits and XP (Replaces UpdateXpAsync)
    Task<CharacterFullDto> PerformRestAsync(Guid id, Guid userId, bool isAdmin, PerformRestDto dto);
    Task<CharacterFullDto> AddTrackerAsync(
        Guid id,
        Guid userId,
        bool isAdmin,
        CreateTrackerDto dto
    );
    Task<HitDiceResultDto> SpendHitDiceAsync(
        Guid id,
        Guid userId,
        bool isAdmin,
        SpendHitDiceDto dto
    );

    Task<CharacterFullDto> AddAttackAsync(
        Guid characterId,
        Guid userId,
        bool isAdmin,
        CreateAttackDto dto
    );

    Task<CharacterFullDto> UpdateAttackAsync(
        Guid characterId,
        Guid attackId,
        Guid userId,
        bool isAdmin,
        CreateAttackDto dto
    );
    Task<RollResponseDto> RollAttackHitAsync(
        Guid characterId,
        Guid attackId,
        Guid userId,
        bool isAdmin
    );
    Task<List<DamageRollResultDto>> RollAttackDamageAsync(
        Guid characterId,
        Guid attackId,
        Guid userId,
        bool isAdmin,
        bool isCritical
    );
    Task<CharacterFullDto> DeleteTrackerAsync(
        Guid characterId,
        Guid trackerId,
        Guid userId,
        bool isAdmin
    );
    Task<CharacterFullDto> DeleteAttackAsync(
        Guid characterId,
        Guid attackId,
        Guid userId,
        bool isAdmin
    );

    // ✅ МАЄ БУТИ ТАК:
    Task<CharacterFullDto> UpdateTrackerAsync(
        Guid characterId,
        Guid trackerId,
        Guid userId,
        bool isAdmin,
        UpdateTrackerDto dto
    );
    Task<CharacterFullDto> UpdateIntegrationsAsync(
        Guid characterId,
        Guid userId,
        bool isAdmin,
        UpdateIntegrationsDto dto
    );
    Task<bool> UpdateImageUrlAsync(Guid characterId, Guid userId, string imageUrl);

    // Spell Slot Methods
    Task<CharacterFullDto> AddSpellSlotAsync(
        Guid characterId,
        Guid userId,
        bool isAdmin,
        CreateSpellSlotDto dto
    );
    Task<CharacterFullDto> UpdateSpellSlotAsync(
        Guid characterId,
        Guid slotId,
        Guid userId,
        bool isAdmin,
        UpdateSpellSlotDto dto
    );
    Task<CharacterFullDto> DeleteSpellSlotAsync(
        Guid characterId,
        Guid slotId,
        Guid userId,
        bool isAdmin
    );
}
