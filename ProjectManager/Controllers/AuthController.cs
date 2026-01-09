using Microsoft.AspNetCore.Mvc;
using ProjectManager.Application.Interfaces;
using ProjectManager.Application.DTO;

namespace ProjectManager.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(ISecurityService security) : ControllerBase
    {
        [HttpPost("login")]
        public async Task<IResult> Login(LoginDTO login)
        {
            var token = await security.Login(login);
            
            if(string.IsNullOrEmpty(token))
                return Results.Unauthorized();

            return Results.Ok(new { token });
        }
    }
}
