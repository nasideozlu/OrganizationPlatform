using System.ComponentModel.DataAnnotations;

namespace OrganizationPlatform.Models;

public class BusinessRating
{
    public int Id { get; set; }
    [Required]
    public string ApplicationUserId { get; set; } = string.Empty;
    [Required]
    public int BusinessProfileId { get; set; }
    [Range(1, 5)]
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

}