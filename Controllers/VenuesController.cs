using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrganizationPlatform.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using OrganizationPlatform.Models;

namespace OrganizationPlatform.Controllers;

public class VenuesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public VenuesController(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var venues = await _context.Venues.ToListAsync();

        return View(venues);
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

        var businessProfile = await _context.BusinessProfiles
            .FirstOrDefaultAsync(business =>
                business.ApplicationUserId == user.Id);

        if (businessProfile == null)
        {
            return NotFound("İşletme profili bulunamadı.");
        }

        var venues = await _context.Venues
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

        var model = new CreateVenueViewModel();

        ViewBag.PhysicalFeatures = await _context.PhysicalFeatures
            .Include(feature => feature.Category)
            .OrderBy(feature => feature.Category.Name)
            .ThenBy(feature => feature.Name)
            .ToListAsync();

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
            ViewBag.PhysicalFeatures = await _context.PhysicalFeatures
                .Include(feature => feature.Category)
                .OrderBy(feature => feature.Category.Name)
                .ThenBy(feature => feature.Name)
                .ToListAsync();

            return View(model);
        }

        var venue = new Venue
        {
            Name = model.Name,
            City = model.City,
            Capacity = model.Capacity,
            Address = model.Address,
            Description = model.Description,
            BusinessProfileId = businessProfile.Id,
            BusinessProfile = businessProfile,
            PhysicalFeatures = selectedFeatures,
            MealIncludedPrice = model.MealIncludedPrice,
            MealExcludedPrice = model.MealExcludedPrice,
        };

        _context.Venues.Add(venue);
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

        var model = new CreateVenueViewModel
        {
            Name = venue.Name,
            City = venue.City,
            Capacity = venue.Capacity,
            Address = venue.Address ?? string.Empty,
            Description = venue.Description,
            MealIncludedPrice = venue.MealIncludedPrice,
            MealExcludedPrice = venue.MealExcludedPrice,
            SelectedPhysicalFeatureIds = venue.PhysicalFeatures
                .Select(f => f.Id)
                .ToList()
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
            await LoadPhysicalFeatures();
            return View(model);
        }

        venue.Name = model.Name;
        venue.City = model.City;
        venue.Capacity = model.Capacity;
        venue.Address = model.Address;
        venue.Description = model.Description;
        venue.MealIncludedPrice = model.MealIncludedPrice;
        venue.MealExcludedPrice = model.MealExcludedPrice;

        venue.PhysicalFeatures.Clear();

        foreach (var feature in selectedFeatures)
        {
            venue.PhysicalFeatures.Add(feature);
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


}