using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrganizationPlatform.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using OrganizationPlatform.Models;
using OrganizationPlatform.Services;
using OrganizationPlatform.Helpers;


namespace OrganizationPlatform.Controllers;

public class VenuesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly LocationService _locationService;
    private readonly PhotoStorageService _photoStorageService;


    public VenuesController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        LocationService locationService,
        PhotoStorageService photoStorageService)
    {
        _context = context;
        _userManager = userManager;
        _locationService = locationService;
        _photoStorageService = photoStorageService;
    }



    [HttpGet]
    public async Task<IActionResult> Index(VenueFilterViewModel filter)
    {
        // İlleri Location API'den al
        filter.Provinces = await _locationService.GetProvincesAsync();

        // Seçilen ilin adını bul
        var selectedProvince = filter.ProvinceId.HasValue
            ? filter.Provinces.FirstOrDefault(
                province => province.Id == filter.ProvinceId.Value)
            : null;

        if (selectedProvince != null)
        {
            filter.City = selectedProvince.Name;
        }

        // Seçilen ilin ilçelerini API'den al
        if (filter.ProvinceId.HasValue)
        {
            filter.DistrictOptions = await _locationService
                .GetDistrictsAsync(filter.ProvinceId.Value);
        }

        var query = _context.Venues
            .AsNoTracking()
            .AsQueryable();


        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var searchTerm = filter.SearchTerm.Trim();

            query = query.Where(venue =>
                EF.Functions.ILike(venue.Name, $"%{searchTerm}%") ||
                EF.Functions.ILike(venue.City, $"%{searchTerm}%") ||
                EF.Functions.ILike(venue.District, $"%{searchTerm}%") ||
                EF.Functions.ILike(venue.Neighborhood, $"%{searchTerm}%"));
        }


        if (!string.IsNullOrWhiteSpace(filter.City))
        {
            var city = SearchTextHelper.Normalize(filter.City);

            query = query.Where(v =>
                PostgreSqlFunctions.Unaccent(v.City)
                    .ToLower()
                    .Replace("ı", "i") == city);
        }

        if (!string.IsNullOrWhiteSpace(filter.District))
        {
            var district = SearchTextHelper.Normalize(filter.District);

            query = query.Where(v =>
                PostgreSqlFunctions.Unaccent(v.District)
                    .ToLower()
                    .Replace("ı", "i") == district);
        }

        // 3. Minimum kapasite filtresi
        if (filter.MinCapacity.HasValue)
        {
            query = query.Where(venue =>
                venue.Capacity >= filter.MinCapacity.Value);
        }

        // 4. Fiyat filtresi
        if (filter.MinPrice.HasValue || filter.MaxPrice.HasValue)
        {
            if (filter.PriceType == "included")
            {
                query = query.Where(venue =>
                    venue.MealIncludedPrice.HasValue &&
                    (!filter.MinPrice.HasValue ||
                     venue.MealIncludedPrice.Value >= filter.MinPrice.Value) &&
                    (!filter.MaxPrice.HasValue ||
                     venue.MealIncludedPrice.Value <= filter.MaxPrice.Value));
            }
            else if (filter.PriceType == "excluded")
            {
                query = query.Where(venue =>
                    venue.MealExcludedPrice.HasValue &&
                    (!filter.MinPrice.HasValue ||
                     venue.MealExcludedPrice.Value >= filter.MinPrice.Value) &&
                    (!filter.MaxPrice.HasValue ||
                     venue.MealExcludedPrice.Value <= filter.MaxPrice.Value));
            }
            else
            {
                query = query.Where(venue =>
                    (venue.MealIncludedPrice.HasValue &&
                     (!filter.MinPrice.HasValue ||
                      venue.MealIncludedPrice.Value >= filter.MinPrice.Value) &&
                     (!filter.MaxPrice.HasValue ||
                      venue.MealIncludedPrice.Value <= filter.MaxPrice.Value))
                    ||
                    (venue.MealExcludedPrice.HasValue &&
                     (!filter.MinPrice.HasValue ||
                      venue.MealExcludedPrice.Value >= filter.MinPrice.Value) &&
                     (!filter.MaxPrice.HasValue ||
                      venue.MealExcludedPrice.Value <= filter.MaxPrice.Value)));
            }
        }

        // 5. Seçilen fiziksel özelliklerin tamamı mekânda bulunmalı
        var selectedFeatureIds = filter.SelectedFeatureIds
            .Distinct()
            .ToList();

        foreach (var featureId in selectedFeatureIds)
        {
            query = query.Where(venue =>
                venue.PhysicalFeatures.Any(feature =>
                    feature.Id == featureId));
        }

        // 6. Sonuçları ve fotoğrafları getir

        var sortBy = filter.SortBy;

        query = sortBy switch
        {
            "price_asc" => query
                .OrderBy(venue =>
                    (filter.PriceType == "included"
                        ? venue.MealIncludedPrice
                        : filter.PriceType == "excluded"
                            ? venue.MealExcludedPrice
                            : venue.MealIncludedPrice ?? venue.MealExcludedPrice)
                    ?? decimal.MaxValue)
                .ThenBy(venue => venue.Name),

            "price_desc" => query
                .OrderByDescending(venue =>
                    (filter.PriceType == "included"
                        ? venue.MealIncludedPrice
                        : filter.PriceType == "excluded"
                            ? venue.MealExcludedPrice
                            : venue.MealIncludedPrice ?? venue.MealExcludedPrice)
                    ?? decimal.MinValue)
                .ThenBy(venue => venue.Name),

            "capacity_asc" => query
                .OrderBy(venue => venue.Capacity)
                .ThenBy(venue => venue.Name),

            "capacity_desc" => query
                .OrderByDescending(venue => venue.Capacity)
                .ThenBy(venue => venue.Name),

            _ => query
                .OrderBy(venue => venue.City)
                .ThenBy(venue => venue.Name)
        };

        filter.Venues = await query
            .Include(venue => venue.Photos)
            .Include(venue => venue.PhysicalFeatures)
            .ToListAsync();


        // 7. Fiziksel özellik kategorilerini yükle

        filter.FeatureCategories = await _context.FeatureCategories
            .AsNoTracking()
            .Include(category => category.PhysicalFeatures)
            .OrderBy(category => category.Name)
            .ToListAsync();


        ViewBag.CanFavorite = false;
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser?.AccountType == AccountType.Customer)
        {
            ViewBag.CanFavorite = true;

            filter.FavoriteVenueIds = await _context.FavoriteVenues
                .Where(f => f.ApplicationUserId == currentUser.Id)
                .Select(f => f.VenueId)
                .ToListAsync();
        }
        return View(filter);
    }


    public async Task<IActionResult> Details(int id)
    {
        var venue = await _context.Venues
            .Include(v => v.BusinessProfile)
                .ThenInclude(b => b.Category)
            .Include(v => v.Photos)
            .Include(v => v.PhysicalFeatures)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (venue == null)
        {
            return NotFound();
        }

        ViewBag.CanFavorite = false;
        ViewBag.IsFavorite = false;
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser?.AccountType == AccountType.Customer)
        {
            ViewBag.CanFavorite = true;
            ViewBag.IsFavorite = await _context.FavoriteVenues.AnyAsync(favorite =>
                favorite.ApplicationUserId == currentUser.Id && favorite.VenueId == id);
        }

        return View(venue);
    }

    [Authorize]
    public async Task<IActionResult> MyVenues()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Challenge();
        }

        if (user.AccountType != AccountType.Business)
        {
            return Forbid();
        }

        if (!await CanManageVenues(user.Id))
        {
            return Forbid();
        }

        var businessProfile = await _context.BusinessProfiles
            .FirstOrDefaultAsync(business =>
                business.ApplicationUserId == user.Id);

        if (businessProfile == null)
        {
            return NotFound("İşletme profili bulunamadı.");
        }


        var venues = await _context.Venues
            .Include(venue => venue.Photos)
            .Where(venue =>
                venue.BusinessProfileId == businessProfile.Id)
            .ToListAsync();


        return View(venues);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Challenge();
        }

        if (user.AccountType != AccountType.Business)
        {
            return Forbid();
        }

        if (!await CanManageVenues(user.Id))
        {
            return Forbid();
        }

        var model = new CreateVenueViewModel();

        await LoadPhysicalFeatures();
        await LoadVenueLocationOptionsAsync(null, null);
        return View(model);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateVenueViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Challenge();
        }

        if (user.AccountType != AccountType.Business)
        {
            return Forbid();
        }

        if (!await CanManageVenues(user.Id))
        {
            return Forbid();
        }

        var businessProfile = await _context.BusinessProfiles
            .FirstOrDefaultAsync(b => b.ApplicationUserId == user.Id);

        if (businessProfile == null)
        {
            return NotFound("İşletme profili bulunamadı.");
        }

        if (model.Capacity <= 0)
        {
            ModelState.AddModelError(
                nameof(model.Capacity),
                "Kapasite sıfırdan büyük olmalıdır.");
        }

        var selectedIds = model.SelectedPhysicalFeatureIds.Distinct().ToList();

        var selectedFeatures = await _context.PhysicalFeatures
            .Where(feature => selectedIds.Contains(feature.Id))
            .ToListAsync();

        if (selectedFeatures.Count != selectedIds.Count)
        {
            ModelState.AddModelError(
                nameof(model.SelectedPhysicalFeatureIds),
                "Seçilen fiziksel özelliklerden biri geçerli değil.");
        }

        if (!ModelState.IsValid)
        {
            await LoadPhysicalFeatures();
            await LoadVenueLocationOptionsAsync(model.City, model.District);
            return View(model);
        }

        var venue = new Venue
        {
            Name = model.Name,
            City = model.City,
            District = model.District,
            Neighborhood = model.Neighborhood,
            Address = model.Address,
            Capacity = model.Capacity,
            Description = model.Description,
            BusinessProfileId = businessProfile.Id,
            BusinessProfile = businessProfile,
            PhysicalFeatures = selectedFeatures,
            MealIncludedPrice = model.MealIncludedPrice,
            MealExcludedPrice = model.MealExcludedPrice,
        };

        _context.Venues.Add(venue);
        await _context.SaveChangesAsync();

        foreach (var photo in model.Photos)
        {
            if (photo.Length == 0)
            {
                continue;
            }

            var filePath = await _photoStorageService.SavePhotoAsync(
                photo,
                "venues",
                venue.Id);

            var venuePhoto = new VenuePhoto
            {
                FilePath = filePath,
                VenueId = venue.Id,
                Venue = venue
            };

            _context.VenuePhotos.Add(venuePhoto);
        }

        await _context.SaveChangesAsync();


        return RedirectToAction(nameof(MyVenues));
    }


    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return Challenge();

        if (user.AccountType != AccountType.Business)
            return Forbid();

        if (!await CanManageVenues(user.Id))
        {
            return Forbid();
        }

        var businessProfile = await _context.BusinessProfiles
            .FirstOrDefaultAsync(b => b.ApplicationUserId == user.Id);

        if (businessProfile == null)
            return NotFound("İşletme profili bulunamadı.");


        var venue = await _context.Venues
            .Include(v => v.PhysicalFeatures)
            .Include(v => v.Photos)
            .FirstOrDefaultAsync(v =>
                v.Id == id &&
                v.BusinessProfileId == businessProfile.Id);


        if (venue == null)
            return NotFound();

        var model = new CreateVenueViewModel
        {
            Name = venue.Name,
            City = venue.City,
            District = venue.District,
            Neighborhood = venue.Neighborhood,
            Capacity = venue.Capacity,
            Address = venue.Address ?? string.Empty,
            Description = venue.Description,
            MealIncludedPrice = venue.MealIncludedPrice,
            MealExcludedPrice = venue.MealExcludedPrice,
            SelectedPhysicalFeatureIds = venue.PhysicalFeatures
                .Select(f => f.Id)
                .ToList(),
            ExistingPhotos = venue.Photos
        };

        await LoadPhysicalFeatures();

        return View(model);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CreateVenueViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return Challenge();

        if (user.AccountType != AccountType.Business)
            return Forbid();

        if (!await CanManageVenues(user.Id))
        {
            return Forbid();
        }

        var businessProfile = await _context.BusinessProfiles
            .FirstOrDefaultAsync(b => b.ApplicationUserId == user.Id);

        if (businessProfile == null)
            return NotFound("İşletme profili bulunamadı.");

        var venue = await _context.Venues
            .Include(v => v.PhysicalFeatures)
            .FirstOrDefaultAsync(v =>
                v.Id == id &&
                v.BusinessProfileId == businessProfile.Id);

        if (venue == null)
            return NotFound();

        if (model.Capacity <= 0)
        {
            ModelState.AddModelError(
                nameof(model.Capacity),
                "Kapasite sıfırdan büyük olmalıdır.");
        }

        var selectedIds = model.SelectedPhysicalFeatureIds
            .Distinct()
            .ToList();

        var selectedFeatures = await _context.PhysicalFeatures
            .Where(f => selectedIds.Contains(f.Id))
            .ToListAsync();

        if (selectedFeatures.Count != selectedIds.Count)
        {
            ModelState.AddModelError(
                nameof(model.SelectedPhysicalFeatureIds),
                "Seçilen fiziksel özelliklerden biri geçerli değil.");
        }


        if (!ModelState.IsValid)
        {
            model.ExistingPhotos = await _context.VenuePhotos
                .Where(p => p.VenueId == venue.Id)
                .ToListAsync();

            await LoadPhysicalFeatures();
            return View(model);
        }


        venue.Name = model.Name;
        venue.City = model.City;
        venue.District = model.District;
        venue.Neighborhood = model.Neighborhood;
        venue.Address = model.Address;
        venue.Capacity = model.Capacity;
        venue.Description = model.Description;
        venue.MealIncludedPrice = model.MealIncludedPrice;
        venue.MealExcludedPrice = model.MealExcludedPrice;

        venue.PhysicalFeatures.Clear();

        foreach (var feature in selectedFeatures)
        {
            venue.PhysicalFeatures.Add(feature);
        }

        foreach (var photo in model.Photos)
        {
            if (photo.Length == 0)
            {
                continue;
            }

            var filePath = await _photoStorageService.SavePhotoAsync(
                photo,
                "venues",
                venue.Id);

            var venuePhoto = new VenuePhoto
            {
                FilePath = filePath,
                VenueId = venue.Id,
                Venue = venue
            };

            _context.VenuePhotos.Add(venuePhoto);
        }


        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(MyVenues));
    }

    private async Task LoadPhysicalFeatures()
    {
        ViewBag.PhysicalFeatures = await _context.PhysicalFeatures
            .Include(feature => feature.Category)
            .OrderBy(feature => feature.Category.Name)
            .ThenBy(feature => feature.Name)
            .ToListAsync();
    }

    private async Task LoadVenueLocationOptionsAsync(string? city, string? district)
    {
        var provinces = await _locationService.GetProvincesAsync();
        ViewBag.Provinces = provinces;

        var selectedProvince = provinces.FirstOrDefault(province => province.Name == city);
        var districts = selectedProvince is not null
            ? await _locationService.GetDistrictsAsync(selectedProvince.Id)
            : new List<LocationItem>();
        ViewBag.Districts = districts;

        var selectedDistrict = districts.FirstOrDefault(item => item.Name == district);
        ViewBag.Neighborhoods = selectedDistrict is not null
            ? await _locationService.GetNeighborhoodsAsync(selectedDistrict.Id)
            : new List<LocationItem>();
    }

    private async Task<bool> CanManageVenues(string userId)
    {
        var businessProfile = await _context.BusinessProfiles
            .Include(business => business.Category)
            .FirstOrDefaultAsync(business =>
                business.ApplicationUserId == userId);

        if (businessProfile == null)
        {
            return false;
        }

        return businessProfile.Category.Name == "Düğün Mekanı";
    }


    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return Challenge();

        if (user.AccountType != AccountType.Business)
            return Forbid();

        if (!await CanManageVenues(user.Id))
        {
            return Forbid();
        }

        var businessProfile = await _context.BusinessProfiles
            .FirstOrDefaultAsync(b => b.ApplicationUserId == user.Id);

        if (businessProfile == null)
            return NotFound("İşletme profili bulunamadı.");

        var venue = await _context.Venues
            .FirstOrDefaultAsync(v =>
                v.Id == id &&
                v.BusinessProfileId == businessProfile.Id);

        if (venue == null)
            return NotFound();

        _context.Venues.Remove(venue);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(MyVenues));
    }


    [HttpGet]
    public async Task<IActionResult> GetDistricts(int provinceId)
    {
        var districts = await _locationService
            .GetDistrictsAsync(provinceId);

        return Json(districts);
    }

    [HttpGet]
    public async Task<IActionResult> GetNeighborhoods(int districtId)
    {
        var neighborhoods = await _locationService
            .GetNeighborhoodsAsync(districtId);

        return Json(neighborhoods);
    }


    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteVenuePhoto(int id)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return Challenge();

        if (user.AccountType != AccountType.Business)
            return Forbid();

        if (!await CanManageVenues(user.Id))
            return Forbid();

        var businessProfile = await _context.BusinessProfiles
            .FirstOrDefaultAsync(b => b.ApplicationUserId == user.Id);

        if (businessProfile == null)
            return NotFound();

        var photo = await _context.VenuePhotos
            .Include(p => p.Venue)
            .FirstOrDefaultAsync(p =>
                p.Id == id &&
                p.Venue.BusinessProfileId == businessProfile.Id);

        if (photo == null)
            return NotFound();

        var venueId = photo.VenueId;

        await _photoStorageService.DeletePhotoAsync(
            photo.FilePath,
            "venues",
            venueId);

        _context.VenuePhotos.Remove(photo);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Edit), new { id = venueId });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleFavorite(int venueId, string? returnUrl)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Challenge();

        if (user.AccountType != AccountType.Customer)
            return Forbid();

        var favorite = await _context.FavoriteVenues
            .FirstOrDefaultAsync(f =>
                f.ApplicationUserId == user.Id &&
                f.VenueId == venueId);

        if (favorite == null)
        {
            var venueExists = await _context.Venues
                .AnyAsync(v => v.Id == venueId);

            if (!venueExists)
                return NotFound();

            _context.FavoriteVenues.Add(new FavoriteVenue
            {
                ApplicationUserId = user.Id,
                VenueId = venueId
            });
        }
        else
        {
            _context.FavoriteVenues.Remove(favorite);
        }

        await _context.SaveChangesAsync();

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);

        return RedirectToAction(nameof(Details), new { id = venueId });
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> MyFavorites()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Challenge();
        if (user.AccountType != AccountType.Customer)
            return Forbid();

        return RedirectToAction("Index", "BusinessFavorites");
    }

}
