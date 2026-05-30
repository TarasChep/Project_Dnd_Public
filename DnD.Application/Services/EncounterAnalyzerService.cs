using DnD.Application.DTOs;
using DnD.Application.Interfaces;
using DnD.Domain.Entities;
using DnD.Domain.Enums;
using DnD.Domain.Interfaces;
using DnD.Domain.Services;
using DnD.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DnD.Application.Services;

public class EncounterAnalyzerService : IEncounterAnalyzerService
{
    private readonly IEncounterRepository _encounterRepository;
    // Now uniquely resolves to DnD.Application.Interfaces.ICombatAnalyticsEngine
    private readonly ICombatAnalyticsEngine _engine; 
    private readonly ICharacterRepository _characterRepository;
    private readonly ApplicationDbContext _context;

    public EncounterAnalyzerService(
        IEncounterRepository encounterRepository, 
        ICombatAnalyticsEngine engine,
        ICharacterRepository characterRepository,
        ApplicationDbContext context)
    {
        _encounterRepository = encounterRepository;
        _engine = engine;
        _characterRepository = characterRepository;
        _context = context;
    }

    public async Task<IEnumerable<EncounterBriefDto>> GetAllEncountersAsync()
    {
        var encounters = await _encounterRepository.GetAllAsync();
        return encounters.Select(e => new EncounterBriefDto
        {
            Id = e.Id,
            Name = e.Name,
            PlayerCount = e.Participants.Count(p => p.Faction == Faction.Player),
            EnemyCount = e.Participants.Count(p => p.Faction == Faction.Enemy)
        });
    }

    public async Task<EncounterDetailDto?> GetEncounterByIdAsync(Guid id, Guid currentUserId, bool isSystemAdmin)
    {
        // 1. Optimized SQL Projection (No .Include(), prevents pulling heavy character blobs)
        var data = await _context.Encounters
            .Where(e => e.Id == id)
            .Select(e => new 
            {
                EncounterId = e.Id,
                e.Name,
                e.Description,
                e.IsActive,
                e.CurrentTurnIndex,
                GmUserId = e.Campaign != null ? e.Campaign.GmUserId : Guid.Empty,
                Participants = e.Participants.Select(p => new 
                {
                    p.Id,
                    p.CharacterId,
                    CharacterName = p.Character != null ? p.Character.Name : "Unknown",
                    OwnerUserId = p.Character != null ? p.Character.UserId : Guid.Empty,
                    p.Faction,
                    p.CustomName,
                    p.InitiativeRoll
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (data == null) return null;

        bool isGm = isSystemAdmin || data.GmUserId == currentUserId;

        var dto = new EncounterDetailDto
        {
            Id = data.EncounterId,
            Name = data.Name,
            Description = data.Description,
            IsActive = data.IsActive,
            CurrentTurnIndex = data.CurrentTurnIndex,
            Participants = new List<ParticipantDetailDto>()
        };

        // 2. Metagame Data Sanitization & Strict FOV Filtering
        foreach (var p in data.Participants)
        {
            var partDto = new ParticipantDetailDto
            {
                Id = p.Id,
                CharacterId = p.CharacterId,
                CharacterName = p.CharacterName,
                Faction = p.Faction,
                CustomName = p.CustomName,
                InitiativeRoll = p.InitiativeRoll,
                CurrentHp = 0, // HP data completely purged from DTO mapping
                MaxHp = 0      // HP data completely purged from DTO mapping
            };

            dto.Participants.Add(partDto);
        }

        return dto;
    }

    public async Task<Guid> CreateEncounterAsync(CreateEncounterDto dto)
    {
        var encounter = new Encounter
        {
            Name = dto.Name,
            Description = dto.Description,
            IsActive = false
        };

        await _encounterRepository.AddAsync(encounter);
        await _encounterRepository.SaveChangesAsync();

        return encounter.Id;
    }

    public async Task<bool> AddParticipantAsync(Guid encounterId, AddParticipantDto dto)
    {
        var encounter = await _encounterRepository.GetByIdWithParticipantsAsync(encounterId);
        if (encounter == null) return false;

        // Підтягуємо оригінальну анкету, щоб взяти її базові стати, якщо потрібно
        var character = await _characterRepository.GetByIdAsync(dto.CharacterId);
        if (character == null) return false;

        var participant = new EncounterParticipant
        {
            EncounterId = encounterId,
            CharacterId = dto.CharacterId,
            Faction = dto.Faction,
            CurrentHp = dto.CurrentHp ?? character.CurrentHp, // Дефолтне HP
            CustomName = dto.CustomName ?? character.Name,
            MaxHp = character.MaxHp
        };
        await _encounterRepository.AddParticipantAsync(participant);
        await _encounterRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RemoveParticipantAsync(Guid encounterId, Guid participantId)
    {
        var encounter = await _encounterRepository.GetByIdWithDetailsAsync(encounterId);
        if (encounter == null) return false;

        var participant = encounter.Participants.FirstOrDefault(p => p.Id == participantId);
        if (participant == null) return false;

        _encounterRepository.RemoveParticipant(participant);
        await _encounterRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdateParticipantAsync(Guid encounterId, Guid participantId, UpdateParticipantDto dto)
    {
        var encounter = await _encounterRepository.GetByIdWithDetailsAsync(encounterId);
        if (encounter == null) return false;

        var participant = encounter.Participants.FirstOrDefault(p => p.Id == participantId);
        if (participant == null) return false;

        if (dto.CurrentHp.HasValue) participant.CurrentHp = dto.CurrentHp.Value;
        if (dto.CustomName != null) participant.CustomName = dto.CustomName;

        await _encounterRepository.SaveChangesAsync();

        return true;
    }

    public async Task<EncounterAnalysisDto> AnalyzeEncounterAsync(Guid encounterId)
    {
        // 1. Витягуємо дані з БД (всі пов'язані сутності для розрахунку)
        var encounter = await _encounterRepository.GetByIdWithDetailsAsync(encounterId);

        if (encounter == null)
            throw new Exception("Encounter not found");

        var players = encounter.Participants.Where(p => p.Faction == Faction.Player).ToList();
        var enemies = encounter.Participants.Where(p => p.Faction == Faction.Enemy).ToList();

        var result = new EncounterAnalysisDto
        {
            EncounterId = encounter.Id,
            EncounterName = encounter.Name,
        };

        if (!players.Any() || !enemies.Any())
        {
            result.Verdict = "Insufficient participants for analysis.";
            return result;
        }

        // 2. Агрегація базових статів
        result.PartyTotalHp = players.Sum(p => p.CurrentHp);
        result.PartyAvgAc = players.Any() ? players.Average(p => p.Character?.ArmorClass ?? 10) : 10;
        result.EnemyTotalHp = enemies.Sum(p => p.CurrentHp);
        result.EnemyAvgAc = enemies.Any() ? enemies.Average(p => p.Character?.ArmorClass ?? 10) : 10;

        // 3. Аналіз кожної сторони
        foreach (var player in players)
        {
            double dpr = player.Character != null ? CalculateMaxParticipantDpr(player.Character, (int)result.EnemyAvgAc) : 0;
            result.PartyTotalEdpr += dpr;

            result.Participants.Add(
                new ParticipantAnalysisDto
                {
                    Name = player.CustomName ?? player.Character?.Name ?? "Unknown",
                    Faction = "Player",
                    Hp = player.CurrentHp,
                    Ac = player.Character?.ArmorClass ?? 10,
                    Edpr = Math.Round(dpr, 2),
                }
            );
        }

        foreach (var enemy in enemies)
        {
            double dpr = enemy.Character != null ? CalculateMaxParticipantDpr(enemy.Character, (int)result.PartyAvgAc) : 0;
            result.EnemyTotalEdpr += dpr;

            result.Participants.Add(
                new ParticipantAnalysisDto
                {
                    Name = enemy.CustomName ?? enemy.Character?.Name ?? "Unknown",
                    Faction = "Enemy",
                    Hp = enemy.CurrentHp,
                    Ac = enemy.Character?.ArmorClass ?? 10,
                    Edpr = Math.Round(dpr, 2),
                }
            );
        }

        // 4. Time To Kill (Захист від ділення на 0)
        result.PartyTtk = 
            result.EnemyTotalEdpr > 0 
                ? Math.Round(result.PartyTotalHp / result.EnemyTotalEdpr, 1) 
                : 999;

        result.EnemyTtk = 
            result.PartyTotalEdpr > 0 
                ? Math.Round(result.EnemyTotalHp / result.PartyTotalEdpr, 1) 
                : 999;

        // 5. Логіка вердикту
        double survivalRatio = result.PartyTtk / (result.EnemyTtk > 0 ? result.EnemyTtk : 1);

        if (survivalRatio > 2.0)
            result.Verdict = "Easy (Trivial)";
        else if (survivalRatio > 1.2)
            result.Verdict = "Medium (Balanced)";
        else if (survivalRatio > 0.8)
            result.Verdict = "Hard (High Risk)";
        else
            result.Verdict = "Deadly (Probable TPK)";

        // Округлення для JSON
        result.PartyAvgAc = Math.Round(result.PartyAvgAc, 1);
        result.EnemyAvgAc = Math.Round(result.EnemyAvgAc, 1);
        result.PartyTotalEdpr = Math.Round(result.PartyTotalEdpr, 2);
        result.EnemyTotalEdpr = Math.Round(result.EnemyTotalEdpr, 2);

        return result;
    }

    private double CalculateMaxParticipantDpr(Character character, int targetAc)
    {
        if (character.Attacks == null || !character.Attacks.Any())
            return 0;

        double maxDpr = 0;

        foreach (var attack in character.Attacks)
        {
            int attackBonus = attack.FlatAttackBonus;

            if (attack.AttackStat.HasValue)
            {
                int statValue = GetStatValue(character, attack.AttackStat.Value);
                attackBonus += DnDCalculator.CalculateModifier(statValue);
            }

            if (attack.IsProficient)
            {
                attackBonus += DnDCalculator.CalculateProficiency(character.Level);
            }

            double currentAttackDpr = 0;

            foreach (var damage in attack.Damages)
            {
                int flatDamageBonus = damage.FlatDamageBonus;

                if (damage.ModifierStat.HasValue)
                {
                    int dmgStatValue = GetStatValue(character, damage.ModifierStat.Value);
                    flatDamageBonus += DnDCalculator.CalculateModifier(dmgStatValue);
                }

                currentAttackDpr += _engine.CalculateExpectedDamagePerAttack(
                    attackBonus: attackBonus,
                    targetAc: targetAc,
                    diceCount: damage.DiceCount,
                    diceType: (int)damage.DiceType,
                    flatBonus: flatDamageBonus
                );
            }

            // Знаходимо найефективнішу атаку (система вважає, що персонаж буде "спамити" її)
            if (currentAttackDpr > maxDpr)
                maxDpr = currentAttackDpr;
        }

        return maxDpr;
    }

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
}
