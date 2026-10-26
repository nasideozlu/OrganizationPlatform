using System.ComponentModel.DataAnnotations;

namespace OrganizationPlatform.Models;

public class CreateVenueViewModel
{
    [Required]
    public string Name { get; set; } = string.Empty;
    [Required]
    public string City { get; set; } = string.Empty;
    [Required]
    public int Capacity { get; set; }
    [Required]
    public string Address { get; set; } = string.Empty;
    public string? Description { get; set; }
    [Range(0.01, 100000000)]
    public decimal? MealIncludedPrice { get; set; }

    [Range(0.01, 100000000)]
    public decimal? MealExcludedPrice { get; set; }

    public List<int> SelectedPhysicalFeatureIds { get; set; } = new();

}