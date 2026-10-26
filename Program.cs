using Microsoft.EntityFrameworkCore;
using OrganizationPlatform.Data;
using Microsoft.AspNetCore.Identity;
using OrganizationPlatform.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;


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

builder.Services.Configure<IdentityOptions>(options =>
{
    options.ClaimsIdentity.RoleClaimType = System.Security.Claims.ClaimTypes.Role;
});

var jwtKey = builder.Configuration["Jwt:SigningKey"]
?? throw new InvalidOperationException("JWT signing key is not configured.");

var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;

builder.Services
    .AddAuthentication()
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)),

            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            ValidateAudience = true,
            ValidAudience = jwtAudience,

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

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

    // Fiziksel özellik kategorilerini oluştur.
    var featureCategories = new[]
    {
        "Manzara",
        "Fiziksel Özellikler",
        "Olanaklar",
        "Erişilebilirlik"
    };

    foreach (var categoryName in featureCategories)
    {
        if (!await context.FeatureCategories
            .AnyAsync(category => category.Name == categoryName))
        {
            context.FeatureCategories.Add(
                new FeatureCategory { Name = categoryName });
        }
    }

    await context.SaveChangesAsync();

    // Kategorileri ve özellikleri tanımla.
    var physicalFeatures = new Dictionary<string, string[]>
    {
        ["Manzara"] = new[]
        {
            "Deniz manzarası",
            "Doğa manzarası",
            "Şehir manzarası",
            "Göl manzarası"
        },
        ["Fiziksel Özellikler"] = new[]
        {
            "Açık alan",
            "Kapalı alan",
            "Bahçe",
            "Teras",
            "Havuz",
            "Sahne"
        },
        ["Olanaklar"] = new[]
        {
            "Otopark",
            "Vale hizmeti",
            "Klima",
            "Ses sistemi",
            "Jeneratör",
            "Gelin odası"
        },
        ["Erişilebilirlik"] = new[]
        {
            "Engelli erişimi"
        }
    };

    foreach (var item in physicalFeatures)
    {
        var category = await context.FeatureCategories
            .FirstAsync(category => category.Name == item.Key);

        foreach (var featureName in item.Value)
        {
            if (!await context.PhysicalFeatures.AnyAsync(feature =>
                feature.Name == featureName &&
                feature.CategoryId == category.Id))
            {
                context.PhysicalFeatures.Add(new PhysicalFeature
                {
                    Name = featureName,
                    CategoryId = category.Id,
                    Category = category
                });
            }
        }
    }

    await context.SaveChangesAsync();

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
