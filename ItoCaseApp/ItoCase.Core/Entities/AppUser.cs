using Microsoft.AspNetCore.Identity;

namespace ItoCase.Core.Entities
{
    // Standart IdentityUser sınıfını genişletiyoruz.
    public class AppUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}