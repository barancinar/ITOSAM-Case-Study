using ItoCase.Core.DTOs;
using ItoCase.Core.Entities;
using ItoCase.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ItoCase.Service.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;

        public UserService(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var userDtos = new List<UserDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userDtos.Add(new UserDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserName = user.UserName,
                    Email = user.Email,
                    Role = roles.FirstOrDefault() ?? "Atanmamış",
                    Roles = roles.ToList()
                });
            }

            return userDtos;
        }

        public async Task<UserDto> GetUserByIdAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) throw new Exception("Kullanıcı bulunamadı.");

            var roles = await _userManager.GetRolesAsync(user);

            return new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                Email = user.Email,
                Role = roles.FirstOrDefault() ?? ""
            };
        }

        public async Task CreateUserAsync(UserCreateDto userDto)
        {
            var user = new AppUser
            {
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                UserName = userDto.UserName,
                Email = userDto.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, userDto.Password ?? "DefaultPass123!");
            if (!result.Succeeded)
            {
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            if (!string.IsNullOrEmpty(userDto.Role))
            {
                if (await _roleManager.RoleExistsAsync(userDto.Role))
                {
                    await _userManager.AddToRoleAsync(user, userDto.Role);
                }
            }
        }

        public async Task UpdateUserAsync(UserUpdateDto userDto)
        {
            var user = await _userManager.FindByIdAsync(userDto.Id ?? "");
            if (user == null) throw new Exception("Kullanıcı bulunamadı.");

            user.FirstName = userDto.FirstName;
            user.LastName = userDto.LastName;
            user.Email = userDto.Email;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

            // Rol Güncelleme logic'i
            var currentRoles = await _userManager.GetRolesAsync(user);
            
            // Eğer rol değiştiyse ve yeni rol boş değilse
            if (userDto.Role != null && !currentRoles.Contains(userDto.Role))
            {
                // Eski rolleri sil
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                // Yeni rolü ekle
                if (await _roleManager.RoleExistsAsync(userDto.Role))
                {
                    await _userManager.AddToRoleAsync(user, userDto.Role);
                }
            }
        }

        public async Task DeleteUserAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) throw new Exception("Kullanıcı bulunamadı.");

            await _userManager.DeleteAsync(user);
        }

        public async Task<List<string>> GetAllRolesAsync()
        {
            return await _roleManager.Roles.Select(r => r.Name ?? "").ToListAsync();
        }
    }
}
