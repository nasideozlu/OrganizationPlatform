using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrganizationPlatform.Data;
using OrganizationPlatform.Models;
using OrganizationPlatform.Services;
using System.Linq;
using OrganizationPlatform.ViewModels;

namespace OrganizationPlatform.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly PhotoStorageService _photoStorageService;
    private readonly LocationService _locationService;

    public ProfileController(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager,
    PhotoStorageService photoStorageService,
    LocationService locationService)
    {
        _context = context;
        _userManager = userManager;
        _photoStorageService = photoStorageService;
        _locationService = locationService;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Challenge();
        }

        if (user.AccountType == AccountType.Customer)
        {
            ViewBag.VenueCount = await _context.Venues.CountAsync();
            ViewBag.BusinessCount = await _context.BusinessProfiles
                .CountAsync(business => business.Category.Name != "Düğün Mekanı");
            var profile = await _context.CustomerProfiles
                .Include(p => p.ApplicationUser)
                .FirstOrDefaultAsync(p => p.ApplicationUserId == user.Id);

            if (profile == null)
            {
                return NotFound();
            }

            return View("CustomerProfile", profile);
        }

        if (user.AccountType == AccountType.Business)
        {
            var profile = await _context.BusinessProfiles
                .Include(p => p.Category)
                .Include(p => p.ApplicationUser)
                .Include(p => p.Photos)
                .FirstOrDefaultAsync(p => p.ApplicationUserId == user.Id);

            if (profile == null)
            {
                return NotFound();
            }

            ViewBag.VenueCount = await _context.Venues
                .CountAsync(venue => venue.BusinessProfileId == profile.Id);
            ViewBag.PricingCount = await _context.BusinessPricings
                .CountAsync(pricing => pricing.BusinessProfileId == profile.Id);
            ViewBag.PackageCount = await _context.BusinessPackages
                .CountAsync(package => package.BusinessProfileId == profile.Id);
            var ratings = await _context.BusinessRatings
                .Where(rating =>
                    rating.BusinessProfileId == profile.Id &&
                    rating.ReviewStatus == ReviewStatus.Approved)
                .OrderByDescending(rating => rating.CreatedAt)
                .Select(rating => new BusinessRatingViewModel
                {
                    Id = rating.Id,
                    Rating = rating.Rating,
                    Comment = rating.Comment,
                    CreatedAt = rating.CreatedAt,
                    CustomerName = _context.CustomerProfiles
        .Where(customer => customer.ApplicationUserId == rating.ApplicationUserId)
        .Select(customer => customer.Name + " " + customer.Surname)
        .FirstOrDefault() ?? "Müşteri",

                    BusinessReply = rating.BusinessReply,
                    BusinessReplyAt = rating.BusinessReplyAt
                })
                .ToListAsync();

            ViewBag.Ratings = ratings;

            ViewBag.RatingCount = ratings.Count;

            ViewBag.AverageRating = ratings.Count > 0
                ? ratings.Average(rating => rating.Rating)
                : 0;
            return View("BusinessProfile", profile);
        }

        return Forbid();
    }

    [HttpGet]
    public async Task<IActionResult> EditCustomer()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Challenge();
        }

        var profile = await _context.CustomerProfiles
            .FirstOrDefaultAsync(p => p.ApplicationUserId == user.Id);

        if (profile == null)
        {
            return NotFound();
        }

        return View(profile);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCustomer(
        string name,
        string surname,
        string phoneNumber)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Challenge();
        }

        var profile = await _context.CustomerProfiles
            .FirstOrDefaultAsync(p => p.ApplicationUserId == user.Id);

        if (profile == null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(surname) ||
            string.IsNullOrWhiteSpace(phoneNumber))
        {
            ModelState.AddModelError("", "Tüm alanları doldurmalısın.");

            profile.ApplicationUser = user;
            return View(profile);
        }

        profile.Name = name;
        profile.Surname = surname;
        user.PhoneNumber = phoneNumber;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }



    [HttpGet]
    public async Task<IActionResult> EditBusiness()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Challenge();
        }

        if (user.AccountType != AccountType.Business)
        {
            return Forbid();
        }

        var profile = await _context.BusinessProfiles
            .Include(p => p.Photos)
            .Include(p => p.FilterOptions)
                .ThenInclude(p => p.FilterOption)
            .Include(p => p.FilterValues)
            .FirstOrDefaultAsync(p => p.ApplicationUserId == user.Id);

        if (profile == null)
        {
            return NotFound();
        }

        var model = new BusinessProfileEditViewModel
        {
            Name = profile.Name,
            City = profile.City,
            District = profile.District,
            Description = profile.Description,
            PhoneNumber = profile.PhoneNumber,
            InstagramUrl = profile.InstagramUrl,
            Address = profile.Address,
            ExistingPhotos = profile.Photos
        };

        // İşletmenin kategorisine ait aktif filtreleri getir.
        var filterDefinitions = await _context.BusinessFilterDefinitions
            .Include(f => f.Options)
            .Where(f => f.BusinessCategoryId == profile.CategoryId && f.IsActive)
            .OrderBy(f => f.SortOrder)
            .ToListAsync();

        foreach (var definition in filterDefinitions)
        {
            // Bu filtreye ait daha önce kaydedilmiş değer.
            var savedValue = profile.FilterValues
                .FirstOrDefault(v => v.BusinessFilterDefinitionId == definition.Id);

            // Daha önce seçilmiş seçeneklerin ID'leri.
            var selectedOptionIds = profile.FilterOptions
                .Where(p => p.FilterOption.BusinessFilterDefinitionId == definition.Id)
                .Select(p => p.BusinessFilterOptionId)
                .ToList();

            model.FilterItems.Add(new BusinessProfileFilterEditItemViewModel
            {
                DefinitionId = definition.Id,
                Name = definition.Name,
                Code = definition.Code,
                DataType = definition.DataType,
                Unit = definition.Unit,
                IsRequired = definition.IsRequired,

                Options = definition.Options
                    .Where(o => o.IsActive || selectedOptionIds.Contains(o.Id))
                    .OrderBy(o => o.SortOrder)
                    .ToList(),

                SelectedOptionIds = selectedOptionIds,

                NumericValue = savedValue?.NumericValue,
                TextValue = savedValue?.TextValue,
                BooleanValue = savedValue?.BooleanValue
            });
        }

        ViewBag.Provinces = await _locationService.GetProvincesAsync();

        var selectedProvince = ((List<LocationItem>)ViewBag.Provinces)
            .FirstOrDefault(p => p.Name == model.City);

        ViewBag.Districts = selectedProvince is not null
            ? await _locationService.GetDistrictsAsync(selectedProvince.Id)
            : new List<LocationItem>();

        return View(model);
    }



    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditBusiness(BusinessProfileEditViewModel model)
    {
        Console.WriteLine("EditBusiness POST metoduna girildi.");
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Challenge();
        }

        if (user.AccountType != AccountType.Business)
        {
            return Forbid();
        }

        var profile = await _context.BusinessProfiles
        .Include(p => p.Photos)
        .Include(p => p.FilterOptions)
            .ThenInclude(p => p.FilterOption)
        .Include(p => p.FilterValues)
        .FirstOrDefaultAsync(p => p.ApplicationUserId == user.Id);
        if (profile == null)
        {
            return NotFound();
        }

        // İşletmenin kategorisine ait aktif filtre tanımlarını getir.
        var definitions = await _context.BusinessFilterDefinitions
            .Include(f => f.Options)
            .Where(f => f.BusinessCategoryId == profile.CategoryId && f.IsActive)
            .ToListAsync();

        // Formdan gelen filtreleri tanımlarıyla eşleştir.
        var postedItems = model.FilterItems
            .GroupBy(item => item.DefinitionId)
            .ToDictionary(group => group.Key, group => group.First());

        // Aynı filtre birden fazla gönderilmiş mi?
        if (postedItems.Count != model.FilterItems.Count)
        {
            ModelState.AddModelError("", "Aynı filtre birden fazla gönderilemez.");
        }

        // Formdan gelen filtrelerin kategoriye ait olup olmadığını kontrol et.
        foreach (var item in model.FilterItems)
        {
            var definition = definitions.FirstOrDefault(d => d.Id == item.DefinitionId);

            if (definition == null)
            {
                ModelState.AddModelError("", "Geçersiz filtre gönderildi.");
                continue;
            }

            if (item.DataType != definition.DataType)
            {
                ModelState.AddModelError("", "Filtre veri türü geçersiz.");
                continue;
            }

            switch (definition.DataType)
            {
                case BusinessFilterDataType.MultiSelect:
                    var validOptionIds = definition.Options
                        .Where(o => o.IsActive)
                        .Select(o => o.Id)
                        .ToHashSet();

                    var selectedIds = item.SelectedOptionIds ?? new List<int>();

                    if (selectedIds.Any(id => !validOptionIds.Contains(id)))
                    {
                        ModelState.AddModelError("", $"{definition.Name} için geçersiz seçenek seçildi.");
                    }

                    if (definition.IsRequired && selectedIds.Count == 0)
                    {
                        ModelState.AddModelError("", $"{definition.Name} alanı zorunludur.");
                    }
                    break;

                case BusinessFilterDataType.Number:
                    if (definition.IsRequired && !item.NumericValue.HasValue)
                    {
                        ModelState.AddModelError("", $"{definition.Name} alanı zorunludur.");
                    }
                    break;

                case BusinessFilterDataType.Text:
                    if (definition.IsRequired && string.IsNullOrWhiteSpace(item.TextValue))
                    {
                        ModelState.AddModelError("", $"{definition.Name} alanı zorunludur.");
                    }
                    break;

                case BusinessFilterDataType.Boolean:
                    if (definition.IsRequired && !item.BooleanValue.HasValue)
                    {
                        ModelState.AddModelError("", $"{definition.Name} alanı zorunludur.");
                    }
                    break;

                default:
                    ModelState.AddModelError("", "Geçersiz filtre veri türü.");
                    break;
            }
        }

        // Zorunlu bir filtre formdan hiç gönderilmemiş mi?
        foreach (var definition in definitions.Where(d => d.IsRequired))
        {
            if (!postedItems.ContainsKey(definition.Id))
            {
                ModelState.AddModelError("", $"{definition.Name} alanı zorunludur.");
            }
        }

        // Filtre doğrulaması başarısızsa formu yeniden hazırla.
        if (!ModelState.IsValid)
        {
            model.ExistingPhotos = profile.Photos;

            model.FilterItems = definitions.Select(definition =>
            {
                postedItems.TryGetValue(definition.Id, out var posted);

                return new BusinessProfileFilterEditItemViewModel
                {
                    DefinitionId = definition.Id,
                    Name = definition.Name,
                    Code = definition.Code,
                    DataType = definition.DataType,
                    Unit = definition.Unit,
                    IsRequired = definition.IsRequired,
                    Options = definition.Options
                        .Where(o => o.IsActive)
                        .OrderBy(o => o.SortOrder)
                        .ToList(),
                    SelectedOptionIds = posted?.SelectedOptionIds ?? new List<int>(),
                    NumericValue = posted?.NumericValue,
                    TextValue = posted?.TextValue,
                    BooleanValue = posted?.BooleanValue
                };
            }).ToList();

            await LoadBusinessLocationAsync(model.City);
            return View(model);
        }

        // Önceki filtre seçimlerini temizle.
        _context.BusinessProfileFilterOptions.RemoveRange(profile.FilterOptions);
        _context.BusinessProfileFilterValues.RemoveRange(profile.FilterValues);

        // Yeni filtre seçimlerini kaydet.
        foreach (var definition in definitions)
        {
            if (!postedItems.TryGetValue(definition.Id, out var item))
            {
                continue;
            }

            if (definition.DataType == BusinessFilterDataType.MultiSelect)
            {
                foreach (var optionId in item.SelectedOptionIds.Distinct())
                {
                    _context.BusinessProfileFilterOptions.Add(new BusinessProfileFilterOption
                    {
                        BusinessProfileId = profile.Id,
                        BusinessFilterOptionId = optionId
                    });
                }
            }
            else
            {
                var hasValue = definition.DataType switch
                {
                    BusinessFilterDataType.Number => item.NumericValue.HasValue,
                    BusinessFilterDataType.Text => !string.IsNullOrWhiteSpace(item.TextValue),
                    BusinessFilterDataType.Boolean => item.BooleanValue.HasValue,
                    _ => false
                };

                if (!hasValue)
                {
                    continue;
                }

                _context.BusinessProfileFilterValues.Add(new BusinessProfileFilterValue
                {
                    BusinessProfileId = profile.Id,
                    BusinessFilterDefinitionId = definition.Id,
                    NumericValue = definition.DataType == BusinessFilterDataType.Number
                        ? item.NumericValue : null,
                    TextValue = definition.DataType == BusinessFilterDataType.Text
                        ? item.TextValue : null,
                    BooleanValue = definition.DataType == BusinessFilterDataType.Boolean
                        ? item.BooleanValue : null
                });
            }
        }

        profile.Name = model.Name;
        profile.Description = model.Description;
        profile.City = model.City;
        profile.District = model.District;
        profile.PhoneNumber = model.PhoneNumber?.Trim();
        profile.InstagramUrl = model.InstagramUrl?.Trim();
        profile.Address = model.Address?.Trim();

        try
        {
            foreach (var photo in model.Photos?.Where(p => p.Length > 0) ?? Enumerable.Empty<IFormFile>())
            {
                var filePath = await _photoStorageService.SavePhotoAsync(
                    photo,
                    "businesses",
                    profile.Id);

                var businessPhoto = new BusinessPhoto
                {
                    FilePath = filePath,
                    BusinessProfileId = profile.Id,
                    BusinessProfile = profile
                };

                _context.BusinessPhotos.Add(businessPhoto);
            }
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(nameof(model.Photos), exception.Message);
            model.ExistingPhotos = profile.Photos;
            await LoadBusinessLocationAsync(model.City);
            return View(model);
        }

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));

    }
    [HttpGet]
    public async Task<IActionResult> AuthorizedPeople()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return Challenge();

        if (user.AccountType != AccountType.Business)
            return Forbid();

        var businessProfile = await _context.BusinessProfiles
            .FirstOrDefaultAsync(b => b.ApplicationUserId == user.Id);

        if (businessProfile == null)
            return NotFound();

        var people = await _context.AuthorizedPeople
            .Where(p => p.BusinessProfileId == businessProfile.Id)
            .OrderBy(p => p.Role)
            .ToListAsync();

        return View(people);
    }

    [HttpGet]
    public async Task<IActionResult> AddAuthorizedPerson()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return Challenge();

        if (user.AccountType != AccountType.Business)
            return Forbid();

        var businessProfileExists = await _context.BusinessProfiles
            .AnyAsync(b => b.ApplicationUserId == user.Id);

        if (!businessProfileExists)
            return NotFound();

        return View(new AuthorizedPersonViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddAuthorizedPerson(AuthorizedPersonViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return Challenge();

        if (user.AccountType != AccountType.Business)
            return Forbid();

        var businessProfile = await _context.BusinessProfiles
            .FirstOrDefaultAsync(b => b.ApplicationUserId == user.Id);

        if (businessProfile == null)
            return NotFound();

        if (!ModelState.IsValid)
            return View(model);

        var person = new AuthorizedPerson
        {
            FirstName = model.FirstName.Trim(),
            LastName = model.LastName.Trim(),
            Role = model.Role,
            PhoneNumber = model.PhoneNumber?.Trim(),
            Email = model.Email?.Trim(),
            BusinessProfileId = businessProfile.Id
        };

        try
        {
            if (model.Photo != null && model.Photo.Length > 0)
            {
                person.PhotoPath = await _photoStorageService.SavePhotoAsync(
                    model.Photo,
                    "authorized-people",
                    businessProfile.Id);
            }
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(nameof(model.Photo), exception.Message);
            return View(model);
        }

        _context.AuthorizedPeople.Add(person);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Yetkili kişi başarıyla eklendi.";

        return RedirectToAction(nameof(AuthorizedPeople));
    }

    [HttpGet]
    public async Task<IActionResult> EditAuthorizedPerson(int id)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return Challenge();

        if (user.AccountType != AccountType.Business)
            return Forbid();

        var businessProfile = await _context.BusinessProfiles
            .FirstOrDefaultAsync(b => b.ApplicationUserId == user.Id);

        if (businessProfile == null)
            return NotFound();

        var person = await _context.AuthorizedPeople
            .FirstOrDefaultAsync(p =>
                p.Id == id &&
                p.BusinessProfileId == businessProfile.Id);

        if (person == null)
            return NotFound();

        var model = new AuthorizedPersonViewModel
        {
            Id = person.Id,
            FirstName = person.FirstName,
            LastName = person.LastName,
            Role = person.Role,
            PhoneNumber = person.PhoneNumber,
            Email = person.Email,
            ExistingPhotoPath = person.PhotoPath
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditAuthorizedPerson(
    int id,
    AuthorizedPersonViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return Challenge();

        if (user.AccountType != AccountType.Business)
            return Forbid();

        var businessProfile = await _context.BusinessProfiles
            .FirstOrDefaultAsync(b => b.ApplicationUserId == user.Id);

        if (businessProfile == null)
            return NotFound();

        var person = await _context.AuthorizedPeople
            .FirstOrDefaultAsync(p =>
                p.Id == id &&
                p.BusinessProfileId == businessProfile.Id);

        if (person == null)
            return NotFound();

        if (!ModelState.IsValid)
        {
            model.ExistingPhotoPath = person.PhotoPath;
            return View(model);
        }

        person.FirstName = model.FirstName.Trim();
        person.LastName = model.LastName.Trim();
        person.Role = model.Role;
        person.PhoneNumber = model.PhoneNumber?.Trim();
        person.Email = model.Email?.Trim();

        string? oldPhotoPath = null;

        try
        {
            if (model.Photo != null && model.Photo.Length > 0)
            {
                var newPhotoPath = await _photoStorageService.SavePhotoAsync(
                    model.Photo,
                    "authorized-people",
                    businessProfile.Id);

                oldPhotoPath = person.PhotoPath;
                person.PhotoPath = newPhotoPath;
            }
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(nameof(model.Photo), exception.Message);
            model.ExistingPhotoPath = person.PhotoPath;
            return View(model);
        }

        await _context.SaveChangesAsync();

        if (oldPhotoPath != null)
        {
            await _photoStorageService.DeletePhotoAsync(
                oldPhotoPath,
                "authorized-people",
                businessProfile.Id);
        }

        TempData["Success"] = "Yetkili kişi bilgileri güncellendi.";

        return RedirectToAction(nameof(AuthorizedPeople));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAuthorizedPerson(int id)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return Challenge();

        if (user.AccountType != AccountType.Business)
            return Forbid();

        var businessProfile = await _context.BusinessProfiles
            .FirstOrDefaultAsync(b => b.ApplicationUserId == user.Id);

        if (businessProfile == null)
            return NotFound();

        var person = await _context.AuthorizedPeople
            .FirstOrDefaultAsync(p =>
                p.Id == id &&
                p.BusinessProfileId == businessProfile.Id);

        if (person == null)
            return NotFound();

        if (!string.IsNullOrWhiteSpace(person.PhotoPath))
        {
            await _photoStorageService.DeletePhotoAsync(
                person.PhotoPath,
                "authorized-people",
                businessProfile.Id);
        }

        _context.AuthorizedPeople.Remove(person);

        await _context.SaveChangesAsync();

        TempData["Success"] = "Yetkili kişi silindi.";

        return RedirectToAction(nameof(AuthorizedPeople));
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteBusinessPhoto(int id)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Challenge();
        }

        if (user.AccountType != AccountType.Business)
        {
            return Forbid();
        }

        var profile = await _context.BusinessProfiles
            .FirstOrDefaultAsync(p => p.ApplicationUserId == user.Id);

        if (profile == null)
        {
            return NotFound();
        }

        var photo = await _context.BusinessPhotos
            .FirstOrDefaultAsync(p =>
                p.Id == id &&
                p.BusinessProfileId == profile.Id);

        if (photo == null)
        {
            return NotFound();
        }

        await _photoStorageService.DeletePhotoAsync(
            photo.FilePath,
            "businesses",
            profile.Id);

        _context.BusinessPhotos.Remove(photo);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(EditBusiness));
    }

    private async Task LoadBusinessLocationAsync(string? city)
    {
        ViewBag.Provinces = await _locationService.GetProvincesAsync();

        var provinces =
            (List<LocationItem>)ViewBag.Provinces;

        var selectedProvince = provinces
            .FirstOrDefault(p => p.Name == city);

        ViewBag.Districts = selectedProvince != null
            ? await _locationService.GetDistrictsAsync(selectedProvince.Id)
            : new List<LocationItem>();
    }

}

