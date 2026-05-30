using DnD.Application.DTOs;
using DnD.Application.Interfaces;
using DnD.Domain.Entities;
using DnD.Domain.Enums;
using DnD.Domain.Interfaces;

namespace DnD.Application.Services;

public class CampaignService : ICampaignService
{
    private readonly ICampaignRepository _campaignRepository;
    private readonly ICharacterRepository _characterRepository;

    public CampaignService(
        ICampaignRepository campaignRepository,
        ICharacterRepository characterRepository
    )
    {
        _campaignRepository = campaignRepository;
        _characterRepository = characterRepository;
    }

    public async Task<IEnumerable<CampaignResponseDto>> GetMyCampaignsAsync(Guid userId)
    {
        var campaigns = await _campaignRepository.GetUserCampaignsAsync(userId);

        return campaigns.Select(c => new CampaignResponseDto
        {
            Id = c.Id,
            Name = c.Name,
            GmUserId = c.GmUserId,
            InviteCode = c.InviteCode,
            CreatedAt = c.CreatedAt,
            Role = c.GmUserId == userId ? "GM" : "Player"
        }).ToList();
    }

    public async Task<CampaignResponseDto> CreateCampaignAsync(CreateCampaignDto dto, Guid gmUserId)
    {
        var campaign = new Campaign
        {
            Name = dto.Name,
            GmUserId = gmUserId,
            InviteCode = GenerateInviteCode(),
        };

        await _campaignRepository.AddAsync(campaign);

        // Automatically add the GM as an approved member
        var gmMember = new CampaignMember
        {
            CampaignId = campaign.Id,
            UserId = gmUserId,
            Role = CampaignRole.GM,
            Status = CampaignMemberStatus.Approved,
        };

        await _campaignRepository.AddMemberAsync(gmMember);
        await _campaignRepository.SaveChangesAsync();

        return new CampaignResponseDto
        {
            Id = campaign.Id,
            Name = campaign.Name,
            GmUserId = campaign.GmUserId,
            InviteCode = campaign.InviteCode,
            CreatedAt = campaign.CreatedAt,
            Role = "GM"
        };
    }

    public async Task<CampaignResponseDto> JoinCampaignAsync(string inviteCode, Guid userId)
    {
        var campaign =
            await _campaignRepository.GetByInviteCodeAsync(inviteCode)
            ?? throw new Exception("Invalid invite code.");

        if (await _campaignRepository.IsUserInCampaignAsync(campaign.Id, userId))
            throw new Exception("You are already a member of this campaign.");

        var playerMember = new CampaignMember
        {
            CampaignId = campaign.Id,
            UserId = userId,
            Role = CampaignRole.Player,
            Status = CampaignMemberStatus.Approved, // ГРАВЕЦЬ ОДРАЗУ ОТРИМУЄ ДОСТУП
        };

        await _campaignRepository.AddMemberAsync(playerMember);
        await _campaignRepository.SaveChangesAsync();

        return new CampaignResponseDto
        {
            Id = campaign.Id,
            Name = campaign.Name,
            GmUserId = campaign.GmUserId,
            InviteCode = campaign.InviteCode,
            Role = "Player"
        };
    }

    public async Task<CampaignDetailDto?> GetCampaignDetailsAsync(Guid campaignId, Guid userId)
    {
        var campaign = await _campaignRepository.GetByIdWithDetailsAsync(campaignId);
        if (campaign == null)
            return null;

        bool isGm = campaign.GmUserId == userId;

        // ПЕРЕВІРКА БЕЗПЕКИ: Чи є гравець Approved?
        var currentMember = campaign.Members.FirstOrDefault(m => m.UserId == userId);
        if (!isGm && currentMember == null)
        {
            return null; // Сторонні отримують справжній 404/403
        }

        // STRICT FOV FILTERING: 
        // If the user is a Player, only return their own characters in the Campaign Dashboard.
        var visibleCharacters = isGm
            ? campaign.Characters.ToList()
            : campaign.Characters
                .Where(c => c.Character != null && c.Character.UserId == userId)
                .ToList();

        return new CampaignDetailDto
        {
            Id = campaign.Id,
            Name = campaign.Name,
            GmUserId = campaign.GmUserId,
            InviteCode = campaign.InviteCode,
            CreatedAt = campaign.CreatedAt,
            Role = isGm ? "GM" : currentMember?.Role.ToString() ?? "Player",
            Members = campaign
                .Members.Select(m => new CampaignMemberDto
                {
                    UserId = m.UserId,
                    UserName = m.User?.UserName ?? "Unknown",
                    Role = m.Role.ToString(),
                    Status = m.Status.ToString()
                })
                .ToList(),
            Characters = visibleCharacters
                .Select(c => new CampaignCharacterDto
                {
                    CharacterId = c.CharacterId,
                    CharacterName = c.Character?.Name ?? "Unknown",
                    OwnerUserId = c.Character?.UserId ?? Guid.Empty,
                    OwnerUserName = c.IsPlayerCharacter ? (campaign.Members.FirstOrDefault(m => m.UserId == c.Character?.UserId)?.User?.UserName ?? "Unknown") : string.Empty,
                    IsVisibleToAllPlayers = c.IsVisibleToAllPlayers,
                    VisibleToUserIds = c.VisibleToUserIds,
                    IsPlayerCharacter = c.IsPlayerCharacter,
                    FolderId = c.FolderId,
                    Character = c.Character != null ? new CharacterBriefDto
                    {
                        Id = c.Character.Id,
                        Name = c.Character.Name,
                        Level = c.Character.Level,
                        Class = c.Character.Class,
                        ImageUrl = c.Character.ImageUrl,
                        Race = c.Character.Race,
                        LastModifiedAt = c.Character.LastModifiedAt,
                        CurrentHp = c.Character.CurrentHp,
                        MaxHp = c.Character.MaxHp,
                        TemporaryHp = c.Character.TemporaryHp
                    } : null
                })
                .ToList(),
            
            // ФІЛЬТРАЦІЯ: Папки бачить тільки GM
            Folders = isGm 
                ? campaign.Folders.Select(f => new CampaignFolderDto { Id = f.Id, Name = f.Name }).ToList() 
                : new List<CampaignFolderDto>()
        };
    }

    public async Task<bool> ApproveJoinRequestAsync(Guid campaignId, Guid targetUserId, Guid gmUserId)
    {
        var campaign = await _campaignRepository.GetByIdWithDetailsAsync(campaignId);
        if (campaign == null || campaign.GmUserId != gmUserId) return false;

        var member = await _campaignRepository.GetMemberAsync(campaignId, targetUserId);
        if (member == null) return false;

        member.Status = CampaignMemberStatus.Approved;
        await _campaignRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RejectJoinRequestAsync(Guid campaignId, Guid targetUserId, Guid gmUserId)
    {
        var campaign = await _campaignRepository.GetByIdWithDetailsAsync(campaignId);
        if (campaign == null || campaign.GmUserId != gmUserId) return false;

        var member = await _campaignRepository.GetMemberAsync(campaignId, targetUserId);
        if (member == null) return false;

        _campaignRepository.RemoveMember(member);
        await _campaignRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AddCharacterToCampaignAsync(
        Guid campaignId,
        AddCampaignCharacterDto dto,
        Guid userId
    )
    {
        var campaign = await _campaignRepository.GetByIdWithDetailsAsync(campaignId);
        if (campaign == null) throw new Exception("Campaign not found.");

        bool isGm = campaign.GmUserId == userId;

        // Якщо це гравець, він мусить бути Approved
        var member = campaign.Members.FirstOrDefault(m => m.UserId == userId);
        if (!isGm && (member == null || member.Status != CampaignMemberStatus.Approved)) 
            throw new Exception("Access Denied: You must be an approved member to add characters.");

        var character = await _characterRepository.GetByIdAsync(dto.CharacterId);
        if (character == null) throw new Exception("Character not found.");
        if (character.UserId != userId && !isGm) throw new Exception("You do not own this character.");

        var existing = await _campaignRepository.GetCampaignCharacterAsync(campaignId, dto.CharacterId);
        if (existing != null)
            throw new Exception("Character is already in the campaign.");

        var campaignCharacter = new CampaignCharacter
        {
            CampaignId = campaignId,
            CharacterId = dto.CharacterId,
            FolderId = isGm ? dto.FolderId : null,
            IsPlayerCharacter = dto.FolderId == null, // Якщо папки немає (Active Party) - це гравець/активний член групи
            IsVisibleToAllPlayers = !isGm // Гравці бачать лише своїх персонажів, поки GM не зробить їх видимими
        };

        await _campaignRepository.AddCharacterAsync(campaignCharacter);
        await _campaignRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveCharacterFromCampaignAsync(
        Guid campaignId,
        Guid characterId,
        Guid userId
    )
    {
        var campaign = await _campaignRepository.GetByIdWithDetailsAsync(campaignId);
        if (campaign == null)
            return false;

        var campaignCharacter = await _campaignRepository.GetCampaignCharacterAsync(
            campaignId,
            characterId
        );
        if (campaignCharacter == null)
            return false;

        _campaignRepository.RemoveCharacter(campaignCharacter);
        await _campaignRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleCharacterGlobalVisibilityAsync(
        Guid campaignId,
        Guid characterId,
        Guid userId,
        bool isVisible
    )
    {
        var campaign = await _campaignRepository.GetByIdWithDetailsAsync(campaignId);
        if (campaign == null || campaign.GmUserId != userId)
            return false; // Тільки Майстер може перемикати видимість

        var campaignCharacter = await _campaignRepository.GetCampaignCharacterAsync(
            campaignId,
            characterId
        );
        if (campaignCharacter == null)
            return false;

        campaignCharacter.IsVisibleToAllPlayers = isVisible;
        await _campaignRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> GrantCharacterAccessAsync(
        Guid campaignId,
        Guid characterId,
        Guid gmUserId,
        Guid targetUserId
    )
    {
        var campaign = await _campaignRepository.GetByIdWithDetailsAsync(campaignId);
        if (campaign == null || campaign.GmUserId != gmUserId)
            return false;

        var campaignCharacter = await _campaignRepository.GetCampaignCharacterAsync(
            campaignId,
            characterId
        );
        if (campaignCharacter == null)
            return false;

        if (!campaignCharacter.VisibleToUserIds.Contains(targetUserId))
        {
            campaignCharacter.VisibleToUserIds.Add(targetUserId);
            await _campaignRepository.SaveChangesAsync();
        }
        return true;
    }

    public async Task<bool> RevokeCharacterAccessAsync(
        Guid campaignId,
        Guid characterId,
        Guid gmUserId,
        Guid targetUserId
    )
    {
        var campaign = await _campaignRepository.GetByIdWithDetailsAsync(campaignId);
        if (campaign == null || campaign.GmUserId != gmUserId)
            return false;

        var campaignCharacter = await _campaignRepository.GetCampaignCharacterAsync(
            campaignId,
            characterId
        );
        if (campaignCharacter == null)
            return false;

        if (campaignCharacter.VisibleToUserIds.Contains(targetUserId))
        {
            campaignCharacter.VisibleToUserIds.Remove(targetUserId);
            await _campaignRepository.SaveChangesAsync();
        }
        return true;
    }

    public async Task<bool> DeleteCampaignAsync(Guid campaignId, Guid userId)
    {
        var campaign = await _campaignRepository.GetByIdWithDetailsAsync(campaignId);
        if (campaign == null || campaign.GmUserId != userId)
            return false; // Тільки GM може видалити кампанію

        _campaignRepository.Remove(campaign);
        await _campaignRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> LeaveCampaignAsync(Guid campaignId, Guid userId)
    {
        var campaign = await _campaignRepository.GetByIdWithDetailsAsync(campaignId);
        if (campaign == null)
            return false;

        // GM не може просто вийти, він мав видалити кампанію
        if (campaign.GmUserId == userId)
            return false;

        var member = campaign.Members.FirstOrDefault(m => m.UserId == userId);
        if (member == null)
            return false; // Користувач не член кампанії

        _campaignRepository.RemoveMember(member);
        await _campaignRepository.SaveChangesAsync();
        return true;
    }

    public async Task<CampaignFolderDto> CreateFolderAsync(Guid campaignId, string folderName, Guid userId)
    {
        var campaign = await _campaignRepository.GetByIdWithDetailsAsync(campaignId);
        if (campaign == null) throw new Exception("Campaign not found.");

        if (campaign.GmUserId != userId)
            throw new Exception("Access Denied: Only the GM can create folders.");

        var folder = new CampaignFolder
        {
            CampaignId = campaignId,
            Name = folderName
        };

        campaign.Folders.Add(folder);
        await _campaignRepository.SaveChangesAsync();

        return new CampaignFolderDto { Id = folder.Id, Name = folder.Name };
    }

    private string GenerateInviteCode()
    {
        return Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(); // Generates an 8-character uppercase code
    }
}
