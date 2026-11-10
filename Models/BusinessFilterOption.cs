using System.ComponentModel.DataAnnotations;

namespace OrganizationPlatform.Models;

public class BusinessFilterOption
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Value { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public int BusinessFilterDefinitionId { get; set; }
    public bool IsActive { get; set; } = true;

    public BusinessFilterDefinition Definition { get; set; } = null!;

    public List<BusinessProfileFilterOption> BusinessProfileOptions { get; set; } = new();
}