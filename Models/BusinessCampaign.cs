using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace OrganizationPlatform.Models;

public class BusinessCampaign
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public BusinessCampaignType CampaignType { get; set; }

    public DiscountType DiscountType { get; set; }

    public decimal DiscountValue { get; set; }

    public bool IsActive { get; set; } = true;

    // Kampanyanın sahibi olan işletme
    public int BusinessProfileId { get; set; }

    [ValidateNever]
    public BusinessProfile BusinessProfile { get; set; } = null!;

    // Paket bazlı kampanya ise dolu olacak.
    // İşletme geneli kampanya ise null olacak.
    public int? BusinessPackageId { get; set; }

    [ValidateNever]
    public BusinessPackage? BusinessPackage { get; set; }
    public List<Venue> Venues { get; set; } = new();
}