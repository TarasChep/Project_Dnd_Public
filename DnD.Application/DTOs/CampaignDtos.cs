namespace DnD.Application.DTOs;

public class CreateCampaignDto
{
    public string Name { get; set; } = string.Empty;
}

public class CampaignResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid GmUserId { get; set; }
    public string InviteCode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Role { get; set; } = string.Empty;
}

public class CampaignDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid GmUserId { get; set; }
    public string InviteCode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Role { get; set; } = string.Empty;
    public List<CampaignMemberDto> Members { get; set; } = new();
    public List<CampaignCharacterDto> Characters { get; set; } = new();
    public List<CampaignFolderDto> Folders { get; set; } = new();
}

public class CampaignMemberDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class CampaignCharacterDto
{
    public Guid CharacterId { get; set; }
    public string CharacterName { get; set; } = string.Empty;
    public Guid OwnerUserId { get; set; }
    public string OwnerUserName { get; set; } = string.Empty;
    public bool IsVisibleToAllPlayers { get; set; }
    public List<Guid> VisibleToUserIds { get; set; } = new();
    public bool IsPlayerCharacter { get; set; }
    public Guid? FolderId { get; set; }
    public CharacterBriefDto? Character { get; set; }
}

public class CampaignFolderDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class AddCampaignCharacterDto
{
    public Guid CharacterId { get; set; }
    public Guid? FolderId { get; set; }
}