namespace OrganizationPlatform.ViewModels;

public class BusinessRatingViewModel
{
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CustomerName { get; set; } = string.Empty;
}