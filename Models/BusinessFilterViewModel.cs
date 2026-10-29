
namespace OrganizationPlatform.Models;
using OrganizationPlatform.Services;

public class BusinessFilterViewModel
{
    public string? SearchTerm { get; set; }

    public int? CategoryId { get; set; }

    public int? ProvinceId { get; set; }

    public string? City { get; set; }

    public string? District { get; set; }

    public List<BusinessProfile> Businesses { get; set; } = new();

    public List<BusinessCategory> Categories { get; set; } = new();

    public List<LocationItem> Provinces { get; set; } = new();

    public List<LocationItem> Districts { get; set; } = new();
}
