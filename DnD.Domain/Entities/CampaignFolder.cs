namespace DnD.Domain.Entities;

public class CampaignFolder : BaseEntity
{
    public Guid CampaignId { get; set; }
    public Campaign Campaign { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public ICollection<CampaignCharacter> Characters { get; set; } = new List<CampaignCharacter>();
}