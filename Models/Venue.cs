namespace OrganizationPlatform.Models;

public class Venue
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string City { get; set; }
    public required string District { get; set; }
    public required string Neighborhood { get; set; }
    public string? Address { get; set; }
    public required int Capacity { get; set; }
    public string? Description { get; set; }
    public string? PhoneNumber { get; set; }
    public string? InstagramUrl { get; set; }
    public string? Email { get; set; }
    public decimal? MealIncludedPrice { get; set; }
    public decimal? MealExcludedPrice { get; set; }

    public int BusinessProfileId { get; set; }
    
    public required BusinessProfile BusinessProfile { get; set; }
    public ReviewStatus ApprovalStatus { get; set; } = ReviewStatus.Pending;
    public string? RejectionReason { get; set; }
    public List<AuthorizedPerson> AuthorizedPeople { get; set; } = new();

    public List<PhysicalFeature> PhysicalFeatures { get; set; } = new List<PhysicalFeature>();
    public List<VenuePhoto> Photos { get; set; } = new();
    public List<BusinessPackage> BusinessPackages { get; set; } = new();
    public List<BusinessCampaign> BusinessCampaigns { get; set; } = new();
}