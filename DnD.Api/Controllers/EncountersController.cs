using DnD.Application.DTOs;
using DnD.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DnD.Infrastructure.Persistence;

namespace DnD.Api.Controllers;

[Authorize]
[Route("api/encounters")]
public class EncountersController : BaseApiController
{
    private readonly IEncounterAnalyzerService _analyzerService;

    public EncountersController(IEncounterAnalyzerService analyzerService)
    {
        _analyzerService = analyzerService;
    }

    /// <summary>
    /// Отримати список усіх бойових зіткнень.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EncounterBriefDto>>> GetAll()
    {
        var encounters = await _analyzerService.GetAllEncountersAsync();
        return Ok(encounters);
    }

    /// <summary>
    /// Отримати детальну інформацію про конкретне зіткнення та його учасників.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EncounterDetailDto>> GetById(Guid id)
    {
        var encounter = await _analyzerService.GetEncounterByIdAsync(id, GetCurrentUserId(), IsAdmin);
        
        if (encounter == null)
            return NotFound(new { message = "Encounter not found." });

        return Ok(encounter);
    }

    /// <summary>
    /// Отримати детальну бойову аналітику для зіткнення (EDPR, Time to Kill, Verdict).
    /// </summary>
    [HttpGet("{id:guid}/analyze")]
    public async Task<ActionResult<EncounterPredictionReport>> AnalyzeEncounter(Guid id, [FromServices] IEncounterAnalyticsAdapter adapter, [FromServices] ICombatAnalyticsEngine engine)
    {
        try
        {
            var payload = await adapter.GetAnalyticsPayloadAsync(id);
            var result = engine.AnalyzeEncounter(payload);
            return Ok(result);
        }
        catch (Exception ex)
        {
            if (ex.Message == "Encounter not found")
                return NotFound(new { message = "Encounter not found." });

            return BadRequest(new { error = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    /// <summary>
    /// Створити нове бойове зіткнення (Encounter)
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Guid>> CreateEncounter([FromBody] CreateEncounterDto dto)
    {
        try
        {
            var id = await _analyzerService.CreateEncounterAsync(dto);
            return Ok(new { EncounterId = id });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Додати учасника (гравця або монстра) до зіткнення
    /// </summary>
    [HttpPost("{id:guid}/participants")]
    public async Task<IActionResult> AddParticipant(Guid id, [FromBody] AddParticipantDto dto)
    {
        var success = await _analyzerService.AddParticipantAsync(id, dto);
        if (!success)
            return NotFound(new { message = "Encounter not found." });

        return Ok(new { message = "Participant added successfully." });
    }

    /// <summary>
    /// Видалити учасника з бойового зіткнення
    /// </summary>
    [HttpDelete("{id:guid}/participants/{participantId:guid}")]
    public async Task<IActionResult> RemoveParticipant(Guid id, Guid participantId)
    {
        var success = await _analyzerService.RemoveParticipantAsync(id, participantId);
        if (!success)
            return NotFound(new { message = "Encounter or participant not found." });

        return Ok(new { message = "Participant removed successfully." });
    }

    /// <summary>
    /// Оновити параметри учасника (наприклад, зняти йому HP після атаки)
    /// </summary>
    [HttpPatch("{id:guid}/participants/{participantId:guid}")]
    public async Task<IActionResult> UpdateParticipant(Guid id, Guid participantId, [FromBody] UpdateParticipantDto dto)
    {
        var success = await _analyzerService.UpdateParticipantAsync(id, participantId, dto);
        if (!success)
            return NotFound(new { message = "Encounter or participant not found." });

        return Ok(new { message = "Participant updated successfully." });
    }

    /// <summary>
    /// Створити бойове зіткнення, прив'язане до кампанії
    /// </summary>
    [HttpPost("campaign")]
    public async Task<IActionResult> CreateCampaignEncounter([FromBody] CreateCampaignEncounterDto dto, [FromServices] ICampaignEncounterService encounterService)
    {
        try
        {
            var encounter = await encounterService.CreateEncounterAsync(dto);
            return Ok(new { id = encounter.Id, name = encounter.Name, isActive = encounter.IsActive });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Розпочати бойове зіткнення (генерує ініціативу для ворогів)
    /// </summary>
    [HttpPatch("{id:guid}/start")]
    public async Task<IActionResult> StartEncounter(Guid id, [FromServices] ICampaignEncounterService encounterService)
    {
        try
        {
            await encounterService.StartEncounterAsync(id);
            return Ok(new { message = "Encounter started successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Отримати список бойових зіткнень для конкретної кампанії
    /// </summary>
    [HttpGet("campaign/{campaignId:guid}")]
    public async Task<IActionResult> GetCampaignEncounters(Guid campaignId, [FromServices] ICampaignEncounterService encounterService)
    {
        try
        {
            var service = (DnD.Application.Services.CampaignEncounterService)encounterService;
            var encounters = await service.GetEncountersByCampaignAsync(campaignId);
            return Ok(encounters);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Завершити бойове зіткнення
    /// </summary>
    [HttpPatch("{id:guid}/end")]
    public async Task<IActionResult> EndEncounter(Guid id, [FromServices] ICampaignEncounterService encounterService)
    {
        try
        {
            var service = (DnD.Application.Services.CampaignEncounterService)encounterService;
            await service.EndEncounterAsync(id);
            return Ok(new { message = "Encounter ended successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Оновити ініціативу конкретного учасника
    /// </summary>
    [HttpPatch("participants/{participantId:guid}/initiative")]
    public async Task<IActionResult> UpdateInitiative(Guid participantId, [FromBody] UpdateInitiativeDto dto, [FromServices] ICampaignEncounterService encounterService)
    {
        try
        {
            await encounterService.UpdateInitiativeAsync(participantId, dto.Initiative);
            return Ok(new { message = "Initiative updated successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Передати хід наступному учаснику
    /// </summary>
    [HttpPost("{id:guid}/next-turn")]
    public async Task<IActionResult> NextTurn(Guid id, [FromServices] ICampaignEncounterService encounterService)
    {
        try
        {
            await encounterService.NextTurnAsync(id);
            return Ok(new { message = "Moved to next turn." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Видалити бойове зіткнення
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteEncounter(Guid id, [FromServices] ICampaignEncounterService encounterService)
    {
        try
        {
            var service = (DnD.Application.Services.CampaignEncounterService)encounterService;
            await service.DeleteEncounterAsync(id);
            return Ok(new { message = "Encounter deleted successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Оновити ініціативу для персонажа в активному зіткненні (використовується гравцями)
    /// </summary>
    [HttpPatch("active/characters/{characterId:guid}/initiative")]
    public async Task<IActionResult> UpdateInitiativeByCharacter(Guid characterId, [FromBody] UpdateInitiativeDto dto, [FromServices] ApplicationDbContext dbContext)
    {
        try
        {
            var participant = await dbContext.EncounterParticipants
                .Include(p => p.Encounter)
                    .Where(p => p.CharacterId == characterId)
                    .OrderByDescending(p => p.Encounter.CreatedAt)
                    .FirstOrDefaultAsync();

            if (participant != null)
            {
                participant.InitiativeRoll = dto.Initiative;
                await dbContext.SaveChangesAsync();
            }
            return Ok(new { message = "Initiative updated successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
