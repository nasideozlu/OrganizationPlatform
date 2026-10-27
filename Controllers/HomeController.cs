
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrganizationPlatform.Data;
using OrganizationPlatform.Models;

namespace OrganizationPlatform.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }


    public async Task<IActionResult> Index()
    {
        var venues = await _context.Venues
            .Include(venue => venue.Photos)
            .OrderBy(venue => venue.City)
            .ThenBy(venue => venue.Name)
            .ToListAsync();

        var businesses = await _context.BusinessProfiles
            .Include(business => business.Category)
            .Include(business => business.Photos)
            .Where(business => business.Category.Name != "Düğün Salonu")
            .OrderBy(business => business.Category.Name)
            .ThenBy(business => business.Name)
            .ToListAsync();

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

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}
