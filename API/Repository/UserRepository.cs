using API.Data;
using API.DTOs;
using API.Interfaces;
using API.Models;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public UserRepository(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<AppUser> GetUserByNameAsync(string name)
        {
            return await _context.Users.Include(p => p.Photos).SingleOrDefaultAsync(u => u.UserName == name);
        }

        public async Task<UserDto> GetUserDtoByNamedAsync(string name)
        {
            return await _context.Users.Where(u => u.UserName == name).ProjectTo<UserDto>(_mapper.ConfigurationProvider).SingleOrDefaultAsync();    //retorna um UserDto ao inves de um AppUser
            //para a db ñ fazer extra work, para ser + eficiente... pq existem certas props em AppUser que ñ são retornadas como por exemplo(PasswordHash)
            //etão para ser + eficiente retornamos um UserDto apenas com as props necessárias !! Isto é um plus
        }

        public async Task<IReadOnlyList<AppUser>> GetUsersAsync()
        {
            return await _context.Users.Include(p => p.Photos).ToListAsync();
        }

        public async Task<IReadOnlyList<UserDto>> GetUsersDtoAsync()
        {
            return await _context.Users.ProjectTo<UserDto>(_mapper.ConfigurationProvider).ToListAsync();
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;   //se for maior que 0 retorna true, salvou algo... caso contrario retorna false, ñ salvou nada
        }

        public void Update(AppUser appUser)
        {
            _context.Entry(appUser).State = EntityState.Modified;
        }
    }
}
