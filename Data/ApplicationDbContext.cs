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


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CustomerProfile>()
            .HasIndex(customer => customer.ApplicationUserId)
            .IsUnique();


        modelBuilder.Entity<BusinessProfile>()
            .HasIndex(business => business.ApplicationUserId)
            .IsUnique();

    }
}
