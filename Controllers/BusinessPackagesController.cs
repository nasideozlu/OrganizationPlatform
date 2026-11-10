
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrganizationPlatform.Data;
using OrganizationPlatform.Models;

namespace OrganizationPlatform.Controllers;

[Authorize]
public class BusinessPackagesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public BusinessPackagesController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    private async Task<BusinessProfile?> GetCurrentBusinessAsync()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null || user.AccountType != AccountType.Business)
        {
            return null;
        }

        return await _context.BusinessProfiles
            .FirstOrDefaultAsync(p => p.ApplicationUserId == user.Id);
    }
    private async Task LoadVenuesAsync(int businessId, int? selectedVenueId = null)
    {
        ViewBag.Venues = await _context.Venues
            .Where(venue => venue.BusinessProfileId == businessId)
            .OrderBy(venue => venue.Name)
            .ToListAsync();

        ViewBag.SelectedVenueId = selectedVenueId;
    }


    public async Task<IActionResult> Index()
    {
        var business = await GetCurrentBusinessAsync();

        if (business == null)
        {
            return Forbid();
        }

        var packages = await _context.BusinessPackages
            .AsNoTracking()
            .Where(p => p.BusinessProfileId == business.Id)
            .Include(p => p.Items)
            .Include(p => p.Venues)
            .OrderBy(p => p.Name)
            .ToListAsync();

        return View(packages);
    }


    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var business = await GetCurrentBusinessAsync();

        if (business == null)
        {
            return Forbid();
        }

        var isWeddingVenue = await _context.BusinessProfiles
            .Where(profile => profile.Id == business.Id)
            .Select(profile => profile.Category.Name == "Düğün Mekanı")
            .FirstOrDefaultAsync();

        ViewBag.IsWeddingVenue = isWeddingVenue;

        if (isWeddingVenue)
        {
            await LoadVenuesAsync(business.Id);
        }

        return View();
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
    string name,
    string? description,
    decimal amount,
    List<string> items,
    List<int>? venueIds)
    {
        var business = await GetCurrentBusinessAsync();

        if (business == null)
        {
            return Forbid();
        }

        var isWeddingVenue = await _context.BusinessProfiles
            .Where(profile => profile.Id == business.Id)
            .Select(profile => profile.Category.Name == "Düğün Mekanı")
            .FirstOrDefaultAsync();

        var selectedVenues = new List<Venue>();

        if (isWeddingVenue)
        {
            selectedVenues = await _context.Venues
                .Where(venue =>
                    venue.BusinessProfileId == business.Id &&
                    venueIds != null &&
                    venueIds.Contains(venue.Id))
                .ToListAsync();

            if (selectedVenues.Count == 0)
            {
                ModelState.AddModelError(
                    "venueIds",
                    "En az bir mekân seçmelisiniz.");
            }
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            ModelState.AddModelError(
                nameof(name),
                "Paket adı zorunludur.");
        }

        if (amount < 0)
        {
            ModelState.AddModelError(
                nameof(amount),
                "Paket fiyatı negatif olamaz.");
        }

        if (items == null || !items.Any(item => !string.IsNullOrWhiteSpace(item)))
        {
            ModelState.AddModelError(
                nameof(items),
                "En az bir paket içeriği eklemelisiniz.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.IsWeddingVenue = isWeddingVenue;

            if (isWeddingVenue)
            {
                await LoadVenuesAsync(business.Id);
            }

            return View();
        }

        var package = new BusinessPackage
        {
            Name = name.Trim(),
            Description = string.IsNullOrWhiteSpace(description)
                ? null
                : description.Trim(),
            Amount = amount,
            BusinessProfileId = business.Id,
            BusinessProfile = business,

            Items = (items ?? new List<string>())
    .Where(item => !string.IsNullOrWhiteSpace(item))
    .Select(item => new BusinessPackageItem
    {
        Name = item!.Trim(),
        BusinessPackage = null!
    })
    .ToList(),

            Venues = selectedVenues
        };

        _context.BusinessPackages.Add(package);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var business = await GetCurrentBusinessAsync();

        if (business == null)
        {
            return Forbid();
        }

        var package = await _context.BusinessPackages
            .Include(p => p.Items)
            .Include(p => p.Venues)
            .FirstOrDefaultAsync(p =>
                p.Id == id &&
                p.BusinessProfileId == business.Id);

        if (package == null)
        {
            return NotFound();
        }

        var isWeddingVenue = await _context.BusinessProfiles
            .Where(profile => profile.Id == business.Id)
            .Select(profile => profile.Category.Name == "Düğün Mekanı")
            .FirstOrDefaultAsync();

        ViewBag.IsWeddingVenue = isWeddingVenue;

        if (isWeddingVenue)
        {
            await LoadVenuesAsync(
                business.Id,
                package.Venues.FirstOrDefault()?.Id);
        }

        return View(package);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
    int id,
    string name,
    string? description,
    decimal amount,
    List<string>? items,
    List<int>? venueIds)
    {
        var business = await GetCurrentBusinessAsync();

        if (business == null)
        {
            return Forbid();
        }

        var package = await _context.BusinessPackages
            .Include(p => p.Items)
            .Include(p => p.Venues)
            .FirstOrDefaultAsync(p =>
                p.Id == id &&
                p.BusinessProfileId == business.Id);

        if (package == null)
        {
            return NotFound();
        }

        var isWeddingVenue = await _context.BusinessProfiles
            .Where(profile => profile.Id == business.Id)
            .Select(profile => profile.Category.Name == "Düğün Mekanı")
            .FirstOrDefaultAsync();

        var selectedVenues = new List<Venue>();

        if (isWeddingVenue)
        {
            selectedVenues = await _context.Venues
                .Where(venue =>
                    venue.BusinessProfileId == business.Id &&
                    venueIds != null &&
                    venueIds.Contains(venue.Id))
                .ToListAsync();

            if (selectedVenues.Count == 0)
            {
                ModelState.AddModelError(
                    "venueIds",
                    "En az bir mekân seçmelisiniz.");
            }
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            ModelState.AddModelError(
                nameof(name),
                "Paket adı zorunludur.");
        }

        if (amount < 0)
        {
            ModelState.AddModelError(
                nameof(amount),
                "Paket fiyatı negatif olamaz.");
        }

        var validItems = (items ?? new List<string>())
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Select(item => item.Trim())
            .ToList();

        if (validItems.Count == 0)
        {
            ModelState.AddModelError(
                nameof(items),
                "En az bir paket içeriği eklemelisiniz.");
        }

        if (!ModelState.IsValid)
        {
            package.Name = name ?? string.Empty;
            package.Description = description;
            package.Amount = amount;
            package.Items = validItems
                .Select(item => new BusinessPackageItem
                {
                    Name = item,
                    BusinessPackage = package
                })
                .ToList();

            package.Venues = selectedVenues;

            ViewBag.IsWeddingVenue = isWeddingVenue;
            await LoadVenuesAsync(business.Id);

            return View(package);
        }

        package.Name = name.Trim();
        package.Description = string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();

        package.Amount = amount;

        _context.BusinessPackageItems.RemoveRange(package.Items);

        package.Items = validItems
            .Select(item => new BusinessPackageItem
            {
                Name = item,
                BusinessPackage = package
            })
            .ToList();

        package.Venues = selectedVenues;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var business = await GetCurrentBusinessAsync();

        if (business == null)
        {
            return Forbid();
        }

        var package = await _context.BusinessPackages
            .FirstOrDefaultAsync(p =>
                p.Id == id &&
                p.BusinessProfileId == business.Id);

        if (package == null)
        {
            return NotFound();
        }

        package.IsActive = !package.IsActive;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }


}
