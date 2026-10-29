
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrganizationPlatform.Data;
using OrganizationPlatform.Models;

namespace OrganizationPlatform.Controllers;

[Authorize(Roles = "Admin")]
public class FeatureCategoriesController : Controller
{
    private readonly ApplicationDbContext _context;

    public FeatureCategoriesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Özellik gruplarını listele
    public async Task<IActionResult> Index()
    {
        var categories = await _context.FeatureCategories
            .Include(category => category.PhysicalFeatures)
            .OrderBy(category => category.Name)
            .ToListAsync();

        return View(categories);
    }

    // Yeni grup formu
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    // Yeni grup kaydet
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name)
    {
        name = name?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["Error"] = "Grup adı boş olamaz.";
            return RedirectToAction(nameof(Create));
        }

        if (await _context.FeatureCategories.AnyAsync(
            category => category.Name.ToLower() == name.ToLower()))
        {
            TempData["Error"] = "Bu isimde bir özellik grubu zaten mevcut.";
            return RedirectToAction(nameof(Create));
        }

        _context.FeatureCategories.Add(
            new FeatureCategory { Name = name });

        await _context.SaveChangesAsync();

        TempData["Success"] = "Özellik grubu eklendi.";
        return RedirectToAction(nameof(Index));
    }

    // Grup düzenleme formu
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var category = await _context.FeatureCategories
            .FindAsync(id);

        if (category == null)
            return NotFound();

        return View(category);
    }

    // Grup adını güncelle
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id, [Bind("Id,Name")] FeatureCategory model)
    {
        var category = await _context.FeatureCategories
            .FindAsync(id);

        if (category == null)
            return NotFound();

        var name = model.Name?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(name))
        {
            ModelState.AddModelError("Name", "Grup adı boş olamaz.");
            model.Name = name;
            return View(model);
        }

        bool duplicateExists = await _context.FeatureCategories
            .AnyAsync(other =>
                other.Id != id &&
                other.Name.ToLower() == name.ToLower());

        if (duplicateExists)
        {
            ModelState.AddModelError(
                "Name", "Bu isimde başka bir grup zaten mevcut.");

            model.Name = name;
            return View(model);
        }

        category.Name = name;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Özellik grubu güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    // Grubu sil
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _context.FeatureCategories
            .Include(category => category.PhysicalFeatures)
            .FirstOrDefaultAsync(category => category.Id == id);

        if (category == null)
            return NotFound();

        if (category.PhysicalFeatures.Count > 0)
        {
            TempData["Error"] =
                "Bu gruba bağlı özellikler var. Önce özellikleri başka bir gruba taşıyın veya silin.";

            return RedirectToAction(nameof(Index));
        }

        _context.FeatureCategories.Remove(category);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Özellik grubu silindi.";
        return RedirectToAction(nameof(Index));
    }
}
