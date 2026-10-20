namespace OrganizationPlatform.Models;

public class FeatureCategory
{
    public int Id {get; set;}
    public required string Name {get; set;}

    public List<PhysicalFeature> PhysicalFeatures {get; set;} = new();

}
