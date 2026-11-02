using Microsoft.EntityFrameworkCore;
using OrganizationPlatform.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace OrganizationPlatform.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Venue> Venues { get; set; }
    public DbSet<PhysicalFeature> PhysicalFeatures { get; set; }
    public DbSet<FeatureCategory> FeatureCategories { get; set; }
    public DbSet<CustomerProfile> CustomerProfiles { get; set; }
    public DbSet<BusinessCategory> BusinessCategories { get; set; }
    public DbSet<BusinessProfile> BusinessProfiles { get; set; }

    public DbSet<BusinessPhoto> BusinessPhotos { get; set; }
        = null!;

    public DbSet<VenuePhoto> VenuePhotos { get; set; }
        = null!;

    public DbSet<BusinessPricing> BusinessPricings { get; set; }
        = null!;

    public DbSet<BusinessPackage> BusinessPackages { get; set; }
        = null!;

    public DbSet<BusinessPackageItem> BusinessPackageItems { get; set; }
        = null!;

    public DbSet<BusinessCampaign> BusinessCampaigns { get; set; }
        = null!;

    public DbSet<BusinessFavorite> BusinessFavorites { get; set; } = null!;


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CustomerProfile>()
            .HasIndex(customer => customer.ApplicationUserId)
            .IsUnique();

        modelBuilder.Entity<BusinessProfile>()
            .HasIndex(business => business.ApplicationUserId)
            .IsUnique();


        modelBuilder.Entity<BusinessPricing>()
            .Property(pricing => pricing.Amount)
            .HasPrecision(12, 2);


        modelBuilder.Entity<BusinessPackage>()
            .Property(package => package.Amount)
            .HasPrecision(12, 2);


        modelBuilder.Entity<BusinessPackage>()
            .HasMany(package => package.Items)
            .WithOne(item => item.BusinessPackage)
            .HasForeignKey(item => item.BusinessPackageId)
            .OnDelete(DeleteBehavior.Cascade);


        modelBuilder.Entity<BusinessCampaign>()
            .Property(campaign => campaign.DiscountValue)
            .HasPrecision(12, 2);


        modelBuilder.Entity<BusinessCampaign>()
            .HasOne(campaign => campaign.BusinessProfile)
            .WithMany(business => business.Campaigns)
            .HasForeignKey(campaign => campaign.BusinessProfileId)
            .OnDelete(DeleteBehavior.Cascade);


        modelBuilder.Entity<BusinessCampaign>()
            .HasOne(campaign => campaign.BusinessPackage)
            .WithMany(package => package.Campaigns)
            .HasForeignKey(campaign => campaign.BusinessPackageId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<BusinessCampaign>()
            .Property(campaign => campaign.StartDate)
            .HasColumnType("date");

        modelBuilder.Entity<BusinessCampaign>()
            .Property(campaign => campaign.EndDate)
            .HasColumnType("date");

        modelBuilder.Entity<BusinessFavorite>()
    .HasOne(favorite => favorite.ApplicationUser)
    .WithMany()
    .HasForeignKey(favorite => favorite.ApplicationUserId)
    .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BusinessFavorite>()
            .HasOne(favorite => favorite.BusinessProfile)
            .WithMany()
            .HasForeignKey(favorite => favorite.BusinessProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BusinessFavorite>()
            .HasIndex(favorite => new
            {
                favorite.ApplicationUserId,
                favorite.BusinessProfileId
            })
            .IsUnique();

    }
}