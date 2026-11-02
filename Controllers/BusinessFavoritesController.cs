using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OrganizationPlatform.Data;
using OrganizationPlatform.Models;

namespace OrganizationPlatform.Controllers;

[Authorize]
public class BusinessFavoritesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public BusinessFavoritesController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleFavorite(int businessProfileId, string? returnUrl)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Challenge();
        }

        if (user.AccountType != AccountType.Customer)
        {
            return Forbid();
        }

        var businessExists = await _context.BusinessProfiles
            .AnyAsync(business => business.Id == businessProfileId);

        if (!businessExists)
        {
            return NotFound();
        }

        var existingFavorite = await _context.BusinessFavorites
            .FirstOrDefaultAsync(favorite =>
                favorite.ApplicationUserId == user.Id &&
                favorite.BusinessProfileId == businessProfileId);

        if (existingFavorite != null)
        {
            _context.BusinessFavorites.Remove(existingFavorite);
        }
        else
        {
            var favorite = new BusinessFavorite
            {
                ApplicationUserId = user.Id,
                BusinessProfileId = businessProfileId,
                CreatedAt = DateTime.UtcNow
            };

            _context.BusinessFavorites.Add(favorite);
        }

        await _context.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction(
            "Details",
            "Businesses",
            new { id = businessProfileId });
    }
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Challenge();
        }

        if (user.AccountType != AccountType.Customer)
        {
            return Forbid();
        }

        var favorites = await _context.BusinessFavorites
            .AsNoTracking()
            .Where(favorite => favorite.ApplicationUserId == user.Id)
            .Include(favorite => favorite.BusinessProfile)
                .ThenInclude(business => business.Category)
            .Include(favorite => favorite.BusinessProfile.Photos)
            .OrderByDescending(favorite => favorite.CreatedAt)
            .ToListAsync();

        return View(favorites);
    }
}
