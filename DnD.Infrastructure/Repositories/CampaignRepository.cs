using DnD.Domain.Entities;
using DnD.Domain.Interfaces;
using DnD.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DnD.Infrastructure.Repositories;

public class CampaignRepository : ICampaignRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CampaignRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<CampaignCharacter>> GetPlayerCharactersInCampaignAsync(Guid campaignId)
    {
        return await _dbContext.CampaignCharacters
            .Include(cc => cc.Character)
            .Where(cc => cc.CampaignId == campaignId && cc.IsPlayerCharacter)
            .ToListAsync();
    }

    public async Task<IEnumerable<Campaign>> GetUserCampaignsAsync(Guid userId)
    {
        return await _dbContext.Campaigns
            .Where(c => c.Members.Any(m => m.UserId == userId))
            .ToListAsync();
    }

    public async Task AddAsync(Campaign campaign)
    {
        await _dbContext.Campaigns.AddAsync(campaign);
    }

    public async Task<Campaign?> GetByInviteCodeAsync(string inviteCode)
    {
        return await _dbContext.Campaigns.FirstOrDefaultAsync(c => c.InviteCode == inviteCode);
    }

    public async Task AddMemberAsync(CampaignMember member)
    {
        await _dbContext.CampaignMembers.AddAsync(member);
    }

    public async Task<bool> IsUserInCampaignAsync(Guid campaignId, Guid userId)
    {
        return await _dbContext.CampaignMembers.AnyAsync(cm => cm.CampaignId == campaignId && cm.UserId == userId);
    }

    public async Task<CampaignMember?> GetMemberAsync(Guid campaignId, Guid userId)
    {
        return await _dbContext.CampaignMembers.FirstOrDefaultAsync(cm => cm.CampaignId == campaignId && cm.UserId == userId);
    }

    public async Task<Campaign?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _dbContext.Campaigns
            .Include(c => c.Members)
                .ThenInclude(m => m.User)
            .Include(c => c.Characters)
                .ThenInclude(cc => cc.Character)
            .Include(c => c.Folders)
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<CampaignCharacter?> GetCampaignCharacterAsync(Guid campaignId, Guid characterId)
    {
        return await _dbContext.CampaignCharacters
            .FirstOrDefaultAsync(cc => cc.CampaignId == campaignId && cc.CharacterId == characterId);
    }

    public async Task AddCharacterAsync(CampaignCharacter character)
    {
        await _dbContext.CampaignCharacters.AddAsync(character);
    }

    public void RemoveCharacter(CampaignCharacter character)
    {
        _dbContext.CampaignCharacters.Remove(character);
    }

    public void Remove(Campaign campaign)
    {
        _dbContext.Campaigns.Remove(campaign);
    }

    public void RemoveMember(CampaignMember member)
    {
        _dbContext.CampaignMembers.Remove(member);
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}