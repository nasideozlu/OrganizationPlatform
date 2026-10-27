
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace OrganizationPlatform.Models;

public class BusinessProfileEditViewModel
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public string City { get; set; } = string.Empty;
    [Required]
    public string District { get; set; } = string.Empty;

    public List<IFormFile> Photos { get; set; } = new();
    public List<BusinessPhoto> ExistingPhotos { get; set; } = new();

}
