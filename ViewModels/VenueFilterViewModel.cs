using OrganizationPlatform.Services;
using OrganizationPlatform.Models;

namespace OrganizationPlatform.ViewModels;

public class VenueFilterViewModel
{
    public string? City { get; set; }

    public string? District { get; set; }

    public decimal? MinPrice { get; set; }

    public decimal? MaxPrice { get; set; }

    public int? MinCapacity { get; set; }

    public string? PriceType { get; set; }
    public string? SortBy { get; set; }
    public string? SearchTerm { get; set; }
    public int? ProvinceId { get; set; }

    public List<int> SelectedFeatureIds { get; set; } = new();

    public List<Venue> Venues { get; set; } = new();

    public List<FeatureCategory> FeatureCategories { get; set; } = new();
    public List<LocationItem> Provinces { get; set; } = new();

    public List<LocationItem> DistrictOptions { get; set; } = new();
    public List<int> FavoriteVenueIds { get; set; } = new();
}
