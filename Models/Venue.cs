namespace OrganizationPlatform.Models;

public class Venue
{
    public required string Name {get; set;} 
    public required string City {get; set;}
    public required int Capacity {get; set;}
    public string? Description {get; set;}
    public string? Address {get; set;}
    public List<string> PhysicalFeatures {get; set;} = new List<string> ();
    
}

