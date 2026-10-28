
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrganizationPlatform.Data;
using OrganizationPlatform.Models;
using OrganizationPlatform.Services;

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

        filter.Categories = await _context.BusinessCategories
            .AsNoTracking()
            .OrderBy(category => category.Name)
            .ToListAsync();

        return View(filter);
    }



    public async Task<IActionResult> Details(int id)
    {
        var business = await _context.BusinessProfiles
            .Include(b => b.Category)
            .Include(b => b.Photos)
            .Include(b => b.ApplicationUser)
            .Include(b => b.Venues)
                .ThenInclude(venue => venue.Photos)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (business == null)
        {
            return NotFound();
        }

        return View(business);
    }
}
