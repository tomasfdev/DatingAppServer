using API.Data;
using API.DTOs;
using API.Interfaces;
using API.Models;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;

        public AccountController(AppDbContext context, ITokenService tokenService, IMapper mapper)
        {
            _context = context;
            _tokenService = tokenService;
            _mapper = mapper;
        }

        private async Task<bool> UserExists(string username)
        {
            return await _context.Users.AnyAsync(u => u.UserName == username.ToLower());
        }

        [HttpPost("register")]
        public async Task<ActionResult<AppUserDto>> Register(RegisterDto registerDto)
        {
            if (await UserExists(registerDto.Username))
                return BadRequest("Username is taken");

            var newUser = _mapper.Map<AppUser>(registerDto);

            using var hmac = new HMACSHA512();

            newUser.UserName = registerDto.Username.ToLower();
            newUser.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
            newUser.PasswordSalt = hmac.Key;

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(new AppUserDto
            {
                Username = newUser.UserName,
                Token = _tokenService.CreateToken(newUser),
                KnownAs = newUser.KnownAs
            });
        }

        [HttpPost("login")]
        public async Task<ActionResult<AppUserDto>> Login(LoginDto loginDto)
        {
            var user = await _context.Users.Include(p => p.Photos).SingleOrDefaultAsync(u => u.UserName == loginDto.Username);  //FirstOrDefaultAsync(u => u.UserName == loginDto.Username) ou SingleOrDefaultAsync

            if (user == null)
                return Unauthorized("Invalid username");

            using var hmac = new HMACSHA512(user.PasswordSalt);

            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i])
                    return Unauthorized("Invalid password");
            }

            return Ok(new AppUserDto
            {
                Username = user.UserName,
                Token = _tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(p => p.IsMain)?.Url,
                KnownAs = user.KnownAs
            });
        }
    }
}
