using API.Data;
using API.Helpers;
using API.Interfaces;
using API.Models;
using API.Repository;
using API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace API.Extensions
{
    public static class AppServicesExtensions
    {
        public static IServiceCollection AppServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddCors(options => //Cors service
            {
                options.AddPolicy(name: "AngularApp", policy =>
                {
                    policy.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                });
            });

            services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));    //Cloudinary service
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());    //AutoMapper service
            services.AddScoped<LogUserActivity>();
            services.AddScoped<ITokenService, TokenService>();  //Token service
            services.AddScoped<IPhotoService, PhotoService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddSignalR();
            services.AddSingleton<PresenceTracker>();   //Singleton para que o serviço/resultado seja sempre o mesmo para cada request e que acabe quando a app encerrar

            //--------------------------Identity Services------------------------------------------------------------------------

            services.AddIdentityCore<AppUser>(options =>
            {
                options.Password.RequireNonAlphanumeric = false;
            })
                .AddRoles<AppRole>()
                .AddRoleManager<RoleManager<AppRole>>()
                .AddEntityFrameworkStores<AppDbContext>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>  //Authentication service
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"])),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };

                options.Events = new JwtBearerEvents    //Authentication SignalR
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];    //accessToken = Bearer_Token. Como ñ podemos passar como HTTP HEADER, temos que passar como QueryString

                        var path = context.HttpContext.Request.Path;    //verifica 
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs")) //verifica se há token e se estamos no caminho("/hubs")
                        {
                            context.Token = accessToken;    //dá acesso ao token/Bearer_Token
                        }

                        return Task.CompletedTask;
                    }
                };
            });

            services.AddAuthorization(options =>    //Authorization service
            {
                options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
                options.AddPolicy("ModeratePhotoRole", policy => policy.RequireRole("Admin", "Moderator"));
            });

            return services;
        }
    }
}
