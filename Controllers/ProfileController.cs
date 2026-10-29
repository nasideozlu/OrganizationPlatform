using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrganizationPlatform.Data;
using OrganizationPlatform.Models;
using OrganizationPlatform.Services;
using System.Linq;

namespace OrganizationPlatform.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly PhotoStorageService _photoStorageService;
    private readonly LocationService _locationService;

    public ProfileController(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager,
    PhotoStorageService photoStorageService,
    LocationService locationService)
    {
        _context = context;
        _userManager = userManager;
        _photoStorageService = photoStorageService;
        _locationService = locationService;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Challenge();
        }

        if (user.AccountType == AccountType.Customer)
        {
            ViewBag.VenueCount = await _context.Venues.CountAsync();
            ViewBag.BusinessCount = await _context.BusinessProfiles
                .CountAsync(business => business.Category.Name != "Düğün Salonu");
            var profile = await _context.CustomerProfiles
                .Include(p => p.ApplicationUser)
                .FirstOrDefaultAsync(p => p.ApplicationUserId == user.Id);

            if (profile == null)
            {
                return NotFound();
            }

            return View("CustomerProfile", profile);
        }

        if (user.AccountType == AccountType.Business)
        {
            var profile = await _context.BusinessProfiles
                .Include(p => p.Category)
                .Include(p => p.ApplicationUser)
                .Include(p => p.Photos)
                .FirstOrDefaultAsync(p => p.ApplicationUserId == user.Id);

            if (profile == null)
            {
                return NotFound();
            }

            ViewBag.VenueCount = await _context.Venues
                .CountAsync(venue => venue.BusinessProfileId == profile.Id);

            return View("BusinessProfile", profile);
        }

        return Forbid();
    }

    [HttpGet]
    public async Task<IActionResult> EditCustomer()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Challenge();
        }

        var profile = await _context.CustomerProfiles
            .FirstOrDefaultAsync(p => p.ApplicationUserId == user.Id);

        if (profile == null)
        {
            return NotFound();
        }

        return View(profile);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCustomer(
        string name,
        string surname,
        string phoneNumber)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Challenge();
        }

        var profile = await _context.CustomerProfiles
            .FirstOrDefaultAsync(p => p.ApplicationUserId == user.Id);

        if (profile == null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(surname) ||
            string.IsNullOrWhiteSpace(phoneNumber))
        {
            ModelState.AddModelError("", "Tüm alanları doldurmalısın.");

            profile.ApplicationUser = user;
            return View(profile);
        }

        profile.Name = name;
        profile.Surname = surname;
        user.PhoneNumber = phoneNumber;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }



    [HttpGet]
    public async Task<IActionResult> EditBusiness()
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

        var profile = await _context.BusinessProfiles
    .Include(p => p.Photos)
    .FirstOrDefaultAsync(p => p.ApplicationUserId == user.Id);
        if (profile == null)
        {
            return NotFound();
        }


        var model = new BusinessProfileEditViewModel
        {
            Name = profile.Name,
            City = profile.City,
            District = profile.District,
            Description = profile.Description,
            ExistingPhotos = profile.Photos
        };

        ViewBag.Provinces = await _locationService.GetProvincesAsync();

        var selectedProvince = ((List<LocationItem>)ViewBag.Provinces)
            .FirstOrDefault(p => p.Name == model.City);

        ViewBag.Districts = selectedProvince is not null
            ? await _locationService.GetDistrictsAsync(selectedProvince.Id)
            : new List<LocationItem>();
        return View(model);
    }




    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditBusiness(BusinessProfileEditViewModel model)
    {
        Console.WriteLine("EditBusiness POST metoduna girildi.");
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Challenge();
        }

        if (user.AccountType != AccountType.Business)
        {
            return Forbid();
        }

        var profile = await _context.BusinessProfiles
            .Include(p => p.Photos)
            .FirstOrDefaultAsync(p => p.ApplicationUserId == user.Id);

        if (profile == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            model.ExistingPhotos = profile.Photos;
            await LoadBusinessLocationAsync(model.City);
            return View(model);
        }

        profile.Name = model.Name;
        profile.Description = model.Description;
        profile.City = model.City;
        profile.District = model.District;

        try
        {
            foreach (var photo in model.Photos.Where(p => p.Length > 0))
            {
                var filePath = await _photoStorageService.SavePhotoAsync(
                    photo,
                    "businesses",
                    profile.Id);

                var businessPhoto = new BusinessPhoto
                {
                    FilePath = filePath,
                    BusinessProfileId = profile.Id,
                    BusinessProfile = profile
                };

                _context.BusinessPhotos.Add(businessPhoto);
            }
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(nameof(model.Photos), exception.Message);
            model.ExistingPhotos = profile.Photos;
            await LoadBusinessLocationAsync(model.City);
            return View(model);
        }

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));

    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteBusinessPhoto(int id)
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

        var profile = await _context.BusinessProfiles
            .FirstOrDefaultAsync(p => p.ApplicationUserId == user.Id);

        if (profile == null)
        {
            return NotFound();
        }

        var photo = await _context.BusinessPhotos
            .FirstOrDefaultAsync(p =>
                p.Id == id &&
                p.BusinessProfileId == profile.Id);

        if (photo == null)
        {
            return NotFound();
        }

        await _photoStorageService.DeletePhotoAsync(
            photo.FilePath,
            "businesses",
            profile.Id);

        _context.BusinessPhotos.Remove(photo);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(EditBusiness));
    }

    private async Task LoadBusinessLocationAsync(string? city)
    {
        ViewBag.Provinces = await _locationService.GetProvincesAsync();

        var provinces =
            (List<LocationItem>)ViewBag.Provinces;

        var selectedProvince = provinces
            .FirstOrDefault(p => p.Name == city);

        ViewBag.Districts = selectedProvince != null
            ? await _locationService.GetDistrictsAsync(selectedProvince.Id)
            : new List<LocationItem>();
    }

}

