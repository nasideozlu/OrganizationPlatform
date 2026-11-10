namespace OrganizationPlatform.Models;

public class BusinessProfileFilterValue
{
    public int Id { get; set; }

    public int BusinessProfileId { get; set; }

    public BusinessProfile BusinessProfile { get; set; } = null!;

    public int BusinessFilterDefinitionId { get; set; }

    public BusinessFilterDefinition Definition { get; set; } = null!;

    public decimal? NumericValue { get; set; }

    public string? TextValue { get; set; }

    public bool? BooleanValue { get; set; }
}