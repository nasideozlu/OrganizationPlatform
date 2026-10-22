using System.ComponentModel.DataAnnotations;

namespace OrganizationPlatform.Models;

public class RegisterViewModel
{
    [Required]
    public string Name {get; set;} = string.Empty;
    
    [Required]
    public string Surname {get; set;} = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email {get; set;} = string.Empty;
    
    [Required]
    [Phone]
    public string PhoneNumber {get; set;} = string.Empty;
    [Required]
    [MinLength(8)]
    public string Password {get; set;} = string.Empty;
    [Required]
    [Compare("Password")]
    public string PasswordConfirmation {get; set;} = string.Empty;

}