using DnD.Application.DTOs;
using DnD.Application.Interfaces;
using DnD.Domain.Entities;
using DnD.Domain.Enums;
using DnD.Domain.Interfaces;
using DnD.Domain.Services;
using DnD.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DnD.Application.Services;

public class CharacterService : ICharacterService
{
    private readonly ICharacterRepository _repository;
    private readonly IDiscordNotificationService _discordNotificationService;
    private readonly ApplicationDbContext _context;

    public CharacterService(
        ICharacterRepository repository,
        IDiscordNotificationService discordNotificationService,
        ApplicationDbContext context
    )
    {
        _repository = repository;
        _discordNotificationService = discordNotificationService;
        _context = context;
    }

    public async Task<IEnumerable<CharacterBriefDto>> GetMyCharactersAsync(Guid userId, bool isAdmin)
    {
        // Optimized query: Select directly from DbContext filtering by Role or Ownership
        return await _context.Characters
            .Where(c => c.UserId == userId || isAdmin)
            .Select(c => new CharacterBriefDto
            {
                Id = c.Id,
                Name = c.Name,
                Class = c.Class,
                Race = c.Race,
                Level = c.Level,
                ImageUrl = c.ImageUrl,
                CurrentHp = c.CurrentHp,
                MaxHp = c.MaxHp,
                TemporaryHp = c.TemporaryHp,
                LastModifiedAt = c.LastModifiedAt,
            })
            .ToListAsync();
    }

    public async Task<CharacterFullDto?> GetByIdAsync(Guid id, Guid userId, bool isAdmin)
    {
        var character = await _repository.GetByIdWithDetailsAsync(id);

        if (character == null)
            return null;

        // Access check (Business logic)
        bool isAuthorized = await IsUserAuthorizedForCharacterAsync(character.Id, userId);
        
        if (!isAdmin && !isAuthorized)
        {
            throw new UnauthorizedAccessException("You do not have permission to access this character.");
        }

        var dto = MapToFullDto(character);
        dto.CanEdit = true;
        return dto;
    }

    public async Task<CharacterFullDto> CreateAsync(CharacterCreateDto dto, Guid userId)
    {
        var character = new Character
        {
            Id = Guid.NewGuid(), // Server generates ID
            UserId = userId, // Server assigns owner
            CreatedAt = DateTime.UtcNow,
            LastModifiedAt = DateTime.UtcNow,

            // Player data mapping
            Name = dto.Name,
            Race = dto.Race,
            Class = dto.Class,
            Background = dto.Background,
            Alignment = dto.Alignment,
            ImageUrl = dto.ImageUrl,

            CurrentXp = dto.CurrentXp,
            MaxHp = dto.MaxHp,
            CurrentHp = dto.CurrentHp,

            HitDiceType = dto.HitDiceType,

            // Base Stats
            Strength = dto.Strength,
            Dexterity = dto.Dexterity,
            Constitution = dto.Constitution,
            Intelligence = dto.Intelligence,
            Wisdom = dto.Wisdom,
            Charisma = dto.Charisma,

            // Arrays / Lists
            Inventory = dto.Inventory,
            Spells = dto.Spells,
            Feats = dto.Feats,
            ClassFeatures = dto.ClassFeatures,
            RacialTraits = dto.RacialTraits,

            AppearanceDescription = dto.AppearanceDescription,
            Notes = dto.Notes,

            // Save Roll Proficiency
            IsStrengthSaveProficient = dto.IsStrengthSaveProficient,
            IsDexteritySaveProficient = dto.IsDexteritySaveProficient,
            IsConstitutionSaveProficient = dto.IsConstitutionSaveProficient,
            IsIntelligenceSaveProficient = dto.IsIntelligenceSaveProficient,
            IsWisdomSaveProficient = dto.IsWisdomSaveProficient,
            IsCharismaSaveProficient = dto.IsCharismaSaveProficient,

            // Skills
            Athletics = dto.Athletics,
            Acrobatics = dto.Acrobatics,
            SleightOfHand = dto.SleightOfHand,
            Stealth = dto.Stealth,
            Arcana = dto.Arcana,
            History = dto.History,
            Investigation = dto.Investigation,
            Nature = dto.Nature,
            Religion = dto.Religion,
            AnimalHandling = dto.AnimalHandling,
            Insight = dto.Insight,
            Medicine = dto.Medicine,
            Perception = dto.Perception,
            Survival = dto.Survival,
            Deception = dto.Deception,
            Intimidation = dto.Intimidation,
            Performance = dto.Performance,
            Persuasion = dto.Persuasion,

            // Initiative
            AdditionalInitiativeBonus = dto.AdditionalInitiativeBonus,
        };

        await _repository.AddAsync(character);
        await _repository.SaveChangesAsync();

        return MapToFullDto(character);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId, bool isAdmin)
    {
        var character = await _repository.GetByIdAsync(id);

        if (character == null)
            return false;

        bool isAuthorized = await IsUserAuthorizedForCharacterAsync(character.Id, userId);
        if (!isAdmin && !isAuthorized)
        {
            throw new UnauthorizedAccessException("You do not have permission to delete this character.");
        }

        _repository.Delete(character);
        await _repository.SaveChangesAsync();

        return true;
    }

    // --- PATCH METHODS FOR HEALTH AND XP ---

    public async Task<CharacterFullDto> UpdateHealthAsync(
        Guid id,
        Guid userId,
        bool isAdmin,
        UpdateValueDto dto
    )
    {
        var character = await _repository.GetByIdWithDetailsAsync(id);

        if (character == null)
            throw new Exception("Not Found");

        if (!(await IsUserAuthorizedForCharacterAsync(character.Id, userId)) && !isAdmin)
            throw new UnauthorizedAccessException("You do not have permission to edit this character.");

        // Logic: Handle Damage (Negative) and Healing (Positive)
        if (dto.Amount < 0)
        {
            // DAMAGE LOGIC
            int damage = Math.Abs(dto.Amount);

            if (character.TemporaryHp > 0)
            {
                if (character.TemporaryHp >= damage)
                {
                    character.TemporaryHp -= damage;
                    damage = 0;
                }
                else
                {
                    damage -= character.TemporaryHp;
                    character.TemporaryHp = 0;
                }
            }

            character.CurrentHp = Math.Max(0, character.CurrentHp - damage);
        }
        else if (dto.Amount > 0)
        {
            // HEALING LOGIC
            // Healing only affects CurrentHp and is capped by MaxHp
            int newHp = character.CurrentHp + dto.Amount;
            character.CurrentHp = Math.Clamp(newHp, 0, character.MaxHp);
        }

        character.LastModifiedAt = DateTime.UtcNow;
        _repository.Update(character);
        await _repository.SaveChangesAsync();

        return MapToFullDto(character);
    }

    public async Task<CharacterFullDto> UpdateVitalsAsync(
        Guid id,
        Guid userId,
        bool isAdmin,
        UpdateVitalsDto dto
    )
    {
        var character = await _repository.GetByIdWithDetailsAsync(id);

        if (character == null)
            throw new Exception("Not Found");

        if (!(await IsUserAuthorizedForCharacterAsync(character.Id, userId)) && !isAdmin)
            throw new UnauthorizedAccessException("You do not have permission to edit this character.");

        // Set new Max HP limit
        if (dto.MaxHp.HasValue)
            character.MaxHp = Math.Max(1, dto.MaxHp.Value);

        // Set Temporary HP (replaces existing, doesn't stack per 5e rules)
        if (dto.TemporaryHp.HasValue)
            character.TemporaryHp = Math.Max(0, dto.TemporaryHp.Value);

        // Add Experience and check for Level Up
        if (dto.AddXp.HasValue)
        {
            character.CurrentXp = Math.Max(0, character.CurrentXp + dto.AddXp.Value);
        }

        character.LastModifiedAt = DateTime.UtcNow;
        _repository.Update(character);
        await _repository.SaveChangesAsync();

        return MapToFullDto(character);
    }

    // --- UPDATE PROGRESSION METHOD ---
    public async Task<CharacterFullDto> UpdateProgressionAsync(
        Guid characterId,
        Guid userId,
        bool isAdmin,
        UpdateProgressionDto dto
    )
    {
        var character = await _repository.GetByIdWithDetailsAsync(characterId);

        // Fog of War security check
        if (character == null)
            throw new Exception("Not Found");

        if (!(await IsUserAuthorizedForCharacterAsync(character.Id, userId)) && !isAdmin)
            throw new UnauthorizedAccessException("You do not have permission to edit this character.");

        // --- 1. ДОСВІД ТА РІВЕНЬ (Core Progression) ---
        if (dto.CurrentXp.HasValue)
        {
            // Встановлюємо новий XP
            character.CurrentXp = dto.CurrentXp.Value;
        }

        // --- 2. КЛАС ТА КУБИКИ ЗДОРОВ'Я ---
        if (!string.IsNullOrWhiteSpace(dto.Class))
        {
            character.Class = dto.Class;
        }

        if (!string.IsNullOrWhiteSpace(dto.Name)) character.Name = dto.Name.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Race)) character.Race = dto.Race.Trim();
        if (dto.Passives != null) character.Passives = dto.Passives;
        if (dto.TrackersText != null) character.TrackersText = dto.TrackersText;

        if (dto.HitDiceType.HasValue)
        {
            // Тут ми очікуємо Enum (DiceType), про який домовлялися раніше
            character.HitDiceType = dto.HitDiceType.Value;
        }

        // --- 3. CORE STATS ---
        character.Strength = dto.Strength ?? character.Strength;
        character.Dexterity = dto.Dexterity ?? character.Dexterity;
        character.Constitution = dto.Constitution ?? character.Constitution;
        character.Intelligence = dto.Intelligence ?? character.Intelligence;
        character.Wisdom = dto.Wisdom ?? character.Wisdom;
        character.Charisma = dto.Charisma ?? character.Charisma;

        if (dto.SpellcastingAbility.HasValue)
        {
            character.SpellcastingAbility = (int)dto.SpellcastingAbility.Value == 0 ? null : dto.SpellcastingAbility;
        }

        // --- 4. SAVING THROWS (Рятівні кидки) ---
        character.IsStrengthSaveProficient =
            dto.IsStrengthSaveProficient ?? character.IsStrengthSaveProficient;
        character.IsDexteritySaveProficient =
            dto.IsDexteritySaveProficient ?? character.IsDexteritySaveProficient;
        character.IsConstitutionSaveProficient =
            dto.IsConstitutionSaveProficient ?? character.IsConstitutionSaveProficient;
        character.IsIntelligenceSaveProficient =
            dto.IsIntelligenceSaveProficient ?? character.IsIntelligenceSaveProficient;
        character.IsWisdomSaveProficient =
            dto.IsWisdomSaveProficient ?? character.IsWisdomSaveProficient;
        character.IsCharismaSaveProficient =
            dto.IsCharismaSaveProficient ?? character.IsCharismaSaveProficient;

        // --- 5. SKILLS (Навички) ---
        character.Athletics = dto.Athletics ?? character.Athletics;
        character.Acrobatics = dto.Acrobatics ?? character.Acrobatics;
        character.SleightOfHand = dto.SleightOfHand ?? character.SleightOfHand;
        character.Stealth = dto.Stealth ?? character.Stealth;
        character.Arcana = dto.Arcana ?? character.Arcana;
        character.History = dto.History ?? character.History;
        character.Investigation = dto.Investigation ?? character.Investigation;
        character.Nature = dto.Nature ?? character.Nature;
        character.Religion = dto.Religion ?? character.Religion;
        character.AnimalHandling = dto.AnimalHandling ?? character.AnimalHandling;
        character.Insight = dto.Insight ?? character.Insight;
        character.Medicine = dto.Medicine ?? character.Medicine;
        character.Perception = dto.Perception ?? character.Perception;
        character.Survival = dto.Survival ?? character.Survival;
        character.Deception = dto.Deception ?? character.Deception;
        character.Intimidation = dto.Intimidation ?? character.Intimidation;
        character.Performance = dto.Performance ?? character.Performance;
        character.Persuasion = dto.Persuasion ?? character.Persuasion;

        character.Speed = dto.Speed ?? character.Speed;
        character.ArmorClass = dto.ArmorClass ?? character.ArmorClass;
        // --- 6. INITIATIVE ---
        if (dto.AdditionalInitiativeBonus.HasValue)
        {
            character.AdditionalInitiativeBonus = dto.AdditionalInitiativeBonus.Value;
        }

        character.LastModifiedAt = DateTime.UtcNow;

        _repository.Update(character);
        await _repository.SaveChangesAsync();

        // Під час мапінгу в DTO твій proficiencyBonus потрапить у JSON відповідь
        return MapToFullDto(character);
    }

    public async Task<CharacterFullDto> UpdateWalletAsync(
        Guid id,
        Guid userId,
        bool isAdmin,
        UpdateWalletDto dto
    )
    {
        var character = await _repository.GetByIdWithDetailsAsync(id);

        if (character == null)
            throw new Exception("Not Found");

        if (!(await IsUserAuthorizedForCharacterAsync(character.Id, userId)) && !isAdmin)
            throw new UnauthorizedAccessException("You do not have permission to edit this character.");

        // 1. Конвертуємо поточний гаманець у мідь
        long currentTotalCp =
            (long)character.Platinum * 1000
            + (long)character.Gold * 100
            + (long)character.Silver * 10
            + character.Copper;

        // 2. Конвертуємо транзакцію у мідь (від'ємні значення залишаться від'ємними!)
        long transactionCp =
            (long)dto.Platinum * 1000 + (long)dto.Gold * 100 + (long)dto.Silver * 10 + dto.Copper;

        // 3. Обчислюємо новий баланс
        long newTotalCp = currentTotalCp + transactionCp;

        if (newTotalCp < 0)
        {
            throw new Exception("Insufficient funds"); // Не вистачає грошей
        }

        // 4. Перерозподіляємо мідь назад у найбільші монети
        character.Platinum = (int)(newTotalCp / 1000);
        newTotalCp %= 1000;

        character.Gold = (int)(newTotalCp / 100);
        newTotalCp %= 100;

        character.Silver = (int)(newTotalCp / 10);
        newTotalCp %= 10;

        character.Copper = (int)newTotalCp;

        character.LastModifiedAt = DateTime.UtcNow;
        _repository.Update(character);
        await _repository.SaveChangesAsync();

        return MapToFullDto(character);
    }

    public async Task<bool> UpdateImageUrlAsync(Guid characterId, Guid userId, string imageUrl)
    {
        var character = await _repository.GetByIdAsync(characterId);

        if (character == null || (character.UserId != userId))
        {
            return false;
        }

        character.ImageUrl = imageUrl;
        character.LastModifiedAt = DateTime.UtcNow;
        _repository.Update(character);
        await _repository.SaveChangesAsync();

        return true;
    }

    // --- AUTHORIZATION HELPER ---
    private async Task<bool> IsUserAuthorizedForCharacterAsync(Guid characterId, Guid userId)
    {
        var character = await _repository.GetByIdAsync(characterId);

        if (character == null)
            return false;

        // Owner can always edit their own character
        if (character.UserId == userId)
            return true;

        // Check if user is GM of ANY campaign containing this character
        var isGmOfCampaign = await _context.Campaigns
            .Where(c => c.GmUserId == userId)
            .SelectMany(c => c.Characters)
            .AnyAsync(cc => cc.CharacterId == characterId);

        return isGmOfCampaign;
    }

    // --- PRIVATE MAPPER ---
    private static CharacterFullDto MapToFullDto(Character c)
    {
        // 1. Виправляємо твою синтаксичну помилку. Передаємо поточний XP.
        var (_, nextXp) = ExperienceTable.CalculateLevelInfo(c.CurrentXp);

        int GetStatMod(StatType? stat)
        {
            if (!stat.HasValue) return 0;
            int statVal = stat.Value switch {
                StatType.Strength => c.Strength,
                StatType.Dexterity => c.Dexterity,
                StatType.Constitution => c.Constitution,
                StatType.Intelligence => c.Intelligence,
                StatType.Wisdom => c.Wisdom,
                StatType.Charisma => c.Charisma,
                _ => 10
            };
            return (statVal - 10) / 2;
        }

        int globalSpellSaveDC = 8 + c.proficiencyBonus + GetStatMod(c.SpellcastingAbility);

        return new CharacterFullDto
        {
            Id = c.Id,
            UserId = c.UserId,
            Name = c.Name,
            Race = c.Race,
            Class = c.Class,
            Level = c.Level,

            // Досвід
            CurrentXp = c.CurrentXp,
            NextLevelXp = nextXp, // Динамічно розрахований поріг наступного рівня

            ImageUrl = c.ImageUrl,
            Alignment = c.Alignment,
            Background = c.Background,

            // Stats & Modifiers
            Strength = c.Strength,
            Dexterity = c.Dexterity,
            Constitution = c.Constitution,
            Intelligence = c.Intelligence,
            Wisdom = c.Wisdom,
            Charisma = c.Charisma,

            StrengthModifier = DnDCalculator.CalculateModifier(c.Strength),
            DexterityModifier = DnDCalculator.CalculateModifier(c.Dexterity),
            ConstitutionModifier = DnDCalculator.CalculateModifier(c.Constitution),
            IntelligenceModifier = DnDCalculator.CalculateModifier(c.Intelligence),
            WisdomModifier = DnDCalculator.CalculateModifier(c.Wisdom),
            CharismaModifier = DnDCalculator.CalculateModifier(c.Charisma),

            SpellcastingAbility = c.SpellcastingAbility,

            // Wallet
            Platinum = c.Platinum,
            Gold = c.Gold,
            Silver = c.Silver,
            Copper = c.Copper,

            // Combat
            CurrentHp = c.CurrentHp,
            MaxHp = c.MaxHp,
            TemporaryHp = c.TemporaryHp,
            ArmorClass = c.ArmorClass,
            Speed = c.Speed,
            ProficiencyBonus = c.proficiencyBonus,

            // INITIATIVE - рассчитывается на основе DEX модификатора
            Initiative = DnDCalculator.CalculateInitiative(
                c.Dexterity,
                c.AdditionalInitiativeBonus
            ),
            AdditionalInitiativeBonus = c.AdditionalInitiativeBonus,

            // SAVING THROWS (Рятівні кидки) - з бонусами і proficiency інформацією
            StrengthSaveBonus = DnDCalculator.CalculateSavingThrowBonus(c, StatType.Strength),
            IsStrengthSaveProficient = c.IsStrengthSaveProficient,

            DexteritySaveBonus = DnDCalculator.CalculateSavingThrowBonus(c, StatType.Dexterity),
            IsDexteritySaveProficient = c.IsDexteritySaveProficient,

            ConstitutionSaveBonus = DnDCalculator.CalculateSavingThrowBonus(
                c,
                StatType.Constitution
            ),
            IsConstitutionSaveProficient = c.IsConstitutionSaveProficient,

            IntelligenceSaveBonus = DnDCalculator.CalculateSavingThrowBonus(
                c,
                StatType.Intelligence
            ),
            IsIntelligenceSaveProficient = c.IsIntelligenceSaveProficient,

            WisdomSaveBonus = DnDCalculator.CalculateSavingThrowBonus(c, StatType.Wisdom),
            IsWisdomSaveProficient = c.IsWisdomSaveProficient,

            CharismaSaveBonus = DnDCalculator.CalculateSavingThrowBonus(c, StatType.Charisma),
            IsCharismaSaveProficient = c.IsCharismaSaveProficient,

            // SKILLS (Calculated via Domain Entity Methods) - з бонусами і proficiency інформацією
            // Strength
            Athletics = c.GetSkillBonus(c.Strength, c.Athletics),
            AthleticsProficiency = c.Athletics,

            // Dexterity
            Acrobatics = c.GetSkillBonus(c.Dexterity, c.Acrobatics),
            AcrobaticsProficiency = c.Acrobatics,

            SleightOfHand = c.GetSkillBonus(c.Dexterity, c.SleightOfHand),
            SleightOfHandProficiency = c.SleightOfHand,

            Stealth = c.GetSkillBonus(c.Dexterity, c.Stealth),
            StealthProficiency = c.Stealth,

            // Intelligence
            Arcana = c.GetSkillBonus(c.Intelligence, c.Arcana),
            ArcanaProficiency = c.Arcana,

            History = c.GetSkillBonus(c.Intelligence, c.History),
            HistoryProficiency = c.History,

            Investigation = c.GetSkillBonus(c.Intelligence, c.Investigation),
            InvestigationProficiency = c.Investigation,

            Nature = c.GetSkillBonus(c.Intelligence, c.Nature),
            NatureProficiency = c.Nature,

            Religion = c.GetSkillBonus(c.Intelligence, c.Religion),
            ReligionProficiency = c.Religion,

            // Wisdom
            AnimalHandling = c.GetSkillBonus(c.Wisdom, c.AnimalHandling),
            AnimalHandlingProficiency = c.AnimalHandling,

            Insight = c.GetSkillBonus(c.Wisdom, c.Insight),
            InsightProficiency = c.Insight,

            Medicine = c.GetSkillBonus(c.Wisdom, c.Medicine),
            MedicineProficiency = c.Medicine,

            Perception = c.GetSkillBonus(c.Wisdom, c.Perception),
            PerceptionProficiency = c.Perception,

            Survival = c.GetSkillBonus(c.Wisdom, c.Survival),
            SurvivalProficiency = c.Survival,

            // Charisma
            Deception = c.GetSkillBonus(c.Charisma, c.Deception),
            DeceptionProficiency = c.Deception,

            Intimidation = c.GetSkillBonus(c.Charisma, c.Intimidation),
            IntimidationProficiency = c.Intimidation,

            Performance = c.GetSkillBonus(c.Charisma, c.Performance),
            PerformanceProficiency = c.Performance,

            Persuasion = c.GetSkillBonus(c.Charisma, c.Persuasion),
            PersuasionProficiency = c.Persuasion,

            // Ресурси відпочинку
            HitDiceMax = c.HitDiceMax,
            HitDiceCurrent = c.HitDiceCurrent,
            HitDiceType = c.HitDiceType.ToString(), // Перетворюємо Enum "D8" у рядок для JSON

            // Lists
            Inventory = c.Inventory,
            Spells = c.Spells,
            Feats = c.Feats,
            ClassFeatures = c.ClassFeatures,
            RacialTraits = c.RacialTraits,
            AppearanceDescription = c.AppearanceDescription,
            Passives = c.Passives,
            TrackersText = c.TrackersText,
            Notes = c.Notes,

            // Зв'язані сутності
            // У твоєму мапері має бути щось таке:
            Trackers = c
                .ResourceTrackers.Select(t => new ResourceTrackerDto
                {
                    Id = t.Id,
                    Name = t.Name, // Було Title
                    Description = t.Description,
                    CurrentValue = t.CurrentValue,
                    MaxValue = t.MaxValue,
                    ResetCondition = t.ResetCondition.ToString(), // Перетворює Enum в текст ("ShortRest")
                })
                .ToList(),
            SpellSlots = c.SpellSlots?.Select(s => new SpellSlotDto
            {
                Id = s.Id,
                Level = s.Level,
                MaxValue = s.MaxValue,
                CurrentValue = s.CurrentValue
            }).ToList() ?? new List<SpellSlotDto>(),
            Attacks =
                c.Attacks?.Select(a => new AttackActionDto
                    {
                        Id = a.Id,
                        Name = a.Name,
                        IsAttackRoll = a.IsAttackRoll,
                        IsProficient = a.IsProficient,
                        AttackStat = a.AttackStat,
                        FlatAttackBonus = a.FlatAttackBonus,
                        ActionCost = a.ActionCost,
                        SpellId = a.SpellId,
                        SaveDC = a.SpellId.HasValue 
                            ? globalSpellSaveDC 
                            : (8 + (a.IsProficient ? c.proficiencyBonus : 0) + GetStatMod(a.AttackStat) + a.FlatAttackBonus),
                        Spell = a.Spell != null ? new SpellDto
                        {
                            Id = a.Spell.Id,
                            Name = a.Spell.Name,
                            Level = a.Spell.Level,
                            RequiresSave = a.Spell.RequiresSave,
                            SaveStat = a.Spell.SaveStat?.ToString(),
                            CastingTime = a.Spell.CastingTime.ToString(),
                            DamageDice = a.Spell.DamageDice,
                            DamageType = a.Spell.DamageType,
                            Description = a.Spell.Description,
                            BuffDebuffNotes = a.Spell.BuffDebuffNotes,
                            School = a.Spell.School
                        } : null,
                        Damages = a
                            .Damages.Select(d => new AttackDamageDto
                            {
                                DiceType = d.DiceType,
                                DiceCount = d.DiceCount,
                                ModifierStat = d.ModifierStat,
                                FlatDamageBonus = d.FlatDamageBonus,
                                DamageType = d.DamageType,
                            })
                            .ToList(),
                    })
                    .ToList()
                ?? new List<AttackActionDto>(),
            DiscordWebhookUrl = c.DiscordWebhookUrl,
            ThemeColor = c.ThemeColor,
            CreatedAt = c.CreatedAt,
            LastModifiedAt = c.LastModifiedAt,
        };
    }

    public async Task<RollResponseDto> PerformUniversalRollAsync(
        Guid characterId,
        Guid userId,
        bool isAdmin,
        UniversalRollRequestDto dto
    )
    {
        var character = await _repository.GetByIdAsync(characterId);

        // 1. Валідація і доступи
        if (character == null)
            throw new Exception("Character Not Found or Access Denied");

        if (!(await IsUserAuthorizedForCharacterAsync(character.Id, userId)) && !isAdmin)
            throw new UnauthorizedAccessException("You do not have permission to edit this character.");

        if (dto.DiceCount > 100)
        {
            throw new Exception("Maximum 100 dice allowed per roll");
        }

        var random = new Random();
        var rolls = new List<RollDetailDto>();

        int diceCount = dto.DiceCount ?? 1;
        int sides = (int)(dto.DiceSides ?? DiceType.D20);

        int modifier = 0;
        var modParts = new List<string>();

        // 2. Плоский модифікатор
        if (dto.FlatModifier.HasValue && dto.FlatModifier.Value != 0)
        {
            modifier += dto.FlatModifier.Value;
            modParts.Add($"{dto.FlatModifier.Value} (Flat)");
        }

        // 3. Магія Характеристик та Рятівних кидків
        if (dto.Type == RollType.Initiative)
        {
            int initMod = DnDCalculator.CalculateInitiative(character.Dexterity, character.AdditionalInitiativeBonus);
            modifier += initMod;
            modParts.Add($"{initMod} (Init)");
        }
        else if (
            dto.Stat.HasValue
            && (dto.Type == RollType.StatCheck || dto.Type == RollType.SavingThrow)
        )
        {
            int statValue = dto.Stat.Value switch
            {
                StatType.Strength => character.Strength,
                StatType.Dexterity => character.Dexterity,
                StatType.Constitution => character.Constitution,
                StatType.Intelligence => character.Intelligence,
                StatType.Wisdom => character.Wisdom,
                StatType.Charisma => character.Charisma,
                _ => 10,
            };

            int statMod = (statValue - 10) / 2;
            modifier += statMod;
            modParts.Add($"{statMod} ({dto.Stat.Value})");

            if (dto.Type == RollType.SavingThrow)
            {
                bool isProficient = dto.Stat.Value switch
                {
                    StatType.Strength => character.IsStrengthSaveProficient,
                    StatType.Dexterity => character.IsDexteritySaveProficient,
                    StatType.Constitution => character.IsConstitutionSaveProficient,
                    StatType.Intelligence => character.IsIntelligenceSaveProficient,
                    StatType.Wisdom => character.IsWisdomSaveProficient,
                    StatType.Charisma => character.IsCharismaSaveProficient,
                    _ => false,
                };

                if (isProficient)
                {
                    int profBonus = 2 + (character.Level - 1) / 4;
                    modifier += profBonus;
                    modParts.Add($"{profBonus} (Prof)");
                }
            }
        }

        // 4. Фізика кидка
        for (int i = 0; i < diceCount; i++)
        {
            int val = random.Next(1, sides + 1);
            rolls.Add(new RollDetailDto { Value = val, Sides = sides });
        }

        var responseDto = new RollResponseDto
        {
            RollName = dto.Type.ToString(),
            Rolls = rolls,
            Modifier = modifier,
            ModifierBreakdown = string.Join(" + ", modParts),
        };

        // 5. Динамічний заголовок для Discord (ТІЛЬКИ АНГЛІЙСЬКОЮ)
        // Зверни увагу: якщо у твоєму DTO немає поля Skill, заміни dto.Skill на те, що ти використовуєш для навичок, або видали цей кейс.
        string discordTitle = dto.Type switch
        {
            RollType.StatCheck => $"Check: {dto.Stat}",
            RollType.SavingThrow => $"Saving Throw: {dto.Stat}",
            RollType.SkillCheck => $"Skill: {dto.Skill}",
            RollType.Initiative => "Initiative Roll",
            _ => dto.Type.ToString(),
        };
        
        if (sides == 20 && rolls.Count == 1)
        {
            if (rolls[0].Value == 20) discordTitle = "🌟 CRITICAL SUCCESS! " + discordTitle;
            else if (rolls[0].Value == 1) discordTitle = "💀 CRITICAL FAIL! " + discordTitle;
        }

        // 6. Відправка в Discord
        await _discordNotificationService.SendRollAsync(character, discordTitle, responseDto);

        // 7. Якщо це Ініціатива - записуємо результат в останнє/активне зіткнення
        if (dto.Type == RollType.Initiative)
        {
            var participant = await _context.EncounterParticipants
                .Include(p => p.Encounter)
                .Where(p => p.CharacterId == characterId)
                .OrderByDescending(p => p.Encounter.CreatedAt)
                .FirstOrDefaultAsync();

            if (participant != null)
            {
                participant.InitiativeRoll = responseDto.Total;
                await _context.SaveChangesAsync();
            }
        }

        return responseDto;
    }

    public async Task<CharacterFullDto> PerformRestAsync(
        Guid id,
        Guid userId,
        bool isAdmin,
        PerformRestDto dto
    )
    {
        // Використовуємо наш новий метод репозиторію!
        var character = await _repository.GetByIdWithDetailsAsync(id);

        if (character == null)
            throw new Exception("Not Found");

        if (!(await IsUserAuthorizedForCharacterAsync(character.Id, userId)) && !isAdmin)
            throw new UnauthorizedAccessException("You do not have permission to edit this character.");

        if (dto.RestType == RestType.Long)
        {
            // --- ЛОГІКА LONG REST ---
            character.CurrentHp = character.MaxHp;
            character.TemporaryHp = 0;

            // Відновлюємо половину Hit Dice (мінімум 1)
            int hitDiceToRecover = Math.Max(1, character.HitDiceMax / 2);
            character.HitDiceCurrent = Math.Min(
                character.HitDiceMax,
                character.HitDiceCurrent + hitDiceToRecover
            );

            // Відновлюємо ресурси (Long Rest скидає і Short і Long трекери)
            foreach (var tracker in character.ResourceTrackers)
            {
                if (
                    tracker.ResetCondition == ResetCondition.LongRest
                    || tracker.ResetCondition == ResetCondition.ShortRest
                )
                {
                    tracker.CurrentValue = tracker.MaxValue;
                }
            }

            foreach (var slot in character.SpellSlots)
            {
                slot.CurrentValue = slot.MaxValue;
            }
        }
        else if (dto.RestType == RestType.Short)
        {
            // --- ЛОГІКА SHORT REST ---
            // Зверни увагу: Short Rest автоматично не лікує HP! Гравець сам витрачає Hit Dice.

            // Відновлюємо тільки ресурси, які залежать від Short Rest (наприклад, Кістки Барда)
            foreach (var tracker in character.ResourceTrackers)
            {
                if (tracker.ResetCondition == ResetCondition.ShortRest)
                {
                    tracker.CurrentValue = tracker.MaxValue;
                }
            }
        }

        character.LastModifiedAt = DateTime.UtcNow;
        _repository.Update(character);
        await _repository.SaveChangesAsync();

        return MapToFullDto(character);
    }

    public async Task<CharacterFullDto> AddTrackerAsync(
        Guid id,
        Guid userId,
        bool isAdmin,
        CreateTrackerDto dto
    )
    {
        var character = await _repository.GetByIdWithDetailsAsync(id);

        if (character == null)
            throw new Exception("Not Found");

        if (!(await IsUserAuthorizedForCharacterAsync(character.Id, userId)) && !isAdmin)
            throw new UnauthorizedAccessException("You do not have permission to edit this character.");

        var tracker = new ResourceTracker
        {
            CharacterId = character.Id,
            Name = dto.Name,
            Description = dto.Description ?? string.Empty,
            MaxValue = dto.MaxValue,
            CurrentValue = dto.MaxValue, // Заповнюємо на максимум
            ResetCondition = dto.ResetCondition,
        };

        character.ResourceTrackers.Add(tracker);
        character.LastModifiedAt = DateTime.UtcNow;

        _repository.Update(character);
        await _repository.SaveChangesAsync();

        return MapToFullDto(character);
    }

    public async Task<CharacterFullDto> UpdateTrackerAsync(
        Guid characterId,
        Guid trackerId,
        Guid userId,
        bool isAdmin,
        UpdateTrackerDto dto
    )
    {
        var character = await _repository.GetByIdWithDetailsAsync(characterId);
        if (character == null || (character.UserId != userId && !isAdmin))
            throw new Exception("Not Found");

        var tracker = character.ResourceTrackers.FirstOrDefault(t => t.Id == trackerId);
        if (tracker == null)
            throw new Exception("Tracker Not Found");

        // 1. Оновлюємо метадані трекера (якщо вони передані у JSON)
        if (!string.IsNullOrWhiteSpace(dto.Name))
            tracker.Name = dto.Name;

        if (dto.Description != null)
            tracker.Description = dto.Description;

        if (dto.MaxValue.HasValue)
            tracker.MaxValue = dto.MaxValue.Value;

        if (dto.ResetCondition.HasValue)
            tracker.ResetCondition = dto.ResetCondition.Value;

        // 2. Логіка оновлення поточного значення (пріоритет: CurrentValue, потім AdjustValue)
        // Важливо: Clamp використовує вже ОНОВЛЕНИЙ tracker.MaxValue
        if (dto.CurrentValue.HasValue)
        {
            tracker.CurrentValue = Math.Clamp(dto.CurrentValue.Value, 0, tracker.MaxValue);
        }
        else if (dto.AdjustValue.HasValue)
        {
            tracker.CurrentValue = Math.Clamp(
                tracker.CurrentValue + dto.AdjustValue.Value,
                0,
                tracker.MaxValue
            );
        }

        // Оновлюємо час модифікації персонажа (гігієна БД)
        character.LastModifiedAt = DateTime.UtcNow;

        _repository.Update(character);
        await _repository.SaveChangesAsync();

        return MapToFullDto(character);
    }

    public async Task<HitDiceResultDto> SpendHitDiceAsync(
        Guid id,
        Guid userId,
        bool isAdmin,
        SpendHitDiceDto dto
    )
    {
        var character = await _repository.GetByIdAsync(id);

        if (character == null)
            throw new Exception("Not Found");

        if (!(await IsUserAuthorizedForCharacterAsync(character.Id, userId)) && !isAdmin)
            throw new UnauthorizedAccessException("You do not have permission to edit this character");

        if (dto.Count <= 0)
            throw new Exception("Count must be at least 1.");
        if (character.HitDiceCurrent < dto.Count)
            throw new Exception("Not enough Hit Dice remaining.");

        // Розрахунок модифікатора статури за правилами D&D 5e: (Статура - 10) / 2
        int conModifier = (character.Constitution - 10) / 2;

        // ЗМІНЕНО: Тепер це список об'єктів
        var rolls = new List<RollDetailDto>();
        int totalHealed = 0;
        var random = new Random();

        // Отримуємо кількість граней кубика (наприклад, D8 = 8)
        int sides = (int)character.HitDiceType;

        // Кидаємо кубики
        for (int i = 0; i < dto.Count; i++)
        {
            int rollValue = random.Next(1, sides + 1);

            // ЗМІНЕНО: Запаковуємо результат у детальний DTO
            rolls.Add(new RollDetailDto { Value = rollValue, Sides = sides });

            // В D&D ти додаєш ConModifier до КОЖНОГО кубика. Мінімальне лікування з кубика = 1.
            int healFromThisDice = Math.Max(1, rollValue + conModifier);
            totalHealed += healFromThisDice;
        }

        // Віднімаємо витрачені кубики
        character.HitDiceCurrent -= dto.Count;

        // Лікуємо, але не більше ніж MaxHp
        character.CurrentHp = Math.Min(character.MaxHp, character.CurrentHp + totalHealed);

        character.LastModifiedAt = DateTime.UtcNow;
        _repository.Update(character);
        await _repository.SaveChangesAsync();

        return new HitDiceResultDto
        {
            Rolls = rolls,
            ConstitutionModifier = conModifier,
            TotalHealed = totalHealed,
            NewCurrentHp = character.CurrentHp,
            HitDiceRemaining = character.HitDiceCurrent,
        };
    }

    // --- 1. ДОДАВАННЯ АТАКИ В АРСЕНАЛ ---
    public async Task<CharacterFullDto> AddAttackAsync(
        Guid characterId,
        Guid userId,
        bool isAdmin,
        CreateAttackDto dto
    )
    {
        var character = await _repository.GetByIdWithDetailsAsync(characterId);
        if (character == null)
            throw new Exception("Not Found");

        if (!(await IsUserAuthorizedForCharacterAsync(character.Id, userId)) && !isAdmin)
            throw new UnauthorizedAccessException("You do not have permission to edit this character.");

        var newAttack = new AttackAction
        {
            Name = dto.Name,
            IsAttackRoll = dto.IsAttackRoll,
            IsProficient = dto.IsProficient,
            AttackStat = dto.AttackStat,
            FlatAttackBonus = dto.FlatAttackBonus,
            ActionCost = dto.ActionCost,
            SpellId = dto.SpellId,
            Damages = dto
                .Damages.Select(d => new AttackDamage
                {
                    DiceType = d.DiceType,
                    DiceCount = d.DiceCount,
                    ModifierStat = d.ModifierStat,
                    FlatDamageBonus = d.FlatDamageBonus,
                    DamageType = d.DamageType,
                })
                .ToList(),
        };

        character.Attacks.Add(newAttack);
        character.LastModifiedAt = DateTime.UtcNow;

        _repository.Update(character);
        await _repository.SaveChangesAsync();

        if (newAttack.SpellId.HasValue)
        {
            await _context.Entry(newAttack).Reference(a => a.Spell).LoadAsync();
        }

        return MapToFullDto(character); // Тобі доведеться додати мапінг Attacks у твій MapToFullDto!
    }

    public async Task<RollResponseDto> RollAttackHitAsync(
        Guid characterId,
        Guid attackId,
        Guid userId,
        bool isAdmin
    )
    {
        var character = await _repository.GetByIdWithDetailsAsync(characterId);
        if (character == null)
            throw new Exception("Character Not Found or Access Denied");

        if (!(await IsUserAuthorizedForCharacterAsync(character.Id, userId)) && !isAdmin)
            throw new UnauthorizedAccessException("You do not have permission to edit this character.");

        var attack = character.Attacks.FirstOrDefault(a => a.Id == attackId);
        if (attack == null)
            throw new Exception("Attack Not Found");
        if (!attack.IsAttackRoll)
            throw new Exception("This attack does not require a hit roll.");

        int toHitMod = 0;
        var modParts = new List<string>();

        if (attack.FlatAttackBonus != 0)
        {
            toHitMod += attack.FlatAttackBonus;
            modParts.Add($"{attack.FlatAttackBonus} (Weapon)");
        }

        var effectiveAttackStat = attack.AttackStat ?? (attack.SpellId.HasValue ? character.SpellcastingAbility : null);
        if (effectiveAttackStat.HasValue)
        {
            int statValue = GetStatValue(character, effectiveAttackStat.Value);
            int statMod = (statValue - 10) / 2;
            toHitMod += statMod;
            modParts.Add($"{statMod} ({effectiveAttackStat.Value})");
        }

        if (attack.IsProficient)
        {
            int profBonus = 2 + (character.Level - 1) / 4;
            toHitMod += profBonus;
            modParts.Add($"{profBonus} (Prof)");
        }

        var random = new Random();
        int d20Roll = random.Next(1, 21);

        var resultDto = new RollResponseDto
        {
            RollName = $"To Hit ({attack.Name})",
            Modifier = toHitMod,
            ModifierBreakdown = string.Join(" + ", modParts),
            Rolls = new List<RollDetailDto>
            {
                new RollDetailDto { Value = d20Roll, Sides = 20 },
            },
        };

        // Стріляємо в Discord
        await _discordNotificationService.SendRollAsync(
            character,
            $"⚔️ Attack: {attack.Name}",
            resultDto
        );

        return resultDto;
    }

    // --- 2.2 КИДОК ШКОДИ (DAMAGE) ---
    public async Task<List<DamageRollResultDto>> RollAttackDamageAsync(
        Guid characterId,
        Guid attackId,
        Guid userId,
        bool isAdmin,
        bool isCritical
    )
    {
        var character = await _repository.GetByIdWithDetailsAsync(characterId);
        if (character == null)
            throw new Exception("Character Not Found or Access Denied");

        if (!(await IsUserAuthorizedForCharacterAsync(character.Id, userId)) && !isAdmin)
            throw new UnauthorizedAccessException("You do not have permission to edit this character.");

        var attack = character.Attacks.FirstOrDefault(a => a.Id == attackId);
        if (attack == null)
            throw new Exception("Attack Not Found");

        var random = new Random();
        var results = new List<DamageRollResultDto>();

        foreach (var dmg in attack.Damages)
        {
            int dmgMod = dmg.FlatDamageBonus;
            var modParts = new List<string>();

            if (dmg.FlatDamageBonus != 0)
                modParts.Add($"{dmg.FlatDamageBonus} (Flat)");

            if (dmg.ModifierStat.HasValue)
            {
                int statValue = GetStatValue(character, dmg.ModifierStat.Value);
                int statMod = (statValue - 10) / 2;
                dmgMod += statMod;
                modParts.Add($"{statMod} ({dmg.ModifierStat.Value})");
            }

            var damageRolls = new List<RollDetailDto>();
            int sides = (int)dmg.DiceType;

            // Якщо Крит - подвоюємо кількість кубиків
            int finalDiceCount = isCritical ? dmg.DiceCount * 2 : dmg.DiceCount;

            for (int i = 0; i < finalDiceCount; i++)
            {
                damageRolls.Add(
                    new RollDetailDto { Value = random.Next(1, sides + 1), Sides = sides }
                );
            }

            var dmgResponseDto = new RollResponseDto
            {
                RollName = $"{dmg.DamageType} Damage",
                Modifier = dmgMod,
                ModifierBreakdown = string.Join(" + ", modParts),
                Rolls = damageRolls,
            };

            results.Add(
                new DamageRollResultDto { DamageType = dmg.DamageType, Roll = dmgResponseDto }
            );

            // Відправляємо КОЖЕН тип шкоди як окреме повідомлення в Discord
            // (Або можна було б зібрати їх в один Embed, але для початку зробимо простіше)
            string critPrefix = isCritical ? "💥 CRITICAL " : "🩸 ";
            await _discordNotificationService.SendRollAsync(
                character,
                $"{critPrefix}Damage: {attack.Name} ({dmg.DamageType})",
                dmgResponseDto
            );
        }

        return results;
    }

    // Приватний хелпер для витягування статів, щоб не писати switch кожен раз
    private int GetStatValue(Character c, StatType stat)
    {
        return stat switch
        {
            StatType.Strength => c.Strength,
            StatType.Dexterity => c.Dexterity,
            StatType.Constitution => c.Constitution,
            StatType.Intelligence => c.Intelligence,
            StatType.Wisdom => c.Wisdom,
            StatType.Charisma => c.Charisma,
            _ => 10,
        };
    }

    public async Task<CharacterFullDto> UpdateAttackAsync(
        Guid characterId,
        Guid attackId,
        Guid userId,
        bool isAdmin,
        CreateAttackDto dto
    )
    {
        var character = await _repository.GetByIdWithDetailsAsync(characterId);
        if (character == null)
            throw new Exception("Not Found");

        if (!(await IsUserAuthorizedForCharacterAsync(character.Id, userId)) && !isAdmin)
            throw new UnauthorizedAccessException("You do not have permission to edit this character.");

        var attack = character.Attacks.FirstOrDefault(a => a.Id == attackId);
        if (attack == null)
        {
            throw new Exception("Attack Not Found");
        }

        // 1. Оновлюємо метадані атаки
        attack.Name = dto.Name;
        attack.IsAttackRoll = dto.IsAttackRoll;
        attack.IsProficient = dto.IsProficient;
        attack.AttackStat = dto.AttackStat;
        attack.FlatAttackBonus = dto.FlatAttackBonus;
        attack.ActionCost = dto.ActionCost;
        attack.SpellId = dto.SpellId;

        // 2. Оновлюємо список шкоди (Full Replacement)
        // Очищуємо стару колекцію
        attack.Damages.Clear();

        // Додаємо нові записи з DTO
        foreach (var d in dto.Damages)
        {
            attack.Damages.Add(
                new AttackDamage
                {
                    DiceType = d.DiceType,
                    DiceCount = d.DiceCount,
                    ModifierStat = d.ModifierStat,
                    FlatDamageBonus = d.FlatDamageBonus,
                    DamageType = d.DamageType,
                }
            );
        }

        character.LastModifiedAt = DateTime.UtcNow;

        _repository.Update(character);
        await _repository.SaveChangesAsync();

        // FIX: Load the Spell data so the fully populated DTO is returned and UI doesn't drop the spell
        if (attack.SpellId.HasValue)
        {
            await _context.Entry(attack).Reference(a => a.Spell).LoadAsync();
        }

        return MapToFullDto(character);
    }

    // --- ВИДАЛЕННЯ (DELETE) ---

    public async Task<CharacterFullDto> DeleteAttackAsync(
        Guid characterId,
        Guid attackId,
        Guid userId,
        bool isAdmin
    )
    {
        var character = await _repository.GetByIdWithDetailsAsync(characterId);
        if (character == null)
            throw new Exception("Not Found");

        if (!(await IsUserAuthorizedForCharacterAsync(character.Id, userId)) && !isAdmin)
            throw new UnauthorizedAccessException("You do not have permission to edit this character.");

        var attack = character.Attacks.FirstOrDefault(a => a.Id == attackId);
        if (attack != null)
        {
            character.Attacks.Remove(attack);
            character.LastModifiedAt = DateTime.UtcNow;
            _repository.Update(character);
            await _repository.SaveChangesAsync();
        }

        return MapToFullDto(character);
    }

    public async Task<CharacterFullDto> DeleteTrackerAsync(
        Guid characterId,
        Guid trackerId,
        Guid userId,
        bool isAdmin
    )
    {
        var character = await _repository.GetByIdWithDetailsAsync(characterId);
        if (character == null)
            throw new Exception("Not Found");

        if (!(await IsUserAuthorizedForCharacterAsync(character.Id, userId)) && !isAdmin)
            throw new UnauthorizedAccessException("You do not have permission to edit this character.");

        var tracker = character.ResourceTrackers.FirstOrDefault(t => t.Id == trackerId);
        if (tracker != null)
        {
            character.ResourceTrackers.Remove(tracker);
            character.LastModifiedAt = DateTime.UtcNow;
            _repository.Update(character);
            await _repository.SaveChangesAsync();
        }

        return MapToFullDto(character);
    }

    public async Task<CharacterFullDto> UpdateIntegrationsAsync(
        Guid characterId,
        Guid userId,
        bool isAdmin,
        UpdateIntegrationsDto dto
    )
    {
        var character = await _repository.GetByIdWithDetailsAsync(characterId);

        // Базова перевірка прав доступу
        if (character == null)
            throw new Exception("Character Not Found or Access Denied");

        if (!(await IsUserAuthorizedForCharacterAsync(character.Id, userId)) && !isAdmin)
            throw new UnauthorizedAccessException("You do not have permission to edit this character.");

        // Оновлюємо поля тільки якщо вони передані в JSON
        if (dto.DiscordWebhookUrl != null)
        {
            // Очищаємо від зайвих пробілів
            character.DiscordWebhookUrl = dto.DiscordWebhookUrl.Trim();
        }

        if (dto.ThemeColor != null)
        {
            // Якщо гравець забув решітку — додаємо самі. Якщо передав пусту строку — зачищаємо.
            string color = dto.ThemeColor.Trim();
            if (!string.IsNullOrEmpty(color) && !color.StartsWith("#"))
            {
                color = "#" + color;
            }
            character.ThemeColor = color;
        }

        character.LastModifiedAt = DateTime.UtcNow;

        _repository.Update(character);
        await _repository.SaveChangesAsync();

        return MapToFullDto(character);
    }

    // --- SPELL SLOT METHODS ---
    public async Task<CharacterFullDto> AddSpellSlotAsync(
        Guid characterId,
        Guid userId,
        bool isAdmin,
        CreateSpellSlotDto dto
    )
    {
        var character = await _repository.GetByIdWithDetailsAsync(characterId);

        if (character == null)
            throw new InvalidOperationException($"Character with ID {characterId} not found");

        if (!(await IsUserAuthorizedForCharacterAsync(character.Id, userId)) && !isAdmin)
            throw new UnauthorizedAccessException("You are not authorized to add spell slots to this character");

        // Prevent duplicate spell slots for the same level
        if (character.SpellSlots.Any(s => s.Level == dto.Level))
        {
            throw new InvalidOperationException($"A spell slot for level {dto.Level} already exists.");
        }

        var spellSlot = new DnD.Domain.Entities.SpellSlot
        {
            CharacterId = characterId,
            Level = dto.Level,
            MaxValue = dto.MaxValue,
            CurrentValue = dto.MaxValue,
            ResetCondition = DnD.Domain.Enums.ResetCondition.LongRest
        };

        character.SpellSlots.Add(spellSlot);
        character.LastModifiedAt = DateTime.UtcNow;
        
        _repository.Update(character);
        await _context.SaveChangesAsync();

        return MapToFullDto(character);
    }

    public async Task<CharacterFullDto> UpdateSpellSlotAsync(
        Guid characterId,
        Guid slotId,
        Guid userId,
        bool isAdmin,
        UpdateSpellSlotDto dto
    )
    {
        var character = await _repository.GetByIdWithDetailsAsync(characterId);

        if (character == null)
            throw new InvalidOperationException($"Character with ID {characterId} not found");

        if (!(await IsUserAuthorizedForCharacterAsync(character.Id, userId)) && !isAdmin)
            throw new UnauthorizedAccessException("You are not authorized to update spell slots for this character");

        var spellSlot = character.SpellSlots.FirstOrDefault(s => s.Id == slotId);

        if (spellSlot == null)
            throw new InvalidOperationException($"Spell slot with ID {slotId} not found");

        if (dto.Level.HasValue)
            spellSlot.Level = dto.Level.Value;

        if (dto.MaxValue.HasValue)
            spellSlot.MaxValue = dto.MaxValue.Value;

        if (dto.CurrentValue.HasValue)
            spellSlot.CurrentValue = Math.Min(dto.CurrentValue.Value, spellSlot.MaxValue);

        if (dto.AdjustValue.HasValue)
        {
            spellSlot.CurrentValue = Math.Max(0, Math.Min(spellSlot.CurrentValue + dto.AdjustValue.Value, spellSlot.MaxValue));
        }

        character.LastModifiedAt = DateTime.UtcNow;
        _repository.Update(character);
        await _context.SaveChangesAsync();

        return MapToFullDto(character);
    }

    public async Task<CharacterFullDto> DeleteSpellSlotAsync(
        Guid characterId,
        Guid slotId,
        Guid userId,
        bool isAdmin
    )
    {
        var character = await _repository.GetByIdWithDetailsAsync(characterId);

        if (character == null)
            throw new InvalidOperationException($"Character with ID {characterId} not found");

        if (!(await IsUserAuthorizedForCharacterAsync(character.Id, userId)) && !isAdmin)
            throw new UnauthorizedAccessException("You are not authorized to delete spell slots for this character");

        var spellSlot = character.SpellSlots.FirstOrDefault(s => s.Id == slotId);

        if (spellSlot != null)
        {
            character.SpellSlots.Remove(spellSlot);
            character.LastModifiedAt = DateTime.UtcNow;
            _repository.Update(character);
            await _context.SaveChangesAsync();
        }

        return MapToFullDto(character);
    }
}
