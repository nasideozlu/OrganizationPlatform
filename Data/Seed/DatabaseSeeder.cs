
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

        await SeedBusinessFiltersAsync(context);

        await context.SaveChangesAsync();
    }
    private static async Task SeedBusinessFiltersAsync(
    ApplicationDbContext context)
    {
        var filters = new Dictionary<string, List<FilterSeedData>>
        {
            ["Fotoğrafçı"] = new()
        {
            new("Çekim Türü", "shooting-type",
                BusinessFilterDataType.MultiSelect, null, new[]
                {
                    "Düğün Çekimi",
                    "Dış Çekim",
                    "Save the Date",
                    "Nişan Çekimi",
                    "Kına Çekimi",
                    "Düğün Hikâyesi",
                    "Video Çekimi",
                    "Drone Çekimi"
                })
        },

            ["Müzik"] = new()
        {
            new("Performans Türü", "performance-type",
                BusinessFilterDataType.MultiSelect, null, new[]
                {
                    "DJ",
                    "Orkestra",
                    "Canlı Müzik",
                    "Solo Sanatçı",
                    "Enstrümantal Performans"
                }),

            new("Müzik Türü", "music-type",
                BusinessFilterDataType.MultiSelect, null, new[]
                {
                    "Pop",
                    "Türk Sanat Müziği",
                    "Türk Halk Müziği",
                    "Rock",
                    "Caz",
                    "Klasik Müzik",
                    "Fasıl",
                    "Roman Müziği",
                    "Elektronik Müzik"
                })
        },

            ["Catering"] = new()
        {
            new("Maksimum Davetli", "max-guest-count",
                BusinessFilterDataType.Number, "kişi"),

            new("Özellikler", "features",
                BusinessFilterDataType.MultiSelect, null, new[]
                {
                    "Servis Personeli",
                    "Açık Büfe",
                    "Set Menü",
                    "Kokteyl Menü",
                    "Vegan Menü",
                    "Vejetaryen Menü",
                    "Glutensiz Menü",
                    "Özel Menü Hazırlama"
                })
        },

            ["Davetiye"] = new()
        {
            new("Teslim Süresi", "delivery-time",
                BusinessFilterDataType.Number, "gün"),

            new("Minimum Sipariş Adedi", "minimum-order",
                BusinessFilterDataType.Number, "adet"),

            new("Özellikler", "features",
                BusinessFilterDataType.MultiSelect, null, new[]
                {
                    "Kişiye Özel Tasarım",
                    "Baskılı Davetiye",
                    "Dijital Davetiye",
                    "Lazer Kesim",
                    "Özel Zarf",
                    "Kaligrafi"
                })
        },

            ["Nikah Şekeri ve Hediyelik"] = new()
        {
            new("Minimum Sipariş", "minimum-order",
                BusinessFilterDataType.Number, "adet"),

            new("Teslimat Süresi", "delivery-time",
                BusinessFilterDataType.Number, "gün"),

            new("Ürün Çeşitleri", "product-types",
                BusinessFilterDataType.MultiSelect, null, new[]
                {
                    "Nikâh Şekeri",
                    "Hediyelik",
                    "Nikâh Şekeri ve Hediyelik"
                }),

            new("İçerik", "contents",
                BusinessFilterDataType.MultiSelect, null, new[]
                {
                    "Sabun",
                    "Kolonya",
                    "Mum",
                    "Magnet",
                    "Lavanta",
                    "Çikolata",
                    "Seramik",
                    "Ahşap",
                    "Cam"
                })
        },

            ["Çiçekçi"] = new()
        {
            new("Teslimat Süresi", "delivery-time",
                BusinessFilterDataType.Number, "gün"),

            new("Özellikler", "features",
                BusinessFilterDataType.MultiSelect, null, new[]
                {
                    "Gelin Buketi",
                    "Damat Yaka Çiçeği",
                    "Araç Süsleme",
                    "Masa Çiçekleri",
                    "Mekân Süsleme",
                    "Kişiye Özel Tasarım"
                }),

            new("İçerik", "contents",
                BusinessFilterDataType.MultiSelect, null, new[]
                {
                    "Canlı Çiçek",
                    "Yapay Çiçek",
                    "Kuru Çiçek",
                    "Mevsim Çiçekleri"
                })
        },

            ["Düğün Pastası"] = new()
        {
            new("Maksimum Dilim", "max-slices",
                BusinessFilterDataType.Number, "dilim"),

            new("Özellikler", "features",
                BusinessFilterDataType.MultiSelect, null, new[]
                {
                    "Şeker Hamuru",
                    "Kremalı",
                    "Çikolatalı",
                    "Meyveli",
                    "Glutensiz",
                    "Vegan",
                    "Kişiye Özel Tasarım"
                })
        },

            ["Organizasyon Şirketi"] = new()
        {
            new("Verilen Hizmet", "services",
                BusinessFilterDataType.MultiSelect, null, new[]
                {
                    "Organizasyon Planlama",
                    "Dekorasyon",
                    "Ses ve Işık",
                    "Fotoğraf ve Video",
                    "Catering",
                    "Müzik ve Eğlence",
                    "Gelin Yolu ve Sahne Tasarımı"
                }),

            new("Organizasyon Türleri", "organization-types",
                BusinessFilterDataType.MultiSelect, null, new[]
                {
                    "Düğün",
                    "Nişan",
                    "Kına",
                    "Söz",
                    "Nikâh",
                    "After Party",
                    "Bekârlığa Veda"
                }),

            new("Organizasyon Mekânı", "organization-venue",
                BusinessFilterDataType.MultiSelect, null, new[]
                {
                    "Açık Alan",
                    "Kapalı Alan",
                    "Otel",
                    "Kır Alanı",
                    "Sahil",
                    "Ev"
                }),

            new("Tarz", "style",
                BusinessFilterDataType.MultiSelect, null, new[]
                {
                    "Bohem",
                    "Rustik",
                    "Modern",
                    "Klasik",
                    "Minimalist",
                    "Vintage",
                    "Lüks"
                })
        }
        };

        foreach (var categoryFilters in filters)
        {
            var category = await context.BusinessCategories
                .FirstOrDefaultAsync(c => c.Name == categoryFilters.Key);

            if (category == null)
                continue;

            foreach (var filterData in categoryFilters.Value)
            {
                var definition = await context.BusinessFilterDefinitions
                    .FirstOrDefaultAsync(d =>
                        d.BusinessCategoryId == category.Id &&
                        d.Code == filterData.Code);

                if (definition == null)
                {
                    definition = new BusinessFilterDefinition
                    {
                        Name = filterData.Name,
                        Code = filterData.Code,
                        DataType = filterData.DataType,
                        Unit = filterData.Unit,
                        SortOrder = categoryFilters.Value.IndexOf(filterData) + 1,
                        IsActive = true,
                        IsRequired = false,
                        BusinessCategoryId = category.Id
                    };

                    context.BusinessFilterDefinitions.Add(definition);
                    await context.SaveChangesAsync();
                }

                if (filterData.Options == null)
                    continue;

                foreach (var optionName in filterData.Options)
                {
                    if (!await context.BusinessFilterOptions.AnyAsync(o =>
                        o.BusinessFilterDefinitionId == definition.Id &&
                        o.Value == optionName))
                    {
                        context.BusinessFilterOptions.Add(new BusinessFilterOption
                        {
                            Name = optionName,
                            Value = optionName,
                            SortOrder = Array.IndexOf(filterData.Options, optionName) + 1,
                            IsActive = true,
                            BusinessFilterDefinitionId = definition.Id
                        });
                    }
                }
            }
        }

        await context.SaveChangesAsync();
    }

    private record FilterSeedData(
        string Name,
        string Code,
        BusinessFilterDataType DataType,
        string? Unit,
        string[]? Options = null);
}
