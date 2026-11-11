using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OrganizationPlatform.Data;
using OrganizationPlatform.Models;

namespace OrganizationPlatform.ViewComponents;

public class BusinessNavigationViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public BusinessNavigationViewComponent(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return Content(string.Empty);
        }

        var user = await _userManager.GetUserAsync(
            HttpContext.User);

        if (user == null ||
            user.AccountType != AccountType.Business)
        {
            return Content(string.Empty);
        }

        var businessProfile = await _context.BusinessProfiles
            .Include(business => business.Category)
            .FirstOrDefaultAsync(business =>
                business.ApplicationUserId == user.Id);

        if (businessProfile == null)
        {
            return Content(string.Empty);
        }

        if (businessProfile.Category.Name == "Düğün Salonu")
        {
            return View("WeddingHall");
        }

        return View("Default");
    }
}