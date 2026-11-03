using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrganizationPlatform.Data;
using OrganizationPlatform.Models;

namespace OrganizationPlatform.Controllers;

[Authorize]
public class BusinessRatingsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public BusinessRatingsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
    int businessProfileId,
    int rating,
    string? comment)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null || user.AccountType != AccountType.Customer)
        {
            return Forbid();
        }

        var businessExists = await _context.BusinessProfiles
            .AnyAsync(business => business.Id == businessProfileId);

        if (!businessExists)
        {
            return NotFound();
        }

        var alreadyRated = await _context.BusinessRatings
            .AnyAsync(businessRating =>
                businessRating.ApplicationUserId == user.Id &&
                businessRating.BusinessProfileId == businessProfileId);

        if (alreadyRated)
        {
            return RedirectToAction(
                "Details",
                "Businesses",
                new { id = businessProfileId });
        }

        if (rating < 1 || rating > 5)
        {
            return RedirectToAction(
                "Details",
                "Businesses",
                new { id = businessProfileId });
        }

        var businessRating = new BusinessRating
        {
            ApplicationUserId = user.Id,
            BusinessProfileId = businessProfileId,
            Rating = rating,
            Comment = string.IsNullOrWhiteSpace(comment)
                ? null
                : comment.Trim()
        };

        _context.BusinessRatings.Add(businessRating);

        await _context.SaveChangesAsync();

        return RedirectToAction(
            "Details",
            "Businesses",
            new { id = businessProfileId });
    }
}