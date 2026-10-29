
namespace OrganizationPlatform.Models;

public class BusinessPhoto
{
    public int Id { get; set; }

    public required string FilePath { get; set; }

    public int BusinessProfileId { get; set; }

    public required BusinessProfile BusinessProfile { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
