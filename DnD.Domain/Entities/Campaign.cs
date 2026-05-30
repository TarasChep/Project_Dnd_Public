namespace DnD.Domain.Entities;

public class Campaign : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    
    public Guid GmUserId { get; set; }
    public User GM { get; set; } = null!;

    public string InviteCode { get; set; } = string.Empty;

    // Навігаційні властивості
    public ICollection<CampaignMember> Members { get; set; } = new List<CampaignMember>();
    public ICollection<CampaignFolder> Folders { get; set; } = new List<CampaignFolder>();
    public ICollection<CampaignCharacter> Characters { get; set; } = new List<CampaignCharacter>();
    public ICollection<Encounter> Encounters { get; set; } = new List<Encounter>();
}