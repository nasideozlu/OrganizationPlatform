using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrganizationPlatform.Data;
using OrganizationPlatform.Models;

namespace OrganizationPlatform.ViewComponents;

public class AccountNavigationViewComponent : ViewComponent
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public AccountNavigationViewComponent(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        if (User.Identity?.IsAuthenticated != true)
            return Content(string.Empty);

        if (HttpContext.User.IsInRole("Admin"))
            return View("Admin");

        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user is null)
            return Content(string.Empty);

        if (user.AccountType == AccountType.Business)
        {
            ViewBag.IsVenueBusiness = await _context.BusinessProfiles
                .Include(profile => profile.Category)
                .AnyAsync(profile => profile.ApplicationUserId == user.Id
                    && profile.Category.Name == "Düğün Mekanı");
            return View("Business");
        }

        return View("Customer");
    }
}
