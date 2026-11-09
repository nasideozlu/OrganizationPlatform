
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
    public string? PhoneNumber { get; set; }

    [Url(ErrorMessage = "Geçerli bir Instagram bağlantısı girin.")]
    public string? InstagramUrl { get; set; }

    public string? Address { get; set; }

    public List<IFormFile>? Photos { get; set; }
    public List<BusinessPhoto> ExistingPhotos { get; set; } = new();
    public List<BusinessProfileFilterEditItemViewModel> FilterItems { get; set; } = new();

}

public class BusinessProfileFilterEditItemViewModel
{
    public int DefinitionId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public BusinessFilterDataType DataType { get; set; }

    public string? Unit { get; set; }

    public bool IsRequired { get; set; }

    public List<BusinessFilterOption> Options { get; set; } = new();

    public List<int> SelectedOptionIds { get; set; } = new();

    public decimal? NumericValue { get; set; }

    public string? TextValue { get; set; }

    public bool? BooleanValue { get; set; }
}
