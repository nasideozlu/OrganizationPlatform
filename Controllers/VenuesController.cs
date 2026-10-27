using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrganizationPlatform.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using OrganizationPlatform.Models;
using OrganizationPlatform.Services;

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


    public async Task<IActionResult> Index()
    {
        var venues = await _context.Venues
            .Include(venue => venue.Photos)
            .OrderBy(venue => venue.City)
            .ThenBy(venue => venue.Name)
            .ToListAsync();

        return View(venues);
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

        return businessProfile.Category.Name == "Düğün Salonu";
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



}
