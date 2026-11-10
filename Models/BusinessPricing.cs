
namespace OrganizationPlatform.Models;

public class BusinessPricing
{
    public int Id { get; set; }

    public required string ServiceName { get; set; }

    public string? Description { get; set; }

    public decimal Amount { get; set; }

    public BusinessPricingType PricingType { get; set; }

    public bool IsActive { get; set; } = true;

    public int BusinessProfileId { get; set; }

    public required BusinessProfile BusinessProfile { get; set; }
}
