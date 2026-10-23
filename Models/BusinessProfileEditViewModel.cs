
using System.ComponentModel.DataAnnotations;

namespace OrganizationPlatform.Models;

public class BusinessProfileEditViewModel
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public string City { get; set; } = string.Empty;

    [Required]
    public int CategoryId { get; set; }

    public List<BusinessCategory> Categories { get; set; } = new();
}
