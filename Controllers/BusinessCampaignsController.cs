
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrganizationPlatform.Data;
using OrganizationPlatform.Models;

namespace OrganizationPlatform.Controllers;

[Authorize]
public class BusinessCampaignsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public BusinessCampaignsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    private async Task<BusinessProfile?> GetCurrentBusinessAsync()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null || user.AccountType != AccountType.Business)
        {
            return null;
        }

        return await _context.BusinessProfiles
            .FirstOrDefaultAsync(profile =>
                profile.ApplicationUserId == user.Id);
    }

    public async Task<IActionResult> Index()
    {
        var business = await GetCurrentBusinessAsync();

        if (business == null)
        {
            return Forbid();
        }

        var campaigns = await _context.BusinessCampaigns
            .AsNoTracking()
            .Include(campaign => campaign.BusinessPackage)
            .Where(campaign =>
                campaign.BusinessProfileId == business.Id)
            .OrderByDescending(campaign => campaign.StartDate)
            .ToListAsync();

        return View(campaigns);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var business = await GetCurrentBusinessAsync();

        if (business == null)
        {
            return Forbid();
        }

        var packages = await _context.BusinessPackages
            .AsNoTracking()
            .Where(package =>
                package.BusinessProfileId == business.Id &&
                package.IsActive)
            .OrderBy(package => package.Name)
            .ToListAsync();

        ViewBag.Packages = packages;

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        string name,
        string? description,
        DateTime startDate,
        DateTime endDate,
        BusinessCampaignType campaignType,
        DiscountType discountType,
        decimal discountValue,
        int? businessPackageId)
    {
        var business = await GetCurrentBusinessAsync();

        if (business == null)
        {
            return Forbid();
        }

        // Temel kontroller
        if (string.IsNullOrWhiteSpace(name))
        {
            ModelState.AddModelError(
                nameof(name),
                "Kampanya adı zorunludur.");
        }

        if (endDate < startDate)
        {
            ModelState.AddModelError(
                nameof(endDate),
                "Bitiş tarihi başlangıç tarihinden önce olamaz.");
        }

        if (discountValue <= 0)
        {
            ModelState.AddModelError(
                nameof(discountValue),
                "İndirim değeri 0'dan büyük olmalıdır.");
        }

        // Yüzde indirim kontrolü
        if (discountType == DiscountType.Percentage &&
            discountValue > 100)
        {
            ModelState.AddModelError(
                nameof(discountValue),
                "Yüzde indirimi 100'den büyük olamaz.");
        }

        // Paket kampanyası kontrolü
        if (campaignType == BusinessCampaignType.Package)
        {
            if (!businessPackageId.HasValue)
            {
                ModelState.AddModelError(
                    nameof(businessPackageId),
                    "Paket bazlı kampanya için bir paket seçmelisiniz.");
            }
            else
            {
                var selectedPackage = await _context.BusinessPackages
                    .FirstOrDefaultAsync(package =>
                        package.Id == businessPackageId.Value &&
                        package.BusinessProfileId == business.Id);

                if (selectedPackage == null)
                {
                    ModelState.AddModelError(
                        nameof(businessPackageId),
                        "Geçerli bir paket seçmelisiniz.");
                }
                else
                {
                    if
                (discountType == DiscountType.FixedAmount &&
                    discountValue >= selectedPackage.Amount)
                    {
                        ModelState.AddModelError(
                            nameof(discountValue),
                            "Sabit indirim paket fiyatından küçük olmalıdır.");
                    }


                    var hasOverlappingCampaign = await _context.BusinessCampaigns
                        .AnyAsync(campaign =>
                            campaign.BusinessProfileId == business.Id &&
                            campaign.BusinessPackageId == businessPackageId.Value &&
                            campaign.CampaignType == BusinessCampaignType.Package &&
                            campaign.IsActive &&
                            campaign.StartDate <= endDate &&
                            campaign.EndDate >= startDate);

                    if (hasOverlappingCampaign)
                    {
                        ModelState.AddModelError(
                            nameof(businessPackageId),
                            "Bu paketin seçilen tarihlerinde zaten aktif bir kampanya bulunuyor.");
                    }
                }
            }
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Packages = await _context.BusinessPackages
                .AsNoTracking()
                .Where(package =>
                    package.BusinessProfileId == business.Id &&
                    package.IsActive)
                .OrderBy(package => package.Name)
                .ToListAsync();

            return View();
        }

        var campaign = new BusinessCampaign
        {
            Name = name.Trim(),

            Description = string.IsNullOrWhiteSpace(description)
                ? null
                : description.Trim(),

            StartDate = startDate,
            EndDate = endDate,

            CampaignType = campaignType,
            DiscountType = discountType,
            DiscountValue = discountValue,

            BusinessProfileId = business.Id,
            BusinessProfile = business,

            BusinessPackageId =
                campaignType == BusinessCampaignType.Package
                    ? businessPackageId
                    : null
        };

        _context.BusinessCampaigns.Add(campaign);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var business = await GetCurrentBusinessAsync();

        if (business == null)
        {
            return Forbid();
        }

        var campaign = await _context.BusinessCampaigns
            .FirstOrDefaultAsync(campaign =>
                campaign.Id == id &&
                campaign.BusinessProfileId == business.Id);

        if (campaign == null)
        {
            return NotFound();
        }

        ViewBag.Packages = await _context.BusinessPackages
            .Where(package =>
                package.BusinessProfileId == business.Id &&
                package.IsActive)
            .OrderBy(package => package.Name)
            .ToListAsync();

        return View(campaign);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id,
        BusinessCampaign model)
    {
        var business = await GetCurrentBusinessAsync();

        if (business == null)
        {
            return Forbid();
        }

        var campaign = await _context.BusinessCampaigns
            .FirstOrDefaultAsync(campaign =>
                campaign.Id == id &&
                campaign.BusinessProfileId == business.Id);

        if (campaign == null)
        {
            return NotFound();
        }

        // Temel kontroller
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            ModelState.AddModelError(
                nameof(model.Name),
                "Kampanya adı zorunludur.");
        }

        if (model.DiscountValue <= 0)
        {
            ModelState.AddModelError(
                nameof(model.DiscountValue),
                "İndirim değeri 0'dan büyük olmalıdır.");
        }

        // Yüzde indirim kontrolü
        if (model.DiscountType == DiscountType.Percentage &&
            model.DiscountValue > 100)
        {
            ModelState.AddModelError(
                nameof(model.DiscountValue),
                "Yüzde indirimi 100'den büyük olamaz.");
        }

        if (model.StartDate > model.EndDate)
        {
            ModelState.AddModelError(
                nameof(model.EndDate),
                "Başlangıç tarihi bitiş tarihinden sonra olamaz.");
        }

        // Paket kampanyası kontrolü

        if (model.CampaignType == BusinessCampaignType.Package)
        {
            if (!model.BusinessPackageId.HasValue)
            {
                ModelState.AddModelError(
                    nameof(model.BusinessPackageId),
                    "Paket kampanyası için bir paket seçmelisiniz.");
            }
            else
            {
                var selectedPackage = await _context.BusinessPackages
                    .FirstOrDefaultAsync(package =>
                        package.Id == model.BusinessPackageId.Value &&
                        package.BusinessProfileId == business.Id);

                if (selectedPackage == null)
                {
                    ModelState.AddModelError(
                        nameof(model.BusinessPackageId),
                        "Geçerli bir paket seçmelisiniz.");
                }
                else
                {
                    if (model.DiscountType == DiscountType.FixedAmount &&
                        model.DiscountValue >= selectedPackage.Amount)
                    {
                        ModelState.AddModelError(
                            nameof(model.DiscountValue),
                            "Sabit indirim paket fiyatından küçük olmalıdır.");
                    }

                    var hasOverlappingCampaign =
                        await _context.BusinessCampaigns.AnyAsync(
                            existingCampaign =>
                                existingCampaign.Id != id &&
                                existingCampaign.BusinessProfileId == business.Id &&
                                existingCampaign.BusinessPackageId == model.BusinessPackageId.Value &&
                                existingCampaign.CampaignType == BusinessCampaignType.Package &&
                                existingCampaign.IsActive &&
                                existingCampaign.StartDate <= model.EndDate &&
                                existingCampaign.EndDate >= model.StartDate);

                    if (hasOverlappingCampaign)
                    {
                        ModelState.AddModelError(
                            nameof(model.BusinessPackageId),
                            "Bu paketin seçilen tarihlerinde zaten aktif bir kampanya bulunuyor.");
                    }
                }
            }
        }


        if (!ModelState.IsValid)
        {
            ViewBag.Packages = await _context.BusinessPackages
                .AsNoTracking()
                .Where(package =>
                    package.BusinessProfileId == business.Id &&
                    package.IsActive)
                .OrderBy(package => package.Name)
                .ToListAsync();

            return View(model);
        }

        campaign.Name = model.Name.Trim();

        campaign.Description =
            string.IsNullOrWhiteSpace(model.Description)
                ? null
                : model.Description.Trim();

        campaign.CampaignType = model.CampaignType;

        campaign.BusinessPackageId =
            model.CampaignType == BusinessCampaignType.Package
                ? model.BusinessPackageId
                : null;

        campaign.DiscountType = model.DiscountType;
        campaign.DiscountValue = model.DiscountValue;
        campaign.StartDate = model.StartDate;
        campaign.EndDate = model.EndDate;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var business = await GetCurrentBusinessAsync();

        if (business == null)
        {
            return Forbid();
        }

        var campaign = await _context.BusinessCampaigns
            .FirstOrDefaultAsync(campaign =>
                campaign.Id == id &&
                campaign.BusinessProfileId == business.Id);

        if (campaign == null)
        {
            return NotFound();
        }

        campaign.IsActive = !campaign.IsActive;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
