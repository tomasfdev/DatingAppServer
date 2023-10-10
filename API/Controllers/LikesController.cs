using API.DTOs;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [ServiceFilter(typeof(LogUserActivity))]
    public class LikesController : ControllerBase
    {
        private readonly IUnitOfWork _uow;

        public LikesController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        [HttpPost("{username}")]
        public async Task<ActionResult> AddLike(string username)    //nome do user que vai levar like
        {
            var sourceUserId = User.GetUserId();    //vai buscar o id do user que vai meter like
            var likedUser = await _uow.UserRepository.GetUserByNameAsync(username); //vai a tabela Users na DB buscar o user que levou like
            var sourceUser = await _uow.LikesRepository.GetUserWithLikes(sourceUserId); //vai à tabela Likes na DB e obtem o user COM os seus likes atraves do id(sourceUserId)

            if (likedUser == null) return NotFound();

            if (sourceUser.UserName == username) return BadRequest("You cannot like yourself");

            var userLike = await _uow.LikesRepository.GetUserLike(sourceUserId, likedUser.Id);  //vai à tabela Likes na DB, busca o user(likedUser.Id) em que o currentUser(sourceUserId) deu like, e retorna-o(retorna o user que já dei like)

            if (userLike != null)   //se houver user, diferente d null(é porque já dei like nesse user)
            {
                sourceUser.LikedUsers.Remove(userLike); //remove o user da lista de LikedUsers... remove like
            }
            else
            {
                //sourceUser.LikedUsers.Add(userLike);
                userLike = new UserLike
                {
                    SourceUserId = sourceUserId,
                    TargetUserId = likedUser.Id
                };

                sourceUser.LikedUsers.Add(userLike);
            }

            if (await _uow.Complete()) return Ok();

            return BadRequest("Failed to like user");
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<LikeDto>>> GetUserLikes([FromQuery]LikesParams likesParams)
        {
            likesParams.UserId = User.GetUserId();

            var users = await _uow.LikesRepository.GetUserLikes(likesParams);

            Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages));

            return Ok(users);
        }
    }
}
