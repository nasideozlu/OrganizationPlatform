namespace OrganizationPlatform.Models;

public class BusinessProfileFilterOption
{
    public int Id { get; set; }

    public int BusinessProfileId { get; set; }

    public BusinessProfile BusinessProfile { get; set; } = null!;

    public int BusinessFilterOptionId { get; set; }

    public BusinessFilterOption FilterOption { get; set; } = null!;
}