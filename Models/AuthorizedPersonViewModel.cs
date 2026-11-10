using OrganizationPlatform.Models;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace OrganizationPlatform.ViewModels;

public class AuthorizedPersonViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Ad alanı zorunludur.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Soyad alanı zorunludur.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Yetki türü seçilmelidir.")]
    public AuthorizedPersonRole Role { get; set; }

    public string? PhoneNumber { get; set; }

    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
    public string? Email { get; set; }

    public IFormFile? Photo { get; set; }

    public string? ExistingPhotoPath { get; set; }
}