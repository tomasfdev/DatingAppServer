using API.DTOs;
using API.Helpers;
using API.Models;

namespace API.Interfaces
{
    public interface ILikesRepository
    {
        //vai a UserLike encontrar um User com o ID = "sourceUserId"(pessoa q meteu like) e com o ID = "targetUserId"(pessoa que eu meti like/levou like)... retorna user em que meti/levou like
        Task<UserLike> GetUserLike(int sourceUserId, int targetUserId); //quem deu like(sourceUserId) e quem "levou" like(targetUserId)
        Task<AppUser> GetUserWithLikes(int userId); //retorna user E os seus likes
        Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams);  //retorna apenas os likes do user
    }
}
