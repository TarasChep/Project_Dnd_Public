using DnD.Application.DTOs;
using DnD.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DnD.Api.Controllers;

[Authorize]
[Route("api/characters")]
public class CombatController : BaseApiController
{
    private readonly ICombatService _combatService;

    public CombatController(ICombatService combatService)
    {
        _combatService = combatService;
    }

    [HttpGet("{id:guid}/combat/actions")]
    public async Task<ActionResult<List<CombatActionDto>>> GetCharacterActions(Guid id)
    {
        try
        {
            var result = await _combatService.GetCharacterActions(id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id:guid}/combat/cast/{actionId:guid}")]
    public async Task<IActionResult> CastSpellAction(Guid id, Guid actionId)
    {
        try
        {
            await _combatService.CastSpellAction(id, actionId);
            return Ok(new { message = "Spell cast successfully, slot consumed." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}