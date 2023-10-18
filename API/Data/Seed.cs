using API.DTOs;
using API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace API.Data
{
    public class Seed
    {
        public static async Task ClearConnections(AppDbContext context)
        {
            context.Connections.RemoveRange(context.Connections);
            await context.SaveChangesAsync();
        }

        public static async Task SeedUsers(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)    //static para ñ ter que criar uma nova instância de Seed para poder usar este method... Seed.SeedUsers
        {
            if (await userManager.Users.AnyAsync()) //verifica se há users data
                return;

            var userData = await File.ReadAllTextAsync("Data/UserSeedData.json");   //vai buscar data ao ficheiro "UserSeedData"

            //var options = new JsonSerializerOptions {PropertyNameCaseInsensitive = true};

            var users = JsonSerializer.Deserialize<List<AppUser>>(userData);    //converter de json para c# object(Neste caso uma List de AppUser)

            var roles = new List<AppRole>
            {
                new AppRole{Name = "Member"},
                new AppRole{Name = "Admin"},
                new AppRole{Name = "Moderator"}
            };

            foreach (var role in roles)
            {
                await roleManager.CreateAsync(role);    //Cria role e adiciona diretamente na DB
            }

            foreach (var user in users)
            {
                //repetir o codigo utilizado para registar users em AccountController

                user.UserName = user.UserName.ToLower();
                user.Created = DateTime.SpecifyKind(user.Created, DateTimeKind.Utc);
                user.LastActive = DateTime.SpecifyKind(user.LastActive, DateTimeKind.Utc);

                await userManager.CreateAsync(user, "Pa$$w0rd");    //Cria user e guarda diretamente na DB

                await userManager.AddToRoleAsync(user, "Member");   //Adiciona role ao user, role "Member"
            }

            var admin = new AppUser
            {
                UserName = "Admin"
            };

            await userManager.CreateAsync(admin, "Pa$$w0rd");
            await userManager.AddToRolesAsync(admin, new[] {"Admin", "Moderator"});
        }
    }
}
