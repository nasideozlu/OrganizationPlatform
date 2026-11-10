
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrganizationPlatform.Data;
using OrganizationPlatform.Models;

namespace OrganizationPlatform.Controllers;

[Authorize]
public class BusinessPricingsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public BusinessPricingsController(
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

        var pricings = await _context.BusinessPricings
            .AsNoTracking()
            .Where(p => p.BusinessProfileId == business.Id)
            .OrderBy(p => p.ServiceName)
            .ToListAsync();

        return View(pricings);
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
        string serviceName,
        string? description,
        decimal amount,
        BusinessPricingType pricingType)
    {
        var business = await GetCurrentBusinessAsync();

        if (business == null)
        {
            return Forbid();
        }

        if (string.IsNullOrWhiteSpace(serviceName))
        {
            ModelState.AddModelError(
                nameof(serviceName),
                "Hizmet adı zorunludur.");
        }

        if (amount < 0)
        {
            ModelState.AddModelError(
                nameof(amount),
                "Fiyat negatif olamaz.");
        }

        if (!Enum.IsDefined(typeof(BusinessPricingType), pricingType))
        {
            ModelState.AddModelError(
                nameof(pricingType),
                "Geçerli bir fiyat türü seçmelisiniz.");
        }

        if (!ModelState.IsValid)
        {
            return View();
        }

        var pricing = new BusinessPricing
        {
            ServiceName = serviceName.Trim(),
            Description = string.IsNullOrWhiteSpace(description)
                ? null
                : description.Trim(),
            Amount = amount,
            PricingType = pricingType,
            BusinessProfileId = business.Id,
            BusinessProfile = business
        };

        _context.BusinessPricings.Add(pricing);
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

        var pricing = await _context.BusinessPricings
            .FirstOrDefaultAsync(p =>
                p.Id == id &&
                p.BusinessProfileId == business.Id);

        if (pricing == null)
        {
            return NotFound();
        }

        return View(pricing);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        string serviceName,
        string? description,
        decimal amount,
        BusinessPricingType pricingType)
    {
        var business = await GetCurrentBusinessAsync();

        if (business == null)
        {
            return Forbid();
        }

        var pricing = await _context.BusinessPricings
            .FirstOrDefaultAsync(p =>
                p.Id == id &&
                p.BusinessProfileId == business.Id);

        if (pricing == null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(serviceName))
        {
            ModelState.AddModelError(
                nameof(serviceName),
                "Hizmet adı zorunludur.");
        }

        if (amount < 0)
        {
            ModelState.AddModelError(
                nameof(amount),
                "Fiyat negatif olamaz.");
        }

        if (!Enum.IsDefined(typeof(BusinessPricingType), pricingType))
        {
            ModelState.AddModelError(
                nameof(pricingType),
                "Geçerli bir fiyat türü seçmelisiniz.");
        }

        if (!ModelState.IsValid)
        {
            return View(pricing);
        }

        pricing.ServiceName = serviceName.Trim();
        pricing.Description = string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();
        pricing.Amount = amount;
        pricing.PricingType = pricingType;

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

        var pricing = await _context.BusinessPricings
            .FirstOrDefaultAsync(p =>
                p.Id == id &&
                p.BusinessProfileId == business.Id);

        if (pricing == null)
        {
            return NotFound();
        }

        pricing.IsActive = !pricing.IsActive;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
