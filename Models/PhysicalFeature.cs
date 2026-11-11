using System.ComponentModel;

namespace OrganizationPlatform.Models;

public class PhysicalFeature
{
    public int Id {get; set;}
    public required string Name {get; set;}
    public int CategoryId {get; set;}
    public required FeatureCategory Category {get; set;}

    public List<Venue> Venues {get; set;} = new();
}
