namespace OrganizationPlatform.Models;

public class Venue
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string City { get; set; }
    public required int Capacity { get; set; }
    public string? Description { get; set; }
    public string? Address { get; set; }
    public decimal? MealIncludedPrice { get; set; }
    public decimal? MealExcludedPrice { get; set; }

    public int BusinessProfileId { get; set; }
    public required BusinessProfile BusinessProfile { get; set; }

    public List<PhysicalFeature> PhysicalFeatures { get; set; } = new List<PhysicalFeature>();

}