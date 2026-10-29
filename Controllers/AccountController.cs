using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OrganizationPlatform.Models;
using OrganizationPlatform.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using OrganizationPlatform.Services;

namespace OrganizationPlatform.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _context;
    private readonly LocationService _locationService;

    public AccountController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ApplicationDbContext context,
    LocationService locationService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _locationService = locationService;
    }

    public IActionResult Register()
    {
        return View();
    }
    public IActionResult Login(string audience = "customer")
    {
        ViewBag.Audience = audience == "business" ? "business" : "customer";
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> BusinessRegister()
    {
        var model = new BusinessRegisterViewModel();
        await LoadBusinessRegistrationOptionsAsync(model);

        return View(model);
    }


    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model, string audience = "customer")
    {
        var accountType = audience == "business"
            ? AccountType.Business
            : AccountType.Customer;

        ViewBag.Audience = audience == "business" ? "business" : "customer";

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _context.Users.FirstOrDefaultAsync(u =>
            u.Email == model.Email &&
            u.AccountType == accountType);

        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "E-posta veya parola hatalı.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            user,
            model.Password,
            isPersistent: false,
            lockoutOnFailure: false);

        if (result.Succeeded)
        {
            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, "E-posta veya parola hatalı.");
        return View(model);
    }


    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var normalizedEmail = _userManager.NormalizeEmail(model.Email);

        var customerExists = await _context.Users.AnyAsync(u =>
            u.NormalizedEmail == normalizedEmail &&
            u.AccountType == AccountType.Customer);

        if (customerExists)
        {
            ModelState.AddModelError(
                nameof(model.Email),
                "Bu e-posta adresiyle bir müşteri hesabı zaten mevcut.");

            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = $"customer-{model.Email}",
            Email = model.Email,
            PhoneNumber = model.PhoneNumber,
            AccountType = AccountType.Customer
        };
        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            var profile = new CustomerProfile
            {
                Name = model.Name,
                Surname = model.Surname,
                ApplicationUserId = user.Id,
                ApplicationUser = user
            };
            _context.CustomerProfiles.Add(profile);
            await _context.SaveChangesAsync();

            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Home");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);

    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> BusinessRegister(BusinessRegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadBusinessRegistrationOptionsAsync(model);
            return View(model);
        }

        var categoryExists = await _context.BusinessCategories
            .AnyAsync(c => c.Id == model.CategoryId);

        if (!categoryExists)
        {
            ModelState.AddModelError("CategoryId", "Lütfen geçerli bir işletme kategorisi seçin.");
            await LoadBusinessRegistrationOptionsAsync(model);
            return View(model);

        }

        var normalizedEmail = _userManager.NormalizeEmail(model.Email);

        var businessExists = await _context.Users.AnyAsync(u =>
            u.NormalizedEmail == normalizedEmail &&
            u.AccountType == AccountType.Business);

        if (businessExists)
        {
            ModelState.AddModelError(
                nameof(model.Email),
                "Bu e-posta adresiyle bir işletme hesabı zaten mevcut.");

            await LoadBusinessRegistrationOptionsAsync(model);
            return View(model);
        }
        var user = new ApplicationUser
        {
            UserName = $"business-{model.Email}",
            Email = model.Email,
            PhoneNumber = model.PhoneNumber,
            AccountType = AccountType.Business
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            await LoadBusinessRegistrationOptionsAsync(model);
            return View(model);
        }
        var category = await _context.BusinessCategories
    .FirstAsync(c => c.Id == model.CategoryId);

        var businessProfile = new BusinessProfile
        {
            Name = model.Name,
            City = model.City,
            District = model.District,
            CategoryId = model.CategoryId,
            Category = category,
            ApplicationUserId = user.Id,
            ApplicationUser = user
        };

        _context.BusinessProfiles.Add(businessProfile);
        await _context.SaveChangesAsync();
        await _signInManager.SignInAsync(user, isPersistent: false);

        return RedirectToAction("Index", "Home");
    }

    private async Task LoadBusinessRegistrationOptionsAsync(BusinessRegisterViewModel model)
    {
        model.Categories = await _context.BusinessCategories.ToListAsync();

        var provinces = await _locationService.GetProvincesAsync();
        ViewBag.Provinces = provinces;

        var selectedProvince = provinces.FirstOrDefault(province => province.Name == model.City);
        ViewBag.Districts = selectedProvince is not null
            ? await _locationService.GetDistrictsAsync(selectedProvince.Id)
            : new List<LocationItem>();
    }
}
