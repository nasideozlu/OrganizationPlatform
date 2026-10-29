using Microsoft.AspNetCore.Identity;

namespace OrganizationPlatform.Models;

public class ApplicationUser : IdentityUser
{
    public AccountType AccountType {get; set;} = AccountType.Customer;
    
}