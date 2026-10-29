
using Microsoft.AspNetCore.Http;

namespace OrganizationPlatform.Services;

public class PhotoStorageService
{
    private readonly IWebHostEnvironment _environment;

    private const long MaxFileSize = 5 * 1024 * 1024;

    private static readonly string[] AllowedExtensions =
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

    public PhotoStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string> SavePhotoAsync(
        IFormFile photo,
        string category,
        int ownerId)
    {
        if (photo.Length == 0)
        {
            throw new InvalidOperationException("Dosya boş olamaz.");
        }

        if (photo.Length > MaxFileSize)
        {
            throw new InvalidOperationException(
                "Fotoğraf boyutu 5 MB sınırını aşamaz.");
        }

        var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();

        if (!AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException(
                "Yalnızca JPEG, PNG ve WebP dosyaları yüklenebilir.");
        }

        var folderPath = Path.Combine(
            _environment.WebRootPath,
            "uploads",
            category,
            ownerId.ToString());

        Directory.CreateDirectory(folderPath);

        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(folderPath, fileName);

        await using var stream = new FileStream(filePath, FileMode.CreateNew);
        await photo.CopyToAsync(stream);

        return $"/uploads/{category}/{ownerId}/{fileName}";
    }


    public Task DeletePhotoAsync(string filePath, string category, int ownerId)
    {
        var normalizedPath = filePath.TrimStart('/')
            .Replace('/', Path.DirectorySeparatorChar);

        var uploadsRoot = Path.GetFullPath(
            Path.Combine(_environment.WebRootPath, "uploads"));

        var fullPath = Path.GetFullPath(
            Path.Combine(_environment.WebRootPath, normalizedPath));

        var ownerFolder = Path.GetFullPath(
            Path.Combine(uploadsRoot, category, ownerId.ToString()));

        // Yalnızca ilgili işletmenin kendi fotoğraf klasöründen silmeye izin ver.
        if (!fullPath.StartsWith(
                ownerFolder + Path.DirectorySeparatorChar,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Geçersiz fotoğraf yolu.");
        }

        if (System.IO.File.Exists(fullPath))
        {
            System.IO.File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

}
