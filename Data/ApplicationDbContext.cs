using Microsoft.EntityFrameworkCore;
using OrganizationPlatform.Models;

namespace OrganizationPlatform.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Venue> Venues {get; set;}
    public DbSet<PhysicalFeature> PhysicalFeatures {get; set;}
    public DbSet<FeatureCategory> FeatureCategories {get; set;}
}
