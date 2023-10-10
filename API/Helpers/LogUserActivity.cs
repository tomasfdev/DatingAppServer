using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Helpers
{
    public class LogUserActivity : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var resultContext = await next();   //retorna uma ActionExecutedContext, significa que a API Action/Endpoint foi concluído e vou receber o context result(ActionExecutedContext)

            if (!resultContext.HttpContext.User.Identity.IsAuthenticated) return;   //se user ñ tiver autenticado, para execution

            var userId = resultContext.HttpContext.User.GetUserId();

            var uow = resultContext.HttpContext.RequestServices.GetRequiredService<IUnitOfWork>(); //aceder ao UserRepository porque vou alterar User
            var user = await uow.UserRepository.GetUserByIdAsync(userId); //vai buscar user
            user.LastActive = DateTime.UtcNow;  
            await uow.Complete();
        }
    }
}
