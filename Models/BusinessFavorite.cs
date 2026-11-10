using System.ComponentModel.DataAnnotations;

namespace OrganizationPlatform.Models;

public class BusinessFavorite
{
    public int Id { get; set; }

    [Required]
    public string ApplicationUserId { get; set; } = string.Empty;

    public ApplicationUser ApplicationUser { get; set; } = null!;

    [Required]
    public int BusinessProfileId { get; set; }

    public BusinessProfile BusinessProfile { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}