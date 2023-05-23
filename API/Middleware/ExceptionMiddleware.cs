using API.Errors;
using System.Net;
using System.Text.Json;

namespace API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);   //print erro no terminal independentemente do enviroment mode, modo desenvolvimento ou produção !!! 
                context.Response.ContentType = "application/json";  //retorna ao cliente
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;  //retorna current error em integer

                var response = _env.IsDevelopment()
                    ? new ApiException(context.Response.StatusCode, ex.Message, ex.StackTrace?.ToString())  //caso esteja em development mode
                    : new ApiException(context.Response.StatusCode, ex.Message, "Internal Server Error");   //caso ñ esteja em development mode

                var options = new JsonSerializerOptions{PropertyNamingPolicy = JsonNamingPolicy.CamelCase}; //error text(formato json) em CamelCase

                var jsonResponse = JsonSerializer.Serialize(response, options); //formata resposta em formato json
                
                await context.Response.WriteAsync(jsonResponse);    //retorna resposta
            }
        }
    }
}
