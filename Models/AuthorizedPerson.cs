namespace OrganizationPlatform.Models;

public class AuthorizedPerson
{
    public int Id { get; set; }

    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public AuthorizedPersonRole Role { get; set; }

    public string? PhotoPath { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public int? BusinessProfileId { get; set; }

    public BusinessProfile? BusinessProfile { get; set; }

    public int? VenueId { get; set; }

    public Venue? Venue { get; set; }
}