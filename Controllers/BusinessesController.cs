
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrganizationPlatform.Data;
using OrganizationPlatform.Models;
using OrganizationPlatform.Services;
using System.Security.Claims;

namespace OrganizationPlatform.Controllers;

public class BusinessesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly LocationService _locationService;

    public BusinessesController(
        ApplicationDbContext context,
        LocationService locationService)
    {
        _context = context;
        _locationService = locationService;
    }



    public async Task<IActionResult> Index(BusinessFilterViewModel filter)
    {
        // İlleri konum API'sinden al.
        filter.Provinces = await _locationService.GetProvincesAsync();

        // Seçilen ilin adını bul.
        var selectedProvince = filter.ProvinceId.HasValue
            ? filter.Provinces.FirstOrDefault(
                province => province.Id == filter.ProvinceId.Value)
            : null;

        filter.City = selectedProvince?.Name;

        // Seçilen ile ait ilçeleri API'den al.
        if (filter.ProvinceId.HasValue)
        {
            filter.Districts = await _locationService
                .GetDistrictsAsync(filter.ProvinceId.Value);
        }

        var query = _context.BusinessProfiles
            .AsNoTracking()
            .Include(business => business.Category)
            .Include(business => business.Photos)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var searchTerm = filter.SearchTerm.Trim();

            query = query.Where(business =>
                EF.Functions.ILike(business.Name, $"%{searchTerm}%") ||
                (business.Description != null &&
                 EF.Functions.ILike(
                     business.Description, $"%{searchTerm}%")));
        }

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(business =>
                business.CategoryId == filter.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.City))
        {
            query = query.Where(business =>
                business.City == filter.City);
        }

        if (!string.IsNullOrWhiteSpace(filter.District))
        {
            query = query.Where(business =>
                business.District == filter.District);
        }

        filter.Businesses = await query
            .OrderBy(business => business.Name)
            .ToListAsync();

        var favoriteBusinessIds = new HashSet<int>();
        ViewBag.CanFavorite = false;

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!string.IsNullOrEmpty(currentUserId))
        {
            var isCustomer = await _context.Users
                .AnyAsync(user =>
                    user.Id == currentUserId &&
                    user.AccountType == AccountType.Customer);

            if (isCustomer)
            {
                ViewBag.CanFavorite = true;
                favoriteBusinessIds = await _context.BusinessFavorites
                    .Where(favorite => favorite.ApplicationUserId == currentUserId)
                    .Select(favorite => favorite.BusinessProfileId)
                    .ToHashSetAsync();
            }
        }

        ViewBag.FavoriteBusinessIds = favoriteBusinessIds;

        filter.Categories = await _context.BusinessCategories
            .AsNoTracking()
            .OrderBy(category => category.Name)
            .ToListAsync();

        return View(filter);
    }



    public async Task<IActionResult> Details(int id)
    {
        var today = DateTime.Today;

        var business = await _context.BusinessProfiles
            .Include(b => b.Category)
            .Include(b => b.Photos)
            .Include(b => b.ApplicationUser)
            .Include(b => b.Pricings.Where(p => p.IsActive))
            .Include(b => b.Packages.Where(p => p.IsActive))
                .ThenInclude(p => p.Items)
            .Include(b => b.Campaigns.Where(c =>
                c.IsActive &&
                c.StartDate <= today &&
                c.EndDate >= today))
                .ThenInclude(c => c.BusinessPackage)
            .Include(b => b.Venues)
                .ThenInclude(venue => venue.Photos)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (business == null)
        {
            return NotFound();
        }

        ViewBag.CanFavorite = false;
        ViewBag.IsFavorite = false;

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!string.IsNullOrEmpty(currentUserId))
        {
            var isCustomer = await _context.Users
                .AnyAsync(user =>
                    user.Id == currentUserId &&
                    user.AccountType == AccountType.Customer);

            if (isCustomer)
            {
                ViewBag.CanFavorite = true;

                ViewBag.IsFavorite = await _context.BusinessFavorites
                    .AnyAsync(favorite =>
                        favorite.ApplicationUserId == currentUserId &&
                        favorite.BusinessProfileId == id);
            }
        }

        return View(business);
    }
}
