using Microsoft.AspNetCore.Mvc;
using ProjectManager.Application.DTO;

namespace ProjectManager.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProjectController : ControllerBase
    {
        [HttpGet(Name = "GetProjects")]
        public async Task<ProjectDTO> Get()
        {
            
        }
    }
}
