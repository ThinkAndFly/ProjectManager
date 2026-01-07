using Microsoft.AspNetCore.Mvc;
using ProjectManager.Application.Interfaces;
using ProjectManager.Application.DTO;

namespace ProjectManager.Presentation.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController(ISecurityApplication security) : ControllerBase
    {
        [HttpPost(Name = "login")]
        public async Task<IResult> Login(LoginDTO login)
        {
            var token = await security.Login(login);
            
            if(string.IsNullOrEmpty(token))
                return Results.Unauthorized();

            return Results.Ok(new { token });
        }
    }
}
