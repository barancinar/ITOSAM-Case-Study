using ItoCase.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace ItoCase.Web.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider service)
        {
            // Kullanıcı ve Rol Yöneticilerini al
            var userManager = service.GetRequiredService<UserManager<AppUser>>();
            var roleManager = service.GetRequiredService<RoleManager<AppRole>>();

            // Roller
            string[] roles = { "Admin", "Uzman", "Analist" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new AppRole { Name = role });
                }
            }

            // DEFAULT ADMIN KULLANCISI
            var adminUser = new AppUser
            {
                FirstName = "Sistem",
                LastName = "Admin",
                UserName = "admin",
                Email = "admin@itocase.com",
                EmailConfirmed = true
            };

            if (await userManager.FindByEmailAsync(adminUser.Email) == null)
            {
                var result = await userManager.CreateAsync(adminUser, "Admin123!");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}