using DnD.Application.DTOs;
using DnD.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DnD.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CombatAnalyticsController : ControllerBase
{
    private readonly ICombatAnalyticsEngine _engine;
    private readonly IEncounterAnalyticsAdapter _adapter;

    public CombatAnalyticsController(
        ICombatAnalyticsEngine engine, 
        IEncounterAnalyticsAdapter adapter)
    {
        _engine = engine;
        _adapter = adapter;
    }

    /// <summary>
    /// 1. PURE MATH TEST ENDPOINT (No Database)
    /// Send an EncounterAnalyticsDto JSON payload here to test the pure math engine directly.
    /// </summary>
    [HttpPost("test-engine")]
    public ActionResult<EncounterPredictionReport> TestEngineMath([FromBody] EncounterAnalyticsDto payload)
    {
        var report = _engine.AnalyzeEncounter(payload);
        return Ok(report);
    }

    /// <summary>
    /// 2. REAL DATABASE ENDPOINT
    /// Pass an Encounter ID to load data from PostgreSQL -> map to DTO -> run through Math Engine
    /// </summary>
    [HttpGet("{encounterId}/analyze")]
    public async Task<ActionResult<EncounterPredictionReport>> AnalyzeRealEncounter(Guid encounterId)
    {
        var payload = await _adapter.GetAnalyticsPayloadAsync(encounterId);
        var report = _engine.AnalyzeEncounter(payload);
        return Ok(report);
    }
}