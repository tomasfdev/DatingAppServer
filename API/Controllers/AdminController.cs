using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;

        public AdminController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        [Authorize(Policy ="RequireAdminRole")]
        [HttpGet("users-with-roles")]
        public async Task<ActionResult> GetUsersWithRoles()
        {
            var users = await _userManager.Users.OrderBy(u => u.UserName).Select(u => new
            {
                u.Id,
                Username = u.UserName,
                Roles = u.UserRoles.Select(r => r.Role.Name).ToList()
            })
                .ToListAsync(); //retorna uma lista de users com a(s) sua(s) role(s)

            return Ok(users);
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost("edit-roles/{username}")]
        public async Task<ActionResult> EditRoles(string username, [FromQuery]string roles)
        {
            if (string.IsNullOrEmpty(roles)) return BadRequest("You must select at least one role");

            var selectedRoles = roles.Split(',').ToArray(); //meter as roles num array separadas por ","

            var user = await _userManager.FindByNameAsync(username);

            if (user == null) return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user); //vai buscar as roles do current user

            var editUserRoles = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));  //adiciona user às roles, menos as que ja tem

            if (!editUserRoles.Succeeded) return BadRequest("Failed to add user roles");

            editUserRoles = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles)); //remove user das roles,

            if (!editUserRoles.Succeeded) return BadRequest("Failed to remove user roles");

            return Ok(await _userManager.GetRolesAsync(user));  //retorna as roles actualizadas do current user
        }

        [Authorize(Policy ="ModeratePhotoRole")]
        [HttpGet("photos-to-moderate")]
        public async Task<ActionResult> GetPhotosForModeration()
        {
            return Ok("Only admins or moderators");
        }
    }
}
