using DnD.Application.DTOs;
using DnD.Application.Interfaces;
using DnD.Domain.Entities;
using DnD.Domain.Enums;
using DnD.Domain.Interfaces;
using DnD.Domain.Services;
using System.Text;

namespace DnD.Application.Services;

public class CampaignEncounterService : ICampaignEncounterService
{
    private readonly IEncounterRepository _encounterRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly ICharacterRepository _characterRepository;
    private readonly IDiscordWebhookService _discordWebhookService;

    public CampaignEncounterService(
        IEncounterRepository encounterRepository,
        ICampaignRepository campaignRepository,
        ICharacterRepository characterRepository,
        IDiscordWebhookService discordWebhookService
    )
    {
        _encounterRepository = encounterRepository;
        _campaignRepository = campaignRepository;
        _characterRepository = characterRepository;
        _discordWebhookService = discordWebhookService;
    }

    public async Task<Encounter> CreateEncounterAsync(CreateCampaignEncounterDto dto)
    {
        var encounter = new Encounter
        {
            CampaignId = dto.CampaignId,
            Name = dto.Name,
            IsActive = false,
            CurrentTurnIndex = 0,
        };

        // Rule A: Auto-populate with all active Player Characters in the campaign
        var playerCharacters = await _campaignRepository.GetPlayerCharactersInCampaignAsync(
            dto.CampaignId
        );

        foreach (var pc in playerCharacters)
        {
            encounter.Participants.Add(
                new EncounterParticipant
                {
                    CharacterId = pc.CharacterId,
                    Faction = Faction.Player,
                    CustomName = pc.Character.Name,
                    CurrentHp = pc.Character.CurrentHp, // Rule A: Default to character's current HP
                    MaxHp = pc.Character.MaxHp,
                    InitiativeRoll = 0,
                }
            );
        }

        await _encounterRepository.AddAsync(encounter);
        await _encounterRepository.SaveChangesAsync();

        return encounter;
    }

    public async Task<IEnumerable<EncounterParticipant>> BulkAddParticipantsAsync(
        Guid encounterId,
        BulkAddParticipantsDto dto
    )
    {
        var template =
            await _characterRepository.GetByIdAsync(dto.CharacterId)
            ?? throw new Exception("Character template not found.");

        var existingCount = await _encounterRepository.GetParticipantCountAsync(
            encounterId,
            dto.CharacterId
        );

        var newParticipants = new List<EncounterParticipant>();

        for (int i = 1; i <= dto.Count; i++)
        {
            newParticipants.Add(
                new EncounterParticipant
                {
                    EncounterId = encounterId,
                    CharacterId = dto.CharacterId,
                    Faction = dto.Faction,
                    CustomName = $"{template.Name} {existingCount + i}", // Rule B: Auto-generate custom name
                    CurrentHp = dto.Faction == Faction.Enemy ? template.MaxHp : template.CurrentHp, // Rule B: Enemies start at Max HP
                    MaxHp = template.MaxHp,
                    InitiativeRoll = 0,
                }
            );
        }

        await _encounterRepository.AddParticipantsAsync(newParticipants);
        await _encounterRepository.SaveChangesAsync();

        return newParticipants;
    }

    public async Task StartEncounterAsync(Guid encounterId)
    {
        var encounter = await _encounterRepository.GetByIdWithParticipantsAsync(encounterId)
            ?? throw new Exception("Encounter not found.");

        encounter.IsActive = true;

        var random = new Random();
        var batchMessage = new StringBuilder();
        batchMessage.AppendLine("⚔️ **Encounter Started!** NPC Initiatives:");
        bool hasNpcs = false;

        // ШУКАЄМО ВЕБХУК: Оскільки у монстрів його немає, беремо вебхук з будь-якого гравця в групі
        string? activeWebhook = encounter.Participants
            .Where(p => p.Character != null && !string.IsNullOrWhiteSpace(p.Character.DiscordWebhookUrl))
            .Select(p => p.Character!.DiscordWebhookUrl)
            .FirstOrDefault();

        foreach (var participant in encounter.Participants.Where(p => p.Faction == Faction.Enemy))
        {
            int initMod = 0;
            if (participant.Character != null)
            {
                initMod = DnDCalculator.CalculateInitiative(participant.Character.Dexterity, participant.Character.AdditionalInitiativeBonus);
            }
            
            int d20Roll = random.Next(1, 21);
            participant.InitiativeRoll = d20Roll + initMod;
            
            string name = participant.CustomName ?? participant.Character?.Name ?? "Enemy";
            
            string critIndicator = d20Roll == 20 ? "🌟 **CRITICAL!** " : (d20Roll == 1 ? "💀 **FAIL!** " : "");
            
            batchMessage.AppendLine($"- **{name}** rolled: {critIndicator}**{participant.InitiativeRoll}** *(d20: {d20Roll} + {initMod})*");
            hasNpcs = true;
        }

        _encounterRepository.Update(encounter);
        await _encounterRepository.SaveChangesAsync();

        if (hasNpcs)
        {
            await _discordWebhookService.SendMessageAsync(batchMessage.ToString(), activeWebhook);
        }
    }

    public async Task UpdateInitiativeAsync(Guid participantId, int roll)
    {
        // ТУТ ПІДСТАВ СВІЙ РЕПОЗИТОРІЙ/DBCONTEXT
        var participant =
            await _encounterRepository.GetParticipantByIdAsync(participantId)
            ?? throw new Exception("Participant not found.");

        participant.InitiativeRoll = roll;

        _encounterRepository.Update(participant);
        await _encounterRepository.SaveChangesAsync();
        
        // Знайдемо Encounter щоб дістати вебхук когось із гравців
        var encounter = await _encounterRepository.GetByIdWithParticipantsAsync(participant.EncounterId);
        string? activeWebhook = encounter?.Participants
            .Where(p => p.Character != null && !string.IsNullOrWhiteSpace(p.Character.DiscordWebhookUrl))
            .Select(p => p.Character!.DiscordWebhookUrl)
            .FirstOrDefault();
            
        await _discordWebhookService.SendInitiativeRollAsync(participant.CustomName ?? participant.Character?.Name ?? "Participant", roll, activeWebhook);
    }

    public async Task NextTurnAsync(Guid encounterId)
    {
        var encounter =
            await _encounterRepository.GetByIdWithParticipantsAsync(encounterId)
            ?? throw new Exception("Encounter not found.");

        if (!encounter.Participants.Any())
            return;

        // Збільшуємо індекс на 1
        encounter.CurrentTurnIndex++;

        // Якщо індекс вийшов за межі кількості учасників - скидаємо на 0 (Початок нового раунду)
        if (encounter.CurrentTurnIndex >= encounter.Participants.Count)
        {
            encounter.CurrentTurnIndex = 0;
            // Тут в майбутньому можна буде додати RoundCount++
        }

        _encounterRepository.Update(encounter);
        await _encounterRepository.SaveChangesAsync();
    }

    public async Task DeleteEncounterAsync(Guid encounterId)
    {
        var encounter = await _encounterRepository.GetByIdWithParticipantsAsync(encounterId);
        if (encounter != null)
            {
            _encounterRepository.Remove(encounter);
            await _encounterRepository.SaveChangesAsync();
        }
    }

    public async Task EndEncounterAsync(Guid encounterId)
    {
        var encounter = await _encounterRepository.GetByIdWithParticipantsAsync(encounterId);
        if (encounter != null)
        {
            encounter.IsActive = false;
            encounter.CurrentTurnIndex = 0;
            foreach (var participant in encounter.Participants)
            {
                participant.InitiativeRoll = 0;
            }
            _encounterRepository.Update(encounter);
            await _encounterRepository.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<object>> GetEncountersByCampaignAsync(Guid campaignId)
    {
        var encounters = await _encounterRepository.GetAllAsync();
        return encounters
            .Where(e => e.CampaignId == campaignId)
            .Select(e => new { id = e.Id, name = e.Name, isActive = e.IsActive });
    }
}
