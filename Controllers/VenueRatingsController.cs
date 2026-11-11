using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrganizationPlatform.Data;
using OrganizationPlatform.Helpers;
using OrganizationPlatform.Models;

namespace OrganizationPlatform.Controllers;

[Authorize]
public class VenueRatingsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public VenueRatingsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
    int venueId,
    int rating,
    string? comment)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null || user.AccountType != AccountType.Customer)
        {
            return Forbid();
        }

        var venueExists = await _context.Venues
            .AnyAsync(venue => venue.Id == venueId);

        if (!venueExists)
        {
            return NotFound();
        }

        var alreadyRated = await _context.VenueRatings
            .AnyAsync(venueRating =>
                venueRating.ApplicationUserId == user.Id &&
                venueRating.VenueId == venueId);

        if (alreadyRated || rating < 1 || rating > 5)
        {
            return RedirectToAction(
                "Details",
                "Venues",
                new { id = venueId });
        }

        var cleanedComment = string.IsNullOrWhiteSpace(comment)
            ? null
            : comment.Trim();

        var venueRating = new VenueRating
        {
            ApplicationUserId = user.Id,
            VenueId = venueId,
            Rating = rating,
            Comment = cleanedComment,

            ReviewStatus = ReviewModerationHelper.ContainsRestrictedWord(cleanedComment)
    ? ReviewStatus.Pending
    : ReviewStatus.Approved
        };

        _context.VenueRatings.Add(venueRating);

        await _context.SaveChangesAsync();

        return RedirectToAction(
            "Details",
            "Venues",
            new { id = venueId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reply(int venueRatingId, string reply)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Forbid();
        }

        var venueRating = await _context.VenueRatings
            .Include(rating => rating.Venue)
                .ThenInclude(venue => venue.BusinessProfile)
            .FirstOrDefaultAsync(rating => rating.Id == venueRatingId);

        if (venueRating == null)
        {
            return NotFound();
        }

        // Yalnızca ilgili mekânın işletme sahibi cevap verebilir.
        if (venueRating.Venue.BusinessProfile.ApplicationUserId != user.Id)
        {
            return Forbid();
        }

        if (venueRating.ReviewStatus != ReviewStatus.Approved ||
    string.IsNullOrWhiteSpace(reply))
        {
            return RedirectToAction(
                "Details",
                "Venues",
                new { id = venueRating.VenueId });
        }

        var cleanedReply = reply.Trim();

        if (ReviewModerationHelper.ContainsRestrictedWord(cleanedReply))
        {
            TempData["Error"] = "Yanıtınız uygunsuz ifadeler içeriyor. Lütfen düzenleyin.";

            return RedirectToAction(
                "Details",
                "Venues",
                new { id = venueRating.VenueId });
        }
        venueRating.BusinessReply = cleanedReply;
        venueRating.BusinessReplyAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return RedirectToAction(
            "Details",
            "Venues",
            new { id = venueRating.VenueId });
    }
}