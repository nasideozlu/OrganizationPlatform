
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrganizationPlatform.Data;
using OrganizationPlatform.Models;

namespace OrganizationPlatform.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Yönetici ana sayfası: kategorileri listele
    public async Task<IActionResult> Index()
    {
        var categories = await _context.BusinessCategories
            .OrderBy(category => category.Name)
            .ToListAsync();

        return View(categories);
    }
    // İşletme filtrelerini listele
    [HttpGet]
    public async Task<IActionResult> BusinessFilters()
    {
        var filters = await _context.BusinessFilterDefinitions
            .Include(filter => filter.BusinessCategory)
            .Include(filter => filter.Options)
            .OrderBy(filter => filter.BusinessCategory.Name)
            .ThenBy(filter => filter.SortOrder)
            .ToListAsync();

        return View(filters);
    }
    // Yeni işletme filtresi formu
    [HttpGet]
    public async Task<IActionResult> CreateBusinessFilter()
    {
        await LoadBusinessCategories();
        return View();
    }

    // Yeni işletme filtresini kaydet
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateBusinessFilter(
        string name,
        string code,
        BusinessFilterDataType dataType,
        string? unit,
        int businessCategoryId,
        int sortOrder,
        bool isRequired)
    {
        name = name?.Trim() ?? "";
        code = code?.Trim().ToLowerInvariant() ?? "";
        unit = unit?.Trim();

        if (string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(code))
        {
            TempData["Error"] = "Filtre adı ve kodu zorunludur.";
            return RedirectToAction(nameof(CreateBusinessFilter));
        }

        if (!Enum.IsDefined(typeof(BusinessFilterDataType), dataType))
        {
            TempData["Error"] = "Geçersiz filtre türü.";
            return RedirectToAction(nameof(CreateBusinessFilter));
        }

        var category = await _context.BusinessCategories
            .FirstOrDefaultAsync(c =>
                c.Id == businessCategoryId && c.Id != 1);

        if (category == null)
        {
            TempData["Error"] = "Geçerli bir işletme kategorisi seçmelisiniz.";
            return RedirectToAction(nameof(CreateBusinessFilter));
        }

        bool codeExists = await _context.BusinessFilterDefinitions
            .AnyAsync(f =>
                f.BusinessCategoryId == businessCategoryId &&
                f.Code == code);

        if (codeExists)
        {
            TempData["Error"] = "Bu kategoride aynı kodla bir filtre zaten mevcut.";
            return RedirectToAction(nameof(CreateBusinessFilter));
        }

        var filter = new BusinessFilterDefinition
        {
            Name = name,
            Code = code,
            DataType = dataType,
            Unit = string.IsNullOrWhiteSpace(unit) ? null : unit,
            BusinessCategoryId = businessCategoryId,
            SortOrder = sortOrder,
            IsRequired = isRequired,
            IsActive = true
        };

        _context.BusinessFilterDefinitions.Add(filter);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Filtre başarıyla oluşturuldu.";
        return RedirectToAction(nameof(BusinessFilters));
    }
    // Filtre düzenleme formu
    [HttpGet]
    public async Task<IActionResult> EditBusinessFilter(int id)
    {
        var filter = await _context.BusinessFilterDefinitions
            .FirstOrDefaultAsync(f => f.Id == id);

        if (filter == null)
            return NotFound();

        await LoadBusinessCategories();

        return View(filter);
    }

    // Filtre güncelleme
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditBusinessFilter(
        int id,
        string name,
        string code,
        BusinessFilterDataType dataType,
        string? unit,
        int businessCategoryId,
        int sortOrder,
        bool isRequired,
        bool isActive)
    {
        var filter = await _context.BusinessFilterDefinitions
            .FirstOrDefaultAsync(f => f.Id == id);

        if (filter == null)
            return NotFound();

        name = name?.Trim() ?? "";
        code = code?.Trim().ToLowerInvariant() ?? "";
        unit = unit?.Trim();

        if (string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(code))
        {
            TempData["Error"] = "Filtre adı ve kodu zorunludur.";
            return RedirectToAction(nameof(EditBusinessFilter), new { id });
        }

        if (!Enum.IsDefined(typeof(BusinessFilterDataType), dataType))
        {
            TempData["Error"] = "Geçersiz filtre türü.";
            return RedirectToAction(nameof(EditBusinessFilter), new { id });
        }

        var categoryExists = await _context.BusinessCategories
            .AnyAsync(c => c.Id == businessCategoryId && c.Id != 1);

        if (!categoryExists)
        {
            TempData["Error"] = "Geçerli bir işletme kategorisi seçmelisiniz.";
            return RedirectToAction(nameof(EditBusinessFilter), new { id });
        }

        bool duplicateExists = await _context.BusinessFilterDefinitions
            .AnyAsync(f =>
                f.Id != id &&
                f.BusinessCategoryId == businessCategoryId &&
                f.Code == code);

        if (duplicateExists)
        {
            TempData["Error"] = "Bu kategoride aynı kodla başka bir filtre var.";
            return RedirectToAction(nameof(EditBusinessFilter), new { id });
        }

        filter.Name = name;
        filter.Code = code;
        filter.DataType = dataType;
        filter.Unit = string.IsNullOrWhiteSpace(unit) ? null : unit;
        filter.BusinessCategoryId = businessCategoryId;
        filter.SortOrder = sortOrder;
        filter.IsRequired = isRequired;
        filter.IsActive = isActive;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Filtre başarıyla güncellendi.";
        return RedirectToAction(nameof(BusinessFilters));
    }
    // Filtrenin seçeneklerini listele
    [HttpGet]
    public async Task<IActionResult> ManageFilterOptions(int id)
    {
        var filter = await _context.BusinessFilterDefinitions
            .Include(f => f.BusinessCategory)
            .Include(f => f.Options.OrderBy(o => o.SortOrder))
            .FirstOrDefaultAsync(f => f.Id == id);

        if (filter == null)
            return NotFound();

        return View(filter);
    }
    // Yeni filtre seçeneği formu
    [HttpGet]
    public async Task<IActionResult> CreateFilterOption(int filterId)
    {
        var filter = await _context.BusinessFilterDefinitions
            .FirstOrDefaultAsync(f => f.Id == filterId);

        if (filter == null)
            return NotFound();

        if (filter.DataType != BusinessFilterDataType.MultiSelect)
            return BadRequest("Bu filtre seçenek listesi kullanmıyor.");

        ViewBag.Filter = filter;

        return View();
    }

    // Yeni filtre seçeneğini kaydet
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateFilterOption(
        int filterId,
        string name,
        string value,
        int sortOrder)
    {
        var filter = await _context.BusinessFilterDefinitions
            .FirstOrDefaultAsync(f => f.Id == filterId);

        if (filter == null)
            return NotFound();

        if (filter.DataType != BusinessFilterDataType.MultiSelect)
            return BadRequest("Bu filtre seçenek listesi kullanmıyor.");

        name = name?.Trim() ?? "";
        value = value?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(value))
        {
            TempData["Error"] = "Seçenek adı ve değeri zorunludur.";
            return RedirectToAction(nameof(CreateFilterOption), new { filterId });
        }

        bool exists = await _context.BusinessFilterOptions
            .AnyAsync(o =>
                o.BusinessFilterDefinitionId == filterId &&
                o.Value == value);

        if (exists)
        {
            TempData["Error"] = "Bu seçenek değeri zaten mevcut.";
            return RedirectToAction(nameof(CreateFilterOption), new { filterId });
        }

        var option = new BusinessFilterOption
        {
            Name = name,
            Value = value,
            SortOrder = sortOrder,
            IsActive = true,
            BusinessFilterDefinitionId = filterId
        };

        _context.BusinessFilterOptions.Add(option);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Seçenek başarıyla eklendi.";

        return RedirectToAction(nameof(ManageFilterOptions), new { id = filterId });
    }
    // Seçenek düzenleme formu
    [HttpGet]
    public async Task<IActionResult> EditFilterOption(int id)
    {
        var option = await _context.BusinessFilterOptions
            .Include(o => o.Definition)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (option == null)
            return NotFound();

        return View(option);
    }

    // Seçenek güncelleme
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditFilterOption(
        int id,
        string name,
        string value,
        int sortOrder,
        bool isActive)
    {
        var option = await _context.BusinessFilterOptions
            .Include(o => o.Definition)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (option == null)
            return NotFound();

        name = name?.Trim() ?? "";
        value = value?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(value))
        {
            TempData["Error"] = "Seçenek adı ve değeri zorunludur.";
            return RedirectToAction(nameof(EditFilterOption), new { id });
        }

        bool duplicateExists = await _context.BusinessFilterOptions
            .AnyAsync(o =>
                o.Id != id &&
                o.BusinessFilterDefinitionId == option.BusinessFilterDefinitionId &&
                o.Value == value);

        if (duplicateExists)
        {
            TempData["Error"] = "Bu filtrede aynı değere sahip başka bir seçenek var.";
            return RedirectToAction(nameof(EditFilterOption), new { id });
        }

        option.Name = name;
        option.Value = value;
        option.SortOrder = sortOrder;
        option.IsActive = isActive;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Seçenek başarıyla güncellendi.";

        return RedirectToAction(
            nameof(ManageFilterOptions),
            new { id = option.BusinessFilterDefinitionId });
    }

    private async Task LoadBusinessCategories()
    {
        ViewBag.BusinessCategories = await _context.BusinessCategories
            .Where(c => c.Id != 1)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    // Yeni kategori formunu aç
    [HttpGet]
    public IActionResult CreateCategory()
    {
        return View();
    }

    // Yeni kategoriyi kaydet
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory(string name)
    {
        name = name?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["Error"] = "Kategori adı boş olamaz.";
            return RedirectToAction(nameof(CreateCategory));
        }

        bool exists = await _context.BusinessCategories
            .AnyAsync(category => category.Name.ToLower() == name.ToLower());

        if (exists)
        {
            TempData["Error"] = "Bu kategori zaten mevcut.";
            return RedirectToAction(nameof(CreateCategory));
        }

        _context.BusinessCategories.Add(
            new BusinessCategory { Name = name });

        await _context.SaveChangesAsync();

        TempData["Success"] = "Kategori başarıyla eklendi.";
        return RedirectToAction(nameof(Index));
    }

    // Kategori düzenleme formunu aç
    [HttpGet]
    public async Task<IActionResult> EditCategory(int id)
    {
        var category = await _context.BusinessCategories
            .FirstOrDefaultAsync(category => category.Id == id);

        if (category == null)
        {
            return NotFound();
        }

        return View(category.Name);
    }

    // Kategori adını güncelle
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCategory(int id, string name)
    {
        var category = await _context.BusinessCategories
            .FirstOrDefaultAsync(category => category.Id == id);

        if (category == null)
        {
            return NotFound();
        }

        name = name?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["Error"] = "Kategori adı boş olamaz.";
            return RedirectToAction(nameof(EditCategory), new { id });
        }

        bool duplicateExists = await _context.BusinessCategories
            .AnyAsync(other =>
                other.Id != id &&
                other.Name.ToLower() == name.ToLower());

        if (duplicateExists)
        {
            TempData["Error"] = "Bu isimde başka bir kategori zaten mevcut.";
            return RedirectToAction(nameof(EditCategory), new { id });
        }

        category.Name = name;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Kategori başarıyla güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    // Kategori silme
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _context.BusinessCategories
            .FirstOrDefaultAsync(category => category.Id == id);

        if (category == null)
        {
            return NotFound();
        }

        bool hasBusinesses = await _context.BusinessProfiles
            .AnyAsync(business => business.CategoryId == id);

        if (hasBusinesses)
        {
            TempData["Error"] =
                "Bu kategoriye bağlı işletmeler var. Önce işletmelerin kategori ilişkilerini düzenlemelisiniz.";

            return RedirectToAction(nameof(Index));
        }

        _context.BusinessCategories.Remove(category);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Kategori başarıyla silindi.";
        return RedirectToAction(nameof(Index));
    }
}
