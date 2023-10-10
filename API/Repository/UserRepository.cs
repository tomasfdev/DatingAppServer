using API.Data;
using API.DTOs;
using API.Helpers;
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

        public async Task<AppUser> GetUserByNameAsync(string username)
        {
            return await _context.Users.Include(p => p.Photos).SingleOrDefaultAsync(u => u.UserName == username);
        }

        public async Task<UserDto> GetUserDtoByNamedAsync(string username)
        {
            return await _context.Users.Where(u => u.UserName == username).ProjectTo<UserDto>(_mapper.ConfigurationProvider).SingleOrDefaultAsync();    //retorna um UserDto ao inves de um AppUser
            //para a db ñ fazer extra work, para ser + eficiente... pq existem certas props em AppUser que ñ são retornadas como por exemplo(PasswordHash)
            //etão para ser + eficiente retornamos um UserDto apenas com as props necessárias !! Isto é um plus
        }

        public async Task<string> GetUserGenderAsync(string username)
        {
            return await _context.Users.Where(n => n.UserName == username).Select(g => g.Gender).FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyList<AppUser>> GetUsersAsync()
        {
            return await _context.Users.Include(p => p.Photos).ToListAsync();
        }

        public async Task<PagedList<UserDto>> GetUsersDtoAsync(UserParams userParams)
        {
            var query = _context.Users.AsQueryable();

            query = query.Where(u => u.UserName != userParams.CurrentUserName); //retorna todos Users com o nome diferente ao CurrentUser.(Exemplo: Em Matches, quando aparece a lista de Users, assim aparecem todos os Users menos o perfil do CurrentUser)
            query = query.Where(u => u.Gender == userParams.Gender);    //retorna apenas os Users com o mesmo Gender do CurrentUser

            var menorDataNascimento = DateTime.Today.AddYears(-userParams.MaxAge - 1);  //oldest DateOfBirth/Person
            var maiorDataNascimento = DateTime.Today.AddYears(-userParams.MinAge);

            query = query.Where(u => u.DateOfBirth >= menorDataNascimento && u.DateOfBirth <= maiorDataNascimento);

            query = userParams.OrderBy switch
            {
                "created" => query.OrderByDescending(u => u.Created),
                _ => query.OrderByDescending(u => u.LastActive) //_ é o caso default
            };

            return await PagedList<UserDto>.CreateAsync(query.AsNoTracking().ProjectTo<UserDto>(_mapper.ConfigurationProvider), userParams.PageNumber, userParams.PageSize);
        }

        public void Update(AppUser appUser)
        {
            _context.Entry(appUser).State = EntityState.Modified;
        }
    }
}
