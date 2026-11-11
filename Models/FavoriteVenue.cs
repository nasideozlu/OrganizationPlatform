using System.ComponentModel.DataAnnotations;

namespace OrganizationPlatform.Models;

public class FavoriteVenue
{
    public int Id { get; set; }

    [Required]
    public string ApplicationUserId { get; set; } = string.Empty;

    public ApplicationUser ApplicationUser { get; set; } = null!;

    [Required]
    public int VenueId { get; set; }

    public Venue Venue { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}