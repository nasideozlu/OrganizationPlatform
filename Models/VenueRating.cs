using System.ComponentModel.DataAnnotations;

namespace OrganizationPlatform.Models;

public class VenueRating
{
    public int Id { get; set; }

    [Required]
    public string ApplicationUserId { get; set; } = string.Empty;

    [Required]
    public int VenueId { get; set; }
    public Venue Venue { get; set; } = null!;

    [Range(1, 5)]
    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ReviewStatus ReviewStatus { get; set; } = ReviewStatus.Approved;

    public string? BusinessReply { get; set; }

    public DateTime? BusinessReplyAt { get; set; }
}