namespace OrganizationPlatform.Models;

public class CustomerProfile
{
    public int Id {get; set;}
    public required string Name {get; set;}
    public required string Surname {get; set;}
    public required string ApplicationUserId {get; set;}

    public required ApplicationUser ApplicationUser {get; set;}

}