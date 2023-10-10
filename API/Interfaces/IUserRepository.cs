using API.DTOs;
using API.Helpers;
using API.Models;

namespace API.Interfaces
{
    public interface IUserRepository
    {
        Task<IReadOnlyList<AppUser>> GetUsersAsync();
        Task<AppUser> GetUserByIdAsync(int id);
        Task<AppUser> GetUserByNameAsync(string username);
        Task<string> GetUserGenderAsync(string username);
        Task<PagedList<UserDto>> GetUsersDtoAsync(UserParams userParams);
        Task<UserDto> GetUserDtoByNamedAsync(string username);
        void Update(AppUser appUser); 
    }
}
