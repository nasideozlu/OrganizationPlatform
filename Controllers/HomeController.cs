
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using OrganizationPlatform.Data;
using OrganizationPlatform.Models;

namespace OrganizationPlatform.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            if (User.IsInRole("Admin"))
                return RedirectToAction("Index", "Admin");

            var user = await _userManager.GetUserAsync(User);

            if (user?.AccountType == AccountType.Business)
                return RedirectToAction("Index", "Profile");

            if (user?.AccountType == AccountType.Customer)
            {
                ViewBag.IsCustomerHome = true;

                ViewBag.CustomerName = await _context.CustomerProfiles
                    .Where(profile => profile.ApplicationUserId == user.Id)
                    .Select(profile => profile.Name)
                    .FirstOrDefaultAsync();
            }
        }

        var venues = await _context.Venues
            .Include(venue => venue.Photos)
            .OrderBy(venue => venue.City)
            .ThenBy(venue => venue.Name)
            .ToListAsync();

        var businesses = await _context.BusinessProfiles
            .Include(business => business.Category)
            .Include(business => business.Photos)
            .Include(business => business.Campaigns.Where(campaign =>
                campaign.IsActive &&
                campaign.StartDate <= DateTime.Today &&
                campaign.EndDate >= DateTime.Today))
            .Where(business => business.Category.Name != "Düğün Mekanı")
            .OrderBy(business => business.Category.Name)
            .ThenBy(business => business.Name)
            .ToListAsync();

        var favoriteBusinessIds = new HashSet<int>();
        var favoriteVenueIds = new HashSet<int>();

        ViewBag.CanFavorite = false;

        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user?.AccountType == AccountType.Customer)
            {
                ViewBag.CanFavorite = true;

                favoriteBusinessIds = await _context.BusinessFavorites
                    .Where(favorite => favorite.ApplicationUserId == user.Id)
                    .Select(favorite => favorite.BusinessProfileId)
                    .ToHashSetAsync();

                favoriteVenueIds = await _context.FavoriteVenues
                    .Where(favorite => favorite.ApplicationUserId == user.Id)
                    .Select(favorite => favorite.VenueId)
                    .ToHashSetAsync();
            }
        }

        ViewBag.FavoriteBusinessIds = favoriteBusinessIds;
        ViewBag.FavoriteVenueIds = favoriteVenueIds;

        var model = new HomeViewModel
        {
            Venues = venues,
            Businesses = businesses
        };

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(
        Duration = 0,
        Location = ResponseCacheLocation.None,
        NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}
