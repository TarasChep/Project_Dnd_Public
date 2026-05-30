using DnD.Application.DTOs;

namespace DnD.Application.Interfaces;

public interface ICampaignService
{
    Task<IEnumerable<CampaignResponseDto>> GetMyCampaignsAsync(Guid userId);
    Task<CampaignResponseDto> CreateCampaignAsync(CreateCampaignDto dto, Guid gmUserId);
    Task<CampaignResponseDto> JoinCampaignAsync(string inviteCode, Guid userId);
    Task<CampaignDetailDto?> GetCampaignDetailsAsync(Guid campaignId, Guid userId);
    Task<bool> AddCharacterToCampaignAsync(Guid campaignId, AddCampaignCharacterDto dto, Guid userId);
    Task<bool> ApproveJoinRequestAsync(Guid campaignId, Guid targetUserId, Guid gmUserId);
    Task<bool> RejectJoinRequestAsync(Guid campaignId, Guid targetUserId, Guid gmUserId);
    Task<bool> RemoveCharacterFromCampaignAsync(Guid campaignId, Guid characterId, Guid userId);
    Task<bool> ToggleCharacterGlobalVisibilityAsync(Guid campaignId, Guid characterId, Guid userId, bool isVisible);
    Task<bool> GrantCharacterAccessAsync(Guid campaignId, Guid characterId, Guid gmUserId, Guid targetUserId);
    Task<bool> RevokeCharacterAccessAsync(Guid campaignId, Guid characterId, Guid gmUserId, Guid targetUserId);
    Task<bool> DeleteCampaignAsync(Guid campaignId, Guid userId);
    Task<bool> LeaveCampaignAsync(Guid campaignId, Guid userId);
}