using DnD.Application.DTOs;
using DnD.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DnD.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
public class CampaignsController : BaseApiController
{
    private readonly ICampaignService _campaignService;

    public CampaignsController(ICampaignService campaignService)
    {
        _campaignService = campaignService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyCampaigns()
    {
        try
        {
            var campaigns = await _campaignService.GetMyCampaignsAsync(GetCurrentUserId());
            return Ok(campaigns);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCampaignDto dto)
    {
        try
        {
            var result = await _campaignService.CreateCampaignAsync(dto, GetCurrentUserId());
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("join/{inviteCode}")]
    public async Task<IActionResult> Join(string inviteCode)
    {
        try
        {
            var result = await _campaignService.JoinCampaignAsync(inviteCode, GetCurrentUserId());
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDetails(Guid id)
    {
        var campaign = await _campaignService.GetCampaignDetailsAsync(id, GetCurrentUserId());
        if (campaign == null)
            return NotFound(new { message = "Campaign not found or access denied." });

        return Ok(campaign);
    }

    [HttpPost("{id:guid}/members/{userId:guid}/approve")]
    public async Task<IActionResult> ApproveMember(Guid id, Guid userId)
    {
        var success = await _campaignService.ApproveJoinRequestAsync(id, userId, GetCurrentUserId());
        if (!success) return BadRequest(new { message = "Could not approve member." });
        return Ok();
    }

    [HttpPost("{id:guid}/members/{userId:guid}/reject")]
    public async Task<IActionResult> RejectMember(Guid id, Guid userId)
    {
        var success = await _campaignService.RejectJoinRequestAsync(id, userId, GetCurrentUserId());
        if (!success) return BadRequest(new { message = "Could not reject member." });
        return Ok();
    }

    [HttpPost("{id:guid}/characters")]
    public async Task<IActionResult> AddCharacter(Guid id, [FromBody] AddCampaignCharacterDto dto)
    {
        try 
        {
            await _campaignService.AddCharacterToCampaignAsync(id, dto, GetCurrentUserId());
            return Ok(new { message = "Character added successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}/characters/{characterId:guid}")]
    public async Task<IActionResult> RemoveCharacter(Guid id, Guid characterId)
    {
        var success = await _campaignService.RemoveCharacterFromCampaignAsync(id, characterId, GetCurrentUserId());
        if (!success) return BadRequest(new { message = "Could not remove character." });
        return Ok(new { message = "Character removed successfully." });
    }

    [HttpPatch("{id:guid}/characters/{characterId:guid}/visibility")]
    public async Task<IActionResult> ToggleGlobalVisibility(Guid id, Guid characterId, [FromBody] bool isVisible)
    {
        var success = await _campaignService.ToggleCharacterGlobalVisibilityAsync(id, characterId, GetCurrentUserId(), isVisible);
        if (!success) return BadRequest(new { message = "Could not update visibility. Ensure you are the GM." });
        return Ok(new { message = "Visibility updated successfully." });
    }

    [HttpPost("{id:guid}/characters/{characterId:guid}/access/{targetUserId:guid}")]
    public async Task<IActionResult> GrantAccess(Guid id, Guid characterId, Guid targetUserId)
    {
        var success = await _campaignService.GrantCharacterAccessAsync(id, characterId, GetCurrentUserId(), targetUserId);
        if (!success) return BadRequest(new { message = "Could not grant access. Ensure you are the GM." });
        return Ok(new { message = "Access granted successfully." });
    }

    [HttpDelete("{id:guid}/characters/{characterId:guid}/access/{targetUserId:guid}")]
    public async Task<IActionResult> RevokeAccess(Guid id, Guid characterId, Guid targetUserId)
    {
        var success = await _campaignService.RevokeCharacterAccessAsync(id, characterId, GetCurrentUserId(), targetUserId);
        if (!success) return BadRequest(new { message = "Could not revoke access." });
        return Ok(new { message = "Access revoked successfully." });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCampaign(Guid id)
    {
        var success = await _campaignService.DeleteCampaignAsync(id, GetCurrentUserId());
        if (!success) return BadRequest(new { message = "Could not delete campaign. Only the GM can delete a campaign." });
        return Ok(new { message = "Campaign deleted successfully." });
    }

    [HttpDelete("{id:guid}/members/{userId:guid}")]
    public async Task<IActionResult> LeaveCampaign(Guid id, Guid userId)
    {
        var success = await _campaignService.LeaveCampaignAsync(id, GetCurrentUserId());
        if (!success) return BadRequest(new { message = "Could not leave campaign. You may not be a member or you are the GM." });
        return Ok(new { message = "You have left the campaign." });
    }

    [HttpPost("{id:guid}/folders")]
    public async Task<IActionResult> CreateFolder(Guid id, [FromBody] CreateFolderDto dto)
    {
        try
        {
            // Casting to the concrete service since ICampaignService may not have this method yet
            var service = (DnD.Application.Services.CampaignService)_campaignService;
            var folder = await service.CreateFolderAsync(id, dto.Name, GetCurrentUserId());
            return Ok(folder);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}