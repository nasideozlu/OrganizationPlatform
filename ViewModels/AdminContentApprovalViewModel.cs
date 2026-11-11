namespace OrganizationPlatform.ViewModels;

public class AdminContentApprovalViewModel
{
    public List<BusinessApprovalItemViewModel> Businesses { get; set; } = new();

    public List<BusinessPhotoApprovalItemViewModel> BusinessPhotos { get; set; } = new();
    public List<VenueApprovalItemViewModel> Venues { get; set; } = new();
    public List<VenuePhotoApprovalItemViewModel> VenuePhotos { get; set; } = new();
}

public class BusinessApprovalItemViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string CategoryName { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string? District { get; set; }
}

public class BusinessPhotoApprovalItemViewModel
{
    public int Id { get; set; }

    public string FilePath { get; set; } = string.Empty;

    public string BusinessName { get; set; } = string.Empty;

    public int BusinessProfileId { get; set; }
}

public class VenueApprovalItemViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string BusinessName { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string? District { get; set; }

    public int Capacity { get; set; }
}

public class VenuePhotoApprovalItemViewModel
{
    public int Id { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string VenueName { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public int VenueId { get; set; }
}