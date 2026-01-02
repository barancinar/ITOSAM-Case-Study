using ItoCase.Core.DTOs;

namespace ItoCase.Core.Interfaces
{
    public interface IUserService
    {
        Task<List<UserDto>> GetAllUsersAsync();
        Task<UserDto> GetUserByIdAsync(string id);
        Task CreateUserAsync(UserCreateDto userDto);
        Task UpdateUserAsync(UserUpdateDto userDto);
        Task DeleteUserAsync(string id);
        Task<List<string>> GetAllRolesAsync();
    }
}
