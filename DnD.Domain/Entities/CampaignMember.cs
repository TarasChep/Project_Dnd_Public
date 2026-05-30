using DnD.Domain.Enums;

namespace DnD.Domain.Entities;

public class CampaignMember
{
    public Guid CampaignId { get; set; }
    public Campaign Campaign { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public CampaignRole Role { get; set; }
    public CampaignMemberStatus Status { get; set; }
}