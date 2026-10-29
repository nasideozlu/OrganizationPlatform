
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
