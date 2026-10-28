
namespace OrganizationPlatform.Models;

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

    public List<int> SelectedFeatureIds { get; set; } = new();

    public List<Venue> Venues { get; set; } = new();

    public List<string> Cities { get; set; } = new();

    public List<string> Districts { get; set; } = new();

    public List<FeatureCategory> FeatureCategories { get; set; } = new();
}
