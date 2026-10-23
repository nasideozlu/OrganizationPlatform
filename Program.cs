using Microsoft.EntityFrameworkCore;
using OrganizationPlatform.Data;
using Microsoft.AspNetCore.Identity;
using OrganizationPlatform.Models;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    if (!await context.BusinessCategories.AnyAsync())
    {
        context.BusinessCategories.AddRange(
            new BusinessCategory { Name = "Düğün Salonu" },
            new BusinessCategory { Name = "Fotoğrafçı" },
            new BusinessCategory { Name = "Organizasyon Şirketi" },
            new BusinessCategory { Name = "Catering" },
            new BusinessCategory { Name = "Çiçekçi" },
            new BusinessCategory { Name = "Müzik ve Eğlence" }
        );

        await context.SaveChangesAsync();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
