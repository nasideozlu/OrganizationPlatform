
namespace OrganizationPlatform.Models;

public class BusinessPackage
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public decimal Amount { get; set; }

    public bool IsActive { get; set; } = true;

    public int BusinessProfileId { get; set; }

    public required BusinessProfile BusinessProfile { get; set; }

    public List<BusinessPackageItem> Items { get; set; } = new();
    public List<BusinessCampaign> Campaigns { get; set; } = new();
}
