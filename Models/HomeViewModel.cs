namespace OrganizationPlatform.Models;

public class HomeViewModel
{
    public List<Venue> Venues { get; set; } = new();

    public List<BusinessProfile> Businesses { get; set; } = new();
}