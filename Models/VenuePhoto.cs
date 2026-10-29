
namespace OrganizationPlatform.Models;

public class VenuePhoto
{
    public int Id { get; set; }

    public required string FilePath { get; set; }

    public int VenueId { get; set; }

    public required Venue Venue { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
