using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OrganizationPlatform.Models;
using OrganizationPlatform.Data;
using Microsoft.EntityFrameworkCore;

namespace OrganizationPlatform.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _context;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ApplicationDbContext context)

    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
    }

    public IActionResult Register()
    {
        return View();
    }
    public IActionResult Login()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> BusinessRegister()
    {
        var categories = await _context.BusinessCategories.ToListAsync();

        var model = new BusinessRegisterViewModel
        {
            Categories = categories
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);

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

        var user = new ApplicationUser
        {
            UserName = model.Email,
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
            model.Categories = await _context.BusinessCategories.ToListAsync();
            return View(model);
        }

        var categoryExists = await _context.BusinessCategories
            .AnyAsync(c => c.Id == model.CategoryId);

        if (!categoryExists)
        {
            ModelState.AddModelError("CategoryId", "Lütfen geçerli bir işletme kategorisi seçin.");
            model.Categories = await _context.BusinessCategories.ToListAsync();
            return View(model);
        }
        var user = new ApplicationUser
        {
            UserName = model.Email,
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

            model.Categories = await _context.BusinessCategories.ToListAsync();
            return View(model);
        }
        var category = await _context.BusinessCategories
    .FirstAsync(c => c.Id == model.CategoryId);

        var businessProfile = new BusinessProfile
        {
            Name = model.Name,
            City = model.City,
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
}
