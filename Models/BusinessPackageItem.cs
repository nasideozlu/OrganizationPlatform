
namespace OrganizationPlatform.Models;

public class BusinessPackageItem
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public int BusinessPackageId { get; set; }

    public required BusinessPackage BusinessPackage { get; set; }
}
