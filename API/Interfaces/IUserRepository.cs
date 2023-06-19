using API.DTOs;
using API.Helpers;
using API.Models;

namespace API.Interfaces
{
    public interface IUserRepository
    {
        Task<IReadOnlyList<AppUser>> GetUsersAsync();
        Task<AppUser> GetUserByIdAsync(int id);
        Task<AppUser> GetUserByNameAsync(string name);
        Task<PagedList<UserDto>> GetUsersDtoAsync(UserParams userParams);
        Task<UserDto> GetUserDtoByNamedAsync(string name);
        Task<bool> SaveAllAsync();
        void Update(AppUser appUser); 
    }
}
