
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrganizationPlatform.Data;

namespace OrganizationPlatform.Controllers;

public class BusinessesController : Controller
{
    private readonly ApplicationDbContext _context;

    public BusinessesController(ApplicationDbContext context)
    {
        _context = context;
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
