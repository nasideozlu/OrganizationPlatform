
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OrganizationPlatform.Data;
using OrganizationPlatform.Models;
using OrganizationPlatform.ViewModels;

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
    public async Task<IActionResult> ToggleFavorite(
        int businessProfileId,
        string? returnUrl)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return Challenge();

        if (user.AccountType != AccountType.Customer)
            return Forbid();

        var businessExists = await _context.BusinessProfiles
            .AnyAsync(business => business.Id == businessProfileId);

        if (!businessExists)
            return NotFound();

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
            _context.BusinessFavorites.Add(new BusinessFavorite
            {
                ApplicationUserId = user.Id,
                BusinessProfileId = businessProfileId,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(returnUrl) &&
            Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction(
            "Details",
            "Businesses",
            new { id = businessProfileId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleVenueFavorite(
        int venueId,
        string? returnUrl)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return Challenge();

        if (user.AccountType != AccountType.Customer)
            return Forbid();

        var venueExists = await _context.Venues
            .AnyAsync(venue => venue.Id == venueId);

        if (!venueExists)
            return NotFound();

        var existingFavorite = await _context.FavoriteVenues
            .FirstOrDefaultAsync(favorite =>
                favorite.ApplicationUserId == user.Id &&
                favorite.VenueId == venueId);

        if (existingFavorite != null)
        {
            _context.FavoriteVenues.Remove(existingFavorite);
        }
        else
        {
            _context.FavoriteVenues.Add(new FavoriteVenue
            {
                ApplicationUserId = user.Id,
                VenueId = venueId,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(returnUrl) &&
            Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction("Details", "Venues", new { id = venueId });
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return Challenge();

        if (user.AccountType != AccountType.Customer)
            return Forbid();

        var businessFavorites = await _context.BusinessFavorites
            .AsNoTracking()
            .Where(favorite => favorite.ApplicationUserId == user.Id)
            .Include(favorite => favorite.BusinessProfile)
                .ThenInclude(business => business.Category)
            .Include(favorite => favorite.BusinessProfile.Photos)
            .OrderByDescending(favorite => favorite.CreatedAt)
            .ToListAsync();

        var venueFavorites = await _context.FavoriteVenues
            .AsNoTracking()
            .Where(favorite => favorite.ApplicationUserId == user.Id)
            .Include(favorite => favorite.Venue)
                .ThenInclude(venue => venue.Photos)
            .OrderByDescending(favorite => favorite.CreatedAt)
            .ToListAsync();

        var model = new FavoritesViewModel
        {
            BusinessFavorites = businessFavorites,
            VenueFavorites = venueFavorites
        };

        return View(model);
    }
}
