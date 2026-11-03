using System.ComponentModel.DataAnnotations;

namespace OrganizationPlatform.Models;

public class BusinessFilterDefinition
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Code { get; set; } = string.Empty;

    public BusinessFilterDataType DataType { get; set; }

    public string? Unit { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public int BusinessCategoryId { get; set; }
    public bool IsRequired { get; set; } = false;

    public BusinessCategory BusinessCategory { get; set; } = null!;

    public List<BusinessFilterOption> Options { get; set; } = new();

    public List<BusinessProfileFilterValue> ProfileValues { get; set; } = new();
}