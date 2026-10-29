
using Microsoft.EntityFrameworkCore;
using OrganizationPlatform.Models;
using Microsoft.AspNetCore.Identity;

namespace OrganizationPlatform.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext context,
        RoleManager<IdentityRole> roleManager)
    {
        // 0. Yönetici rolü
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }
        // 1. İşletme kategorileri
        // Eski kategori adlarını güncelle
        var oldVenueCategory = await context.BusinessCategories
            .FirstOrDefaultAsync(category => category.Name == "Düğün Salonu");

        if (oldVenueCategory != null)
        {
            oldVenueCategory.Name = "Düğün Mekanı";
        }

        var oldMusicCategory = await context.BusinessCategories
            .FirstOrDefaultAsync(category => category.Name == "Müzik ve Eğlence");

        if (oldMusicCategory != null)
        {
            oldMusicCategory.Name = "Müzik";
        }

        string[] businessCategories =
        {
    "Düğün Mekanı",
    "Fotoğrafçı",
    "Organizasyon Şirketi",
    "Catering",
    "Nikah Şekeri ve Hediyelik",
    "Çiçekçi",
    "Düğün Pastası",
    "Müzik",
    "Davetiye"
};

        // İşletme kategorilerini yalnızca ilk kurulumda oluştur.
        if (!await context.BusinessCategories.AnyAsync())
        {
            foreach (var name in businessCategories)
            {
                context.BusinessCategories.Add(
                    new BusinessCategory { Name = name });
            }
        }


        // 2. Fiziksel özellik grupları
        string[] featureCategories =
        {
            "Manzara",
            "Fiziksel Özellikler",
            "Olanaklar",
            "Erişilebilirlik"
        };

        foreach (var name in featureCategories)
        {
            if (!await context.FeatureCategories
                .AnyAsync(category => category.Name == name))
            {
                context.FeatureCategories.Add(
                    new FeatureCategory { Name = name });
            }
        }

        await context.SaveChangesAsync();

        // 3. Mekanların fiziksel özellikleri
        var physicalFeatures = new Dictionary<string, string[]>
        {
            ["Manzara"] = new[]
            {
                "Deniz manzarası",
                "Doğa manzarası",
                "Şehir manzarası",
                "Göl manzarası",
                "Boğaz Manzarası"
            },

            ["Fiziksel Özellikler"] = new[]
            {
                "Açık alan",
                "Kapalı alan",
                "Bahçe",
                "Teras",
                "Havuz",
                "Sahne",
                "Kolonsuz Yapı",
                "Ses Yalıtımı",
                "Giriş ve Karşılama Alanı"
            },

            ["Olanaklar"] = new[]
            {
                "Otopark",
                "Vale hizmeti",
                "Klima ve Havalandırma Sistemi",
                "Canlı Müzik / DJ Performans",
                "Jeneratör Sistemi",
                "Hazırlık Odası",
                "Yiyecek ve İçecek Hizmeti",
                "Gelişmiş Ses, Işık ve Görüntü Sistemleri",
                "Organizasyon ve Dekorasyon Yönetimi",
                "Fotoğraf ve Video Ekibi",
                "Çocuk Oyun Alanı ve Bakıcı"



            },

            ["Erişilebilirlik"] = new[]
            {
                "Engelli erişimi",
                "Asansör"

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
}
