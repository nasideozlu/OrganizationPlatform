using System.ComponentModel;

namespace OrganizationPlatform.Models;

public class PhysicalFeature
{
    public required string Name {get; set;}
    public required FeatureCategory Category {get; set;}
}