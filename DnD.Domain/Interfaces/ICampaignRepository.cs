using DnD.Domain.Entities;

namespace DnD.Domain.Interfaces;

public interface ICampaignRepository
{
    Task<IEnumerable<CampaignCharacter>> GetPlayerCharactersInCampaignAsync(Guid campaignId);
    Task<IEnumerable<Campaign>> GetUserCampaignsAsync(Guid userId);
    Task AddAsync(Campaign campaign);
    Task<Campaign?> GetByInviteCodeAsync(string inviteCode);
    Task AddMemberAsync(CampaignMember member);
    Task<bool> IsUserInCampaignAsync(Guid campaignId, Guid userId);
    Task<CampaignMember?> GetMemberAsync(Guid campaignId, Guid userId);
    Task<Campaign?> GetByIdWithDetailsAsync(Guid id);
    Task<CampaignCharacter?> GetCampaignCharacterAsync(Guid campaignId, Guid characterId);
    Task AddCharacterAsync(CampaignCharacter character);
    void RemoveCharacter(CampaignCharacter character);
    void Remove(Campaign campaign);
    void RemoveMember(CampaignMember member);
    Task SaveChangesAsync();
}