namespace OrganizationPlatform.Models;

public class BusinessCategory
{
    public int Id { get; set; }
    public required string Name { get; set; }

    public List<BusinessProfile> BusinessProfiles { get; set; } = new();
    public List<BusinessFilterDefinition> FilterDefinitions { get; set; } = new();
}