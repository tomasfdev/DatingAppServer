using API.DTOs;
using API.Models;

namespace API.Interfaces
{
    public interface IUserRepository
    {
        Task<IReadOnlyList<AppUser>> GetUsersAsync();
        Task<AppUser> GetUserByIdAsync(int id);
        Task<AppUser> GetUserByNameAsync(string name);
        Task<IReadOnlyList<UserDto>> GetUsersDtoAsync();
        Task<UserDto> GetUserDtoByNamedAsync(string name);
        Task<bool> SaveAllAsync();
        void Update(AppUser appUser); 
    }
}
