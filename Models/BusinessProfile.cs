namespace OrganizationPlatform.Models;

public class BusinessProfile
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public required string City { get; set; }

    public required string District { get; set; }
    public string? PhoneNumber { get; set; }
    public string? InstagramUrl { get; set; }
    public string? Address { get; set; }

    public int CategoryId { get; set; }

    public required string ApplicationUserId { get; set; }

    public required BusinessCategory Category { get; set; }

    public required ApplicationUser ApplicationUser { get; set; }

    public ReviewStatus ApprovalStatus { get; set; } = ReviewStatus.Pending;
    public string? RejectionReason { get; set; }
    public List<Venue> Venues { get; set; } = new();
    public List<AuthorizedPerson> AuthorizedPeople { get; set; } = new();

    public List<BusinessPhoto> Photos { get; set; } = new();

    public List<BusinessPricing> Pricings { get; set; } = new();

    public List<BusinessPackage> Packages { get; set; } = new();

    public List<BusinessCampaign> Campaigns { get; set; } = new();
    public List<BusinessProfileFilterOption> FilterOptions { get; set; } = new();

    public List<BusinessProfileFilterValue> FilterValues { get; set; } = new();
}