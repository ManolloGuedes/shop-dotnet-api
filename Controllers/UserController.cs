using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;
using Shop.Services;

namespace Shop.Controllers
{
    [Route("users")]
    public class UserController : Controller
    {
        [HttpPost]
        [Route("")]
        [AllowAnonymous]
        public async Task<ActionResult<User>> Post(
            [FromServices] DataContext context,
            [FromBody] User user
        )
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                user.Role = "employee";
                context.Users.Add(user);
                await context.SaveChangesAsync();

                user.hidePassword();
                return Created("", user);
            }
            catch
            {
                return BadRequest(new { message = "Não foi possível criar o usuário" });
                throw;
            }
        }

        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<dynamic>> Login(
            [FromServices] DataContext context,
            [FromBody] User user
        )
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userAuthenticated = await context
                .Users
                .AsNoTracking()
                .Where(userDb => userDb.UserName == user.UserName && userDb.Password == user.Password)
                .FirstOrDefaultAsync();

            if (userAuthenticated == null)
                return NotFound(new { message = "Usuário ou senha inválidos" });

            var token = TokenService.GenerateToken(userAuthenticated);

            user.hidePassword();
            return new
            {
                user,
                token
            };
        }
    }
}