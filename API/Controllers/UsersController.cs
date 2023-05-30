using API.DTOs;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

        [HttpGet("{username}")]
        public async Task<ActionResult<UserDto>> GetUserByName(string username)
        {
            var user = await _userRepository.GetUserDtoByNamedAsync(username);

            return Ok(user);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(UserUpdateDto userUpdateDto)
        {
            var userName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;    //NameIdentifier é o NameId dentro do token(Ver NameId em tokenService), vai buscar userName ao token
            var user = await _userRepository.GetUserByNameAsync(userName);

            if (user == null)
                return NotFound();

            _mapper.Map(userUpdateDto, user);   //mapeia e actualiza as props... De userUpdateDto que recebeu/entrou para user que era o "original" que estava na db 

            if (await _userRepository.SaveAllAsync())   //guarda na db
                return NoContent();

            return BadRequest("Failed to update user");
        }
    }
}
