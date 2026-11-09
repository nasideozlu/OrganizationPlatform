using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrganizationPlatform.Data;
using OrganizationPlatform.Models;
using OrganizationPlatform.Helpers;

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

        var cleanedComment = string.IsNullOrWhiteSpace(comment)
    ? null
    : comment.Trim();

        var businessRating = new BusinessRating
        {
            ApplicationUserId = user.Id,
            BusinessProfileId = businessProfileId,
            Rating = rating,
            Comment = cleanedComment,

            ReviewStatus = ReviewModerationHelper.ContainsRestrictedWord(cleanedComment)
    ? ReviewStatus.Pending
    : ReviewStatus.Approved
        };

        _context.BusinessRatings.Add(businessRating);

        await _context.SaveChangesAsync();

        return RedirectToAction(
            "Details",
            "Businesses",
            new { id = businessProfileId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reply(int businessRatingId, string reply)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Forbid();
        }

        var businessRating = await _context.BusinessRatings
            .FirstOrDefaultAsync(rating => rating.Id == businessRatingId);

        if (businessRating == null)
        {
            return NotFound();
        }

        var business = await _context.BusinessProfiles
            .FirstOrDefaultAsync(profile =>
                profile.Id == businessRating.BusinessProfileId);

        if (business == null)
        {
            return NotFound();
        }

        // Yalnızca yorumun ait olduğu işletmenin sahibi cevap verebilir.
        if (business.ApplicationUserId != user.Id)
        {
            return Forbid();
        }

        if (businessRating.ReviewStatus != ReviewStatus.Approved ||
    string.IsNullOrWhiteSpace(reply))
        {
            return RedirectToAction(
                "Details",
                "Businesses",
                new { id = businessRating.BusinessProfileId });
        }

        var cleanedReply = reply.Trim();

        if (ReviewModerationHelper.ContainsRestrictedWord(cleanedReply))
        {
            TempData["Error"] = "Yanıtınız uygunsuz ifadeler içeriyor. Lütfen düzenleyin.";

            return RedirectToAction(
                "Details",
                "Businesses",
                new { id = businessRating.BusinessProfileId });
        }

        businessRating.BusinessReply = cleanedReply;
        businessRating.BusinessReplyAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return RedirectToAction(
            "Details",
            "Businesses",
            new { id = businessRating.BusinessProfileId });
    }
}