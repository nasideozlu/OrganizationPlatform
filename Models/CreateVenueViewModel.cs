using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace OrganizationPlatform.Models;

public class CreateVenueViewModel
{
    [Required]
    public string Name { get; set; } = string.Empty;
    [Required]
    public string City { get; set; } = string.Empty;
    [Required]
    public string District { get; set; } = string.Empty;

    [Required]
    public string Neighborhood { get; set; } = string.Empty;

    public string? Address { get; set; }
    [Required]
    public int Capacity { get; set; }
    [Required]
    public string? Description { get; set; }
    [Range(0.01, 100000000)]
    public decimal? MealIncludedPrice { get; set; }

    [Range(0.01, 100000000)]
    public decimal? MealExcludedPrice { get; set; }

    public List<int> SelectedPhysicalFeatureIds { get; set; } = new();
    public List<IFormFile> Photos { get; set; } = new();
    public List<VenuePhoto> ExistingPhotos { get; set; } = new();

}