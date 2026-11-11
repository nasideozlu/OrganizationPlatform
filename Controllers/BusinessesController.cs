
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrganizationPlatform.Data;
using OrganizationPlatform.Models;
using OrganizationPlatform.Services;
using System.Security.Claims;
using OrganizationPlatform.ViewModels;
using OrganizationPlatform.Helpers;

namespace OrganizationPlatform.Controllers;

public class BusinessesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly LocationService _locationService;

    public BusinessesController(
        ApplicationDbContext context,
        LocationService locationService)
    {
        _context = context;
        _locationService = locationService;
    }



    public async Task<IActionResult> Index(BusinessFilterViewModel filter)
    {
        // İlleri konum API'sinden al.
        filter.Provinces = await _locationService.GetProvincesAsync();

        // Seçilen ilin adını bul.
        var selectedProvince = filter.ProvinceId.HasValue
            ? filter.Provinces.FirstOrDefault(
                province => province.Id == filter.ProvinceId.Value)
            : null;

        filter.City = selectedProvince?.Name;

        // Seçilen ile ait ilçeleri al.
        if (filter.ProvinceId.HasValue)
        {
            filter.Districts = await _locationService
                .GetDistrictsAsync(filter.ProvinceId.Value);
        }

        // Kategorileri getir.
        filter.Categories = await _context.BusinessCategories
            .AsNoTracking()
            .Where(category => category.Name != "Düğün Mekanı")
            .OrderBy(category => category.Name)
            .ToListAsync();

        // Seçilen kategoriye ait aktif filtreleri getir.
        var definitions = new List<BusinessFilterDefinition>();

        if (filter.CategoryId.HasValue)
        {
            definitions = await _context.BusinessFilterDefinitions
                .AsNoTracking()
                .Include(definition => definition.Options)
                .Where(definition =>
                    definition.BusinessCategoryId == filter.CategoryId.Value &&
                    definition.IsActive)
                .OrderBy(definition => definition.SortOrder)
                .ToListAsync();
        }

        // Formdan gelen filtre seçimlerini al.
        var selectedItems = filter.FilterItems
            ?? new List<BusinessFilterSearchItemViewModel>();

        // Yalnızca seçilen kategoriye ait filtreleri ViewModel'e aktar.
        filter.FilterItems = definitions.Select(definition =>
        {
            var selected = selectedItems
                .FirstOrDefault(item => item.DefinitionId == definition.Id);

            var activeOptions = definition.Options
                .Where(option => option.IsActive)
                .OrderBy(option => option.SortOrder)
                .ToList();

            var validOptionIds = activeOptions
                .Select(option => option.Id)
                .ToHashSet();

            return new BusinessFilterSearchItemViewModel
            {
                DefinitionId = definition.Id,
                Name = definition.Name,
                DataType = definition.DataType,
                Unit = definition.Unit,
                Options = activeOptions,

                SelectedOptionIds = selected?.SelectedOptionIds?
                    .Where(id => validOptionIds.Contains(id))
                    .Distinct()
                    .ToList() ?? new List<int>(),

                NumericValue = selected?.NumericValue,
                TextValue = selected?.TextValue,
                BooleanValue = selected?.BooleanValue
            };
        }).ToList();

        // İşletme sorgusu.
        var query = _context.BusinessProfiles
            .AsNoTracking()
            .Include(business => business.Category)
            .Include(business => business.Photos
                .Where(photo => photo.ApprovalStatus == ReviewStatus.Approved))
            .Where(business => business.Category.Name != "Düğün Mekanı")
            .Where(business => business.ApprovalStatus == ReviewStatus.Approved)
            .AsQueryable();

        // İşletme adı veya açıklama araması.
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var searchTerm = filter.SearchTerm.Trim();

            query = query.Where(business =>
                EF.Functions.ILike(business.Name, $"%{searchTerm}%") ||
                (business.Description != null &&
                 EF.Functions.ILike(
                     business.Description, $"%{searchTerm}%")));
        }

        // Kategori filtresi.
        if (filter.CategoryId.HasValue)
        {
            query = query.Where(business =>
                business.CategoryId == filter.CategoryId.Value);
        }

        // İl filtresi.
        if (!string.IsNullOrWhiteSpace(filter.City))
        {
            var city = SearchTextHelper.Normalize(filter.City);

            query = query.Where(b =>
                PostgreSqlFunctions.Unaccent(b.City)
                    .ToLower()
                    .Replace("ı", "i") == city);
        }

        // İlçe filtresi.
        if (!string.IsNullOrWhiteSpace(filter.District))
        {
            var district = SearchTextHelper.Normalize(filter.District);

            query = query.Where(b =>
                PostgreSqlFunctions.Unaccent(b.District)
                    .ToLower()
                    .Replace("ı", "i") == district);
        }

        // Kategoriye özel filtreleri uygula.
        foreach (var item in filter.FilterItems)
        {
            switch (item.DataType)
            {
                case BusinessFilterDataType.MultiSelect:

                    var selectedOptionIds = item.SelectedOptionIds;

                    if (selectedOptionIds.Count > 0)
                    {
                        query = query.Where(business =>
                            business.FilterOptions.Any(profileOption =>
                                selectedOptionIds.Contains(
                                    profileOption.BusinessFilterOptionId)));
                    }

                    break;

                case BusinessFilterDataType.Number:

                    if (item.NumericValue.HasValue)
                    {
                        var definitionId = item.DefinitionId;
                        var numericValue = item.NumericValue.Value;

                        query = query.Where(business =>
                            business.FilterValues.Any(value =>
                                value.BusinessFilterDefinitionId == definitionId &&
                                value.NumericValue >= numericValue));
                    }

                    break;

                case BusinessFilterDataType.Text:

                    if (!string.IsNullOrWhiteSpace(item.TextValue))
                    {
                        var definitionId = item.DefinitionId;
                        var textValue = item.TextValue.Trim();

                        query = query.Where(business =>
                            business.FilterValues.Any(value =>
                                value.BusinessFilterDefinitionId == definitionId &&
                                value.TextValue != null &&
                                EF.Functions.ILike(
                                    value.TextValue,
                                    $"%{textValue}%")));
                    }

                    break;

                case BusinessFilterDataType.Boolean:

                    if (item.BooleanValue.HasValue)
                    {
                        var definitionId = item.DefinitionId;
                        var booleanValue = item.BooleanValue.Value;

                        query = query.Where(business =>
                            business.FilterValues.Any(value =>
                                value.BusinessFilterDefinitionId == definitionId &&
                                value.BooleanValue == booleanValue));
                    }

                    break;
            }
        }

        // Filtrelenmiş işletmeleri seçilen sıraya göre getir.
        query = filter.SortBy switch
        {
            "name_desc" => query
                .OrderByDescending(business => business.Name),
            "price_asc" => query
                .OrderByDescending(business => business.Pricings.Any(pricing => pricing.IsActive))
                .ThenBy(business => business.Pricings
                    .Where(pricing => pricing.IsActive)
                    .Min(pricing => (decimal?)pricing.Amount) ?? decimal.MaxValue)
                .ThenBy(business => business.Name),
            "price_desc" => query
                .OrderByDescending(business => business.Pricings.Any(pricing => pricing.IsActive))
                .ThenByDescending(business => business.Pricings
                    .Where(pricing => pricing.IsActive)
                    .Max(pricing => (decimal?)pricing.Amount) ?? decimal.MinValue)
                .ThenBy(business => business.Name),
            "name_asc" => query
                .OrderBy(business => business.Name),
            _ => query
                .OrderBy(business => business.Name)
        };

        filter.Businesses = await query.ToListAsync();

        // Favori işlemleri.
        var favoriteBusinessIds = new HashSet<int>();
        ViewBag.CanFavorite = false;

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!string.IsNullOrEmpty(currentUserId))
        {
            var isCustomer = await _context.Users
                .AnyAsync(user =>
                    user.Id == currentUserId &&
                    user.AccountType == AccountType.Customer);

            if (isCustomer)
            {
                ViewBag.CanFavorite = true;

                favoriteBusinessIds = await _context.BusinessFavorites
                    .Where(favorite =>
                        favorite.ApplicationUserId == currentUserId)
                    .Select(favorite => favorite.BusinessProfileId)
                    .ToHashSetAsync();
            }
        }

        ViewBag.FavoriteBusinessIds = favoriteBusinessIds;

        return View(filter);
    }

    public async Task<IActionResult> WeddingVenues(VenueFilterViewModel filter)
    {
        // İlleri Location API'den al
        filter.Provinces = await _locationService.GetProvincesAsync();
        Console.WriteLine($"Gelen il sayısı: {filter.Provinces.Count}");

        foreach (var province in filter.Provinces)
        {
            Console.WriteLine($"{province.Id} - {province.Name}");
        }
        // Seçilen ilin adını bul
        var selectedProvince = filter.ProvinceId.HasValue
            ? filter.Provinces.FirstOrDefault(
                province => province.Id == filter.ProvinceId.Value)
            : null;

        filter.City = selectedProvince?.Name;

        // Seçilen ile ait ilçeleri API'den al
        if (filter.ProvinceId.HasValue)
        {
            filter.DistrictOptions = await _locationService
                .GetDistrictsAsync(filter.ProvinceId.Value);
        }

        var query = _context.Venues
            .AsNoTracking()
            .Include(venue => venue.BusinessProfile)
            .Include(venue => venue.Photos
                .Where(photo => photo.ApprovalStatus == ReviewStatus.Approved))
            .Include(venue => venue.PhysicalFeatures)
                .ThenInclude(feature => feature.Category)
            .Where(venue =>
                venue.ApprovalStatus == ReviewStatus.Approved &&
                venue.BusinessProfile.ApprovalStatus == ReviewStatus.Approved)
            .AsQueryable();

        // İl filtresi
        if (!string.IsNullOrWhiteSpace(filter.City))
        {
            query = query.Where(venue =>
                venue.City == filter.City);
        }

        // İlçe filtresi
        if (!string.IsNullOrWhiteSpace(filter.District))
        {
            query = query.Where(venue =>
                venue.District == filter.District);
        }

        // Minimum kapasite filtresi
        if (filter.MinCapacity.HasValue)
        {
            query = query.Where(venue =>
                venue.Capacity >= filter.MinCapacity.Value);
        }

        // Arama filtresi
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var searchTerm = filter.SearchTerm.Trim();

            query = query.Where(venue =>
                EF.Functions.ILike(
                    venue.Name,
                    $"%{searchTerm}%") ||

                (venue.Description != null &&
                 EF.Functions.ILike(
                     venue.Description,
                     $"%{searchTerm}%")));
        }

        // Fiziksel özellik filtresi
        if (filter.SelectedFeatureIds.Any())
        {
            query = query.Where(venue =>
                filter.SelectedFeatureIds.All(featureId =>
                    venue.PhysicalFeatures.Any(
                        feature => feature.Id == featureId)));
        }

        filter.Venues = await query
            .OrderBy(venue => venue.Name)
            .ToListAsync();

        ViewBag.CanFavorite = false;
        filter.FavoriteVenueIds = new List<int>();
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrWhiteSpace(currentUserId) &&
            await _context.Users.AnyAsync(user => user.Id == currentUserId &&
                                                   user.AccountType == AccountType.Customer))
        {
            ViewBag.CanFavorite = true;
            filter.FavoriteVenueIds = await _context.FavoriteVenues
                .Where(favorite => favorite.ApplicationUserId == currentUserId)
                .Select(favorite => favorite.VenueId)
                .ToListAsync();
        }

        // Fiziksel özellik kategorilerini yükle
        filter.FeatureCategories = await _context.FeatureCategories
            .AsNoTracking()
            .Include(category => category.PhysicalFeatures)
            .OrderBy(category => category.Name)
            .ToListAsync();

        return View(filter);
    }



    public async Task<IActionResult> Details(int id)
    {
        var today = DateTime.Today;

        var business = await _context.BusinessProfiles
            .Include(b => b.Category)
            .Include(b => b.Photos
                .Where(photo => photo.ApprovalStatus == ReviewStatus.Approved))
            .Include(b => b.ApplicationUser)
            .Include(b => b.AuthorizedPeople)
            .Include(b => b.Pricings.Where(p => p.IsActive))
            .Include(b => b.Packages.Where(p => p.IsActive))
                .ThenInclude(p => p.Items)
            .Include(b => b.Packages.Where(p => p.IsActive))
                .ThenInclude(p => p.Venues)
            .Include(b => b.Campaigns.Where(c =>
                c.IsActive &&
                c.StartDate <= today &&
                c.EndDate >= today))
                .ThenInclude(c => c.BusinessPackage)
            .Include(b => b.Venues
                .Where(venue =>
                    venue.ApprovalStatus == ReviewStatus.Approved))
                .ThenInclude(venue => venue.Photos
                    .Where(photo => photo.ApprovalStatus == ReviewStatus.Approved))
            .FirstOrDefaultAsync(b => b.Id == id);

        if (business == null ||
            business.ApprovalStatus != ReviewStatus.Approved)
        {
            return NotFound();
        }

        ViewBag.CanFavorite = false;
        ViewBag.IsFavorite = false;

        ViewBag.CanRate = false;
        ViewBag.HasRated = false;

        var currentUserId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!string.IsNullOrEmpty(currentUserId))
        {
            var isCustomer = await _context.Users
                .AnyAsync(user =>
                    user.Id == currentUserId &&
                    user.AccountType == AccountType.Customer);

            if (isCustomer)
            {
                // Favori işlemleri
                ViewBag.CanFavorite = true;

                ViewBag.IsFavorite = await _context.BusinessFavorites
                    .AnyAsync(favorite =>
                        favorite.ApplicationUserId == currentUserId &&
                        favorite.BusinessProfileId == id);

                // Puanlama işlemleri
                ViewBag.CanRate = true;

                ViewBag.HasRated = await _context.BusinessRatings
                    .AnyAsync(rating =>
                        rating.ApplicationUserId == currentUserId &&
                        rating.BusinessProfileId == id);
            }
        }
        var ratings = await _context.BusinessRatings
            .Where(rating => rating.BusinessProfileId == id &&
                rating.ReviewStatus == ReviewStatus.Approved)
            .ToListAsync();

        ViewBag.RatingCount = ratings.Count;

        ViewBag.AverageRating = ratings.Count > 0
            ? ratings.Average(rating => rating.Rating)
            : 0;

        ViewBag.Ratings = await _context.BusinessRatings
            .Where(rating => rating.BusinessProfileId == id &&
                rating.ReviewStatus == ReviewStatus.Approved)
            .OrderByDescending(rating => rating.CreatedAt)
            .Select(rating => new BusinessRatingViewModel
            {
                Id = rating.Id,
                Rating = rating.Rating,
                Comment = rating.Comment,
                CreatedAt = rating.CreatedAt,
                CustomerName = _context.CustomerProfiles
                    .Where(profile => profile.ApplicationUserId == rating.ApplicationUserId)
                    .Select(profile => profile.Name + " " + profile.Surname)
                    .FirstOrDefault() ?? "Müşteri",

                BusinessReply = rating.BusinessReply,
                BusinessReplyAt = rating.BusinessReplyAt
            })
            .ToListAsync();

        return View(business);


    }
}
