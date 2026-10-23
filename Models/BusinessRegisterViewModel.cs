using System.ComponentModel.DataAnnotations;

namespace OrganizationPlatform.Models;

public class BusinessRegisterViewModel
{
    [Required]
    public string Name {get; set;} = string.Empty;
    [Required]
    [EmailAddress]
    public string Email {get; set;} = string.Empty;
    [Required]
    [Phone]
    public string PhoneNumber {get; set;} = string.Empty;
    [Required]
    public string City {get; set;} = string.Empty;
    public int CategoryId {get; set;}
    [Required]
    [MinLength(8)]
    public string Password {get; set;} = string.Empty;
    [Required]
    [Compare("Password")]
    public string PasswordConfirmation {get; set;} = string.Empty;

    public List<BusinessCategory> Categories {get; set;} = new();


}