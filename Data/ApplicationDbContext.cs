using Microsoft.EntityFrameworkCore;
using OrganizationPlatform.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using OrganizationPlatform.Helpers;

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

    public DbSet<BusinessFavorite> BusinessFavorites { get; set; }
    = null!;

    public DbSet<BusinessRating> BusinessRatings { get; set; }
    = null!;

    public DbSet<FavoriteVenue> FavoriteVenues { get; set; }

    public DbSet<BusinessFilterDefinition> BusinessFilterDefinitions { get; set; } = null!;

    public DbSet<BusinessFilterOption> BusinessFilterOptions { get; set; } = null!;

    public DbSet<BusinessProfileFilterOption> BusinessProfileFilterOptions { get; set; } = null!;

    public DbSet<BusinessProfileFilterValue> BusinessProfileFilterValues { get; set; } = null!;

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

        modelBuilder.Entity<BusinessRating>()
            .HasIndex(rating => new
            {
                rating.ApplicationUserId,
                rating.BusinessProfileId
            })
            .IsUnique();

        modelBuilder.Entity<BusinessPackage>()
            .HasMany(package => package.Venues)
            .WithMany(venue => venue.BusinessPackages)
            .UsingEntity(j => j.ToTable("BusinessPackageVenues"));

        modelBuilder.Entity<BusinessCampaign>()
            .HasMany(campaign => campaign.Venues)
            .WithMany(venue => venue.BusinessCampaigns)
            .UsingEntity(j => j.ToTable("BusinessCampaignVenues"));

        modelBuilder.Entity<FavoriteVenue>()
            .HasOne(favorite => favorite.ApplicationUser)
            .WithMany()
            .HasForeignKey(favorite => favorite.ApplicationUserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<FavoriteVenue>()
            .HasOne(favorite => favorite.Venue)
            .WithMany()
            .HasForeignKey(favorite => favorite.VenueId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<FavoriteVenue>()
            .HasIndex(favorite => new
            {
                favorite.ApplicationUserId,
                favorite.VenueId
            })
            .IsUnique();

        // Kategori - Filtre tanımı
        modelBuilder.Entity<BusinessFilterDefinition>()
            .HasOne(definition => definition.BusinessCategory)
            .WithMany(category => category.FilterDefinitions)
            .HasForeignKey(definition => definition.BusinessCategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Filtre tanımı - Seçenekler
        modelBuilder.Entity<BusinessFilterOption>()
            .HasOne(option => option.Definition)
            .WithMany(definition => definition.Options)
            .HasForeignKey(option => option.BusinessFilterDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);

        // İşletme - Filtre seçenekleri
        modelBuilder.Entity<BusinessProfileFilterOption>()
            .HasOne(profileOption => profileOption.BusinessProfile)
            .WithMany(profile => profile.FilterOptions)
            .HasForeignKey(profileOption => profileOption.BusinessProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BusinessProfileFilterOption>()
            .HasOne(profileOption => profileOption.FilterOption)
            .WithMany(option => option.BusinessProfileOptions)
            .HasForeignKey(profileOption => profileOption.BusinessFilterOptionId)
            .OnDelete(DeleteBehavior.Cascade);

        // İşletme - Filtre değerleri
        modelBuilder.Entity<BusinessProfileFilterValue>()
            .HasOne(value => value.BusinessProfile)
            .WithMany(profile => profile.FilterValues)
            .HasForeignKey(value => value.BusinessProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BusinessProfileFilterValue>()
            .HasOne(value => value.Definition)
            .WithMany(definition => definition.ProfileValues)
            .HasForeignKey(value => value.BusinessFilterDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Benzersiz indeksler
        modelBuilder.Entity<BusinessFilterDefinition>()
            .HasIndex(definition => new
            {
                definition.BusinessCategoryId,
                definition.Code
            })
            .IsUnique();

        modelBuilder.Entity<BusinessFilterOption>()
            .HasIndex(option => new
            {
                option.BusinessFilterDefinitionId,
                option.Value
            })
            .IsUnique();

        modelBuilder.Entity<BusinessProfileFilterOption>()
            .HasIndex(profileOption => new
            {
                profileOption.BusinessProfileId,
                profileOption.BusinessFilterOptionId
            })
            .IsUnique();

        modelBuilder.Entity<BusinessProfileFilterValue>()
            .HasIndex(value => new
            {
                value.BusinessProfileId,
                value.BusinessFilterDefinitionId
            })
            .IsUnique();

        modelBuilder
            .HasDbFunction(typeof(PostgreSqlFunctions)
                .GetMethod(nameof(PostgreSqlFunctions.Unaccent))!)
            .HasName("unaccent")
            .HasSchema("public");

    }
}