namespace OrganizationPlatform.Models;

public class BusinessProfile
{
    public int Id {get; set;}
    public required string Name {get; set;}
    public string? Description {get; set;}
    public required string City {get; set;}
    public int CategoryId {get; set;}
    public required string ApplicationUserId {get; set;}

    public required BusinessCategory Category {get; set;}
    public required ApplicationUser ApplicationUser {get; set;}

    public List<Venue> Venues { get; set; } = new List<Venue>();

}