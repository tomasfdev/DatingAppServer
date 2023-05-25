using API.DTOs;
using API.Interfaces;
using API.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UsersController(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<UserDto>>> GetUsers()
        {
            var users = await _userRepository.GetUsersDtoAsync();

            return Ok(users);
        }

        //[HttpGet("{id}")]
        //public async Task<ActionResult<AppUser>> GetUserById(int id)
        //{
        //    return Ok(await _userRepository.GetUserByIdAsync(id));
        //}

        [HttpGet("{name}")]
        public async Task<ActionResult<UserDto>> GetUserByName(string name)
        {
            var user = await _userRepository.GetUserDtoByNamedAsync(name);

            return Ok(user);
        }
    }
}
