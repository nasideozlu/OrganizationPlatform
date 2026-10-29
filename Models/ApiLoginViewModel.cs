using System.ComponentModel.DataAnnotations;

namespace OrganizationPlatform.Models;

public class ApiLoginViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}