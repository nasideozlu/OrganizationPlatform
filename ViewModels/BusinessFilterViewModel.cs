using OrganizationPlatform.Models;
using OrganizationPlatform.Services;

namespace OrganizationPlatform.ViewModels;


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
    public List<BusinessFilterSearchItemViewModel> FilterItems { get; set; } = new();
}

public class BusinessFilterSearchItemViewModel
{
    public int DefinitionId { get; set; }

    public string Name { get; set; } = string.Empty;

    public BusinessFilterDataType DataType { get; set; }

    public string? Unit { get; set; }

    public List<BusinessFilterOption> Options { get; set; } = new();

    public List<int> SelectedOptionIds { get; set; } = new();

    public decimal? NumericValue { get; set; }

    public string? TextValue { get; set; }

    public bool? BooleanValue { get; set; }
}
