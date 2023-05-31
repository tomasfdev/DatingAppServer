﻿using API.DTOs;
using API.Interfaces;
using API.Models;
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
        private readonly IPhotoService _photoService;

        public UsersController(IUserRepository userRepository, IMapper mapper, IPhotoService photoService)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _photoService = photoService;
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
            var userName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;    //vai buscar userName ao token...NameIdentifier é o NameId dentro do token(Ver NameId em tokenService)
            var user = await _userRepository.GetUserByNameAsync(userName);

            if (user == null)
                return NotFound();

            _mapper.Map(userUpdateDto, user);   //mapeia e actualiza as props... De userUpdateDto que recebeu/entrou para user que era o "original" que estava na db 

            if (await _userRepository.SaveAllAsync())   //guarda na db
                return NoContent();

            return BadRequest("Failed to update user");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var userName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _userRepository.GetUserByNameAsync(userName);

            if (user == null)
                return NotFound();

            var result = await _photoService.AddPhotoAsync(file);
            if (result.Error != null)
                return BadRequest(result.Error.Message);

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };

            if (user.Photos.Count == 0) //se for a 1ª foto deste user, passa a ser a main foto de perfil !
                photo.IsMain = true;

            user.Photos.Add(photo); //estes methods (por exemplo Add) só são possiveis pq EntityFrameWork está a dar track no user em memoria(primeiras linhas de codigo deste methods AddPhoto)

            if (await _userRepository.SaveAllAsync())
            {
                return CreatedAtAction(nameof(GetUserByName), new {username = user.UserName}, _mapper.Map<PhotoDto>(photo));    //se guardar corretamente retorna photoDto
            }

            return BadRequest("Problem adding photo");  //caso contrario retorna BadRequest
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var userName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _userRepository.GetUserByNameAsync(userName);

            if (user == null) return NotFound();

            var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);   //vai a todas a fotos do current user("user" neste caso) verificar se existe alguma com o id igual ao id passado como parametro(photoId)... fazer debug em duvida
            if (photo == null) return NotFound();   //caso entre todas as fotos do current user("user" neste caso) ñ encontre nenhuma com os ids iguais(photo.id == photoId) retorna NotFound()... fazer debug em duvida

            if (photo.IsMain) return BadRequest("This is already your main photo");

            var currentMainPhoto = user.Photos.FirstOrDefault(p => p.IsMain);   //vai buscar main photo
            if (currentMainPhoto != null) currentMainPhoto.IsMain = false;
            photo.IsMain = true;

            if (await _userRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Problem setting the main photo");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var userName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _userRepository.GetUserByNameAsync(userName);

            if (user == null) return NotFound();

            var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);
            if (photo == null) return NotFound();

            if (photo.IsMain) return BadRequest("You can't delete your main photo");

            if (photo.PublicId != null)
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null) return BadRequest(result.Error.Message);
            }

            user.Photos.Remove(photo);

            if (await _userRepository.SaveAllAsync()) return Ok();

            return BadRequest("Problem deleting photo");
        }
    }
}
