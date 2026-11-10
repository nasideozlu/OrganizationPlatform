namespace OrganizationPlatform.ViewModels;

public class AdminReviewViewModel
{
    public List<AdminReviewItemViewModel> BusinessReviews { get; set; } = new();

    public List<AdminReviewItemViewModel> VenueReviews { get; set; } = new();
}

public class AdminReviewItemViewModel
{
    public int Id { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public string TargetName { get; set; } = string.Empty;
}