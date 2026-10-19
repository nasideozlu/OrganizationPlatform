using Microsoft.EntityFrameworkCore;

namespace OrganizationPlatform.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
}
