
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrganizationPlatform.Data;
using OrganizationPlatform.Models;

namespace OrganizationPlatform.Controllers;

[Authorize(Roles = "Admin")]
public class PhysicalFeaturesController : Controller
{
    private readonly ApplicationDbContext _context;

    public PhysicalFeaturesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Özellikleri gruplarıyla birlikte listele
    public async Task<IActionResult> Index()
    {
        var features = await _context.PhysicalFeatures
            .Include(feature => feature.Category)
            .OrderBy(feature => feature.Category.Name)
            .ThenBy(feature => feature.Name)
            .ToListAsync();

        return View(features);
    }

    // Yeni özellik formu
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadCategoriesAsync();
        return View();
    }

    // Yeni özellik kaydet
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name, int categoryId)
    {
        name = name?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["Error"] = "Özellik adı boş olamaz.";
            return RedirectToAction(nameof(Create));
        }

        bool categoryExists = await _context.FeatureCategories
            .AnyAsync(category => category.Id == categoryId);

        if (!categoryExists)
        {
            TempData["Error"] = "Geçerli bir özellik grubu seçmelisiniz.";
            return RedirectToAction(nameof(Create));
        }

        bool duplicateExists = await _context.PhysicalFeatures
            .AnyAsync(feature =>
                feature.CategoryId == categoryId &&
                feature.Name.ToLower() == name.ToLower());

        if (duplicateExists)
        {
            TempData["Error"] =
                "Bu grupta aynı isimde bir özellik zaten mevcut.";

            return RedirectToAction(nameof(Create));
        }

        var category = await _context.FeatureCategories
     .FirstOrDefaultAsync(c => c.Id == categoryId);

        if (category == null)
        {
            return NotFound();
        }

        _context.PhysicalFeatures.Add(new PhysicalFeature
        {
            Name = name,
            CategoryId = categoryId,
            Category = category
        });

        await _context.SaveChangesAsync();

        TempData["Success"] = "Mekân özelliği eklendi.";
        return RedirectToAction(nameof(Index));
    }

    // Özellik düzenleme formu
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var feature = await _context.PhysicalFeatures
            .FirstOrDefaultAsync(feature => feature.Id == id);

        if (feature == null)
            return NotFound();

        await LoadCategoriesAsync();
        return View(feature);
    }

    // Özelliği güncelle
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id, string name, int categoryId)
    {
        var feature = await _context.PhysicalFeatures
            .FirstOrDefaultAsync(feature => feature.Id == id);

        if (feature == null)
            return NotFound();

        name = name?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["Error"] = "Özellik adı boş olamaz.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        bool categoryExists = await _context.FeatureCategories
            .AnyAsync(category => category.Id == categoryId);

        if (!categoryExists)
        {
            TempData["Error"] = "Geçerli bir özellik grubu seçmelisiniz.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        bool duplicateExists = await _context.PhysicalFeatures
            .AnyAsync(other =>
                other.Id != id &&
                other.CategoryId == categoryId &&
                other.Name.ToLower() == name.ToLower());

        if (duplicateExists)
        {
            TempData["Error"] =
                "Bu grupta aynı isimde bir özellik zaten mevcut.";

            return RedirectToAction(nameof(Edit), new { id });
        }

        feature.Name = name;
        feature.CategoryId = categoryId;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Mekân özelliği güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    // Özelliği sil
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var feature = await _context.PhysicalFeatures
            .Include(feature => feature.Venues)
            .FirstOrDefaultAsync(feature => feature.Id == id);

        if (feature == null)
            return NotFound();

        if (feature.Venues.Count > 0)
        {
            TempData["Error"] =
                "Bu özellik mevcut mekânlarda kullanılıyor. Önce mekânlarla olan ilişkisini kaldırmalısınız.";

            return RedirectToAction(nameof(Index));
        }

        _context.PhysicalFeatures.Remove(feature);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Mekân özelliği silindi.";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadCategoriesAsync()
    {
        ViewBag.FeatureCategories = await _context.FeatureCategories
            .OrderBy(category => category.Name)
            .ToListAsync();
    }
}
