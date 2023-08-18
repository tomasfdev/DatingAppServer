using API.Data;
using API.DTOs;
using API.Helpers;
using API.Interfaces;
using API.Models;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ServiceFilter(typeof(LogUserActivity))]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;

        public AccountController(UserManager<AppUser> userManager, ITokenService tokenService, IMapper mapper)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _mapper = mapper;
        }

        private async Task<bool> UserExists(string username)
        {
            return await _userManager.Users.AnyAsync(u => u.UserName == username.ToLower());
        }

        [HttpPost("register")]
        public async Task<ActionResult<AppUserDto>> Register(RegisterDto registerDto)
        {
            if (await UserExists(registerDto.Username))
                return BadRequest("Username is taken");

            var newUser = _mapper.Map<AppUser>(registerDto);

            newUser.UserName = registerDto.Username.ToLower();

            var createdUser = await _userManager.CreateAsync(newUser, registerDto.Password);

            if (!createdUser.Succeeded) return BadRequest(createdUser.Errors);

            var addUserRole = await _userManager.AddToRoleAsync(newUser, "Member"); 

            if (!addUserRole.Succeeded) return BadRequest(createdUser.Errors);

            return Ok(new AppUserDto
            {
                Username = newUser.UserName,
                Token = await _tokenService.CreateTokenAsync(newUser),
                KnownAs = newUser.KnownAs,
                Gender = newUser.Gender
            });
        }

        [HttpPost("login")]
        public async Task<ActionResult<AppUserDto>> Login(LoginDto loginDto)
        {
            var user = await _userManager.Users.Include(p => p.Photos).SingleOrDefaultAsync(u => u.UserName == loginDto.Username);  //FirstOrDefaultAsync(u => u.UserName == loginDto.Username) ou SingleOrDefaultAsync

            if (user == null) return Unauthorized("Wrong credentials");

            var pwCheck = await _userManager.CheckPasswordAsync(user, loginDto.Password);

            if (!pwCheck) return Unauthorized("Wrong credentials");

            return Ok(new AppUserDto
            {
                Username = user.UserName,
                Token = await _tokenService.CreateTokenAsync(user),
                PhotoUrl = user.Photos.FirstOrDefault(p => p.IsMain)?.Url,
                KnownAs = user.KnownAs,
                Gender = user.Gender
            });
        }
    }
}
