
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

        return View();
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        string name,
        string? description,
        decimal amount,
        List<string> items)
    {
        var business = await GetCurrentBusinessAsync();

        if (business == null)
        {
            return Forbid();
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
                        Name = item.Trim(),
                        BusinessPackage = null!
                    })
                .ToList()
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
        .FirstOrDefaultAsync(p =>
            p.Id == id &&
            p.BusinessProfileId == business.Id);

    if (package == null)
    {
        return NotFound();
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
    List<string>? items)
{
    var business = await GetCurrentBusinessAsync();

    if (business == null)
    {
        return Forbid();
    }

    var package = await _context.BusinessPackages
        .Include(p => p.Items)
        .FirstOrDefaultAsync(p =>
            p.Id == id &&
            p.BusinessProfileId == business.Id);

    if (package == null)
    {
        return NotFound();
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
