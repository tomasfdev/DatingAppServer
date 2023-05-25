using API.DTOs;
using API.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace API.Data
{
    public class Seed
    {
        public static async Task SeedUsers(AppDbContext context)    //static para ñ ter que criar uma nova instância de Seed para poder usar este method... Seed.SeedUsers
        {
            if (await context.Users.AnyAsync()) //verifica se há users data
                return;

            var userData = await File.ReadAllTextAsync("Data/UserSeedData.json");   //vai buscar data ao ficheiro "UserSeedData"

            var options = new JsonSerializerOptions {PropertyNameCaseInsensitive = true};

            var users = JsonSerializer.Deserialize<List<AppUser>>(userData);    //converter de json para c# object(Neste caso uma List de AppUser)

            foreach (var user in users)
            {
                //repetir o codigo utilizado para registar users em AccountController
                using var hmac = new HMACSHA512();

                user.UserName = user.UserName.ToLower();
                user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("Pa$$w0rd"));
                user.PasswordSalt = hmac.Key;

                context.Users.Add(user);
            }

            await context.SaveChangesAsync();
        }
    }
}
