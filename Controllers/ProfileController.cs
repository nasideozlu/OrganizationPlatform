using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrganizationPlatform.Data;
using OrganizationPlatform.Models;

namespace OrganizationPlatform.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProfileController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
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
                .FirstOrDefaultAsync(p => p.ApplicationUserId == user.Id);

            if (profile == null)
            {
                return NotFound();
            }

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
            .FirstOrDefaultAsync(p => p.ApplicationUserId == user.Id);

        if (profile == null)
        {
            return NotFound();
        }

        var categories = await _context.BusinessCategories.ToListAsync();

        var model = new BusinessProfileEditViewModel
        {
            Name = profile.Name,
            City = profile.City,
            CategoryId = profile.CategoryId,
            Description = profile.Description,
            Categories = categories
        };

        return View(model);
    }




    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditBusiness(BusinessProfileEditViewModel model)
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

        var categoryExists = await _context.BusinessCategories
            .AnyAsync(c => c.Id == model.CategoryId);

        if (!categoryExists)
        {
            ModelState.AddModelError("CategoryId", "Geçersiz kategori seçimi.");
        }

        if (!ModelState.IsValid)
        {
            model.Categories = await _context.BusinessCategories.ToListAsync();
            return View(model);
        }

        profile.Name = model.Name;
        profile.Description = model.Description;
        profile.City = model.City;
        profile.CategoryId = model.CategoryId;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }


}