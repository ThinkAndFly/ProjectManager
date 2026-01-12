using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManager.Application.DTO;
using ProjectManager.Application.Interfaces;
using ProjectManager.Domain.Interfaces;

namespace ProjectManager.Controllers
{
    [ApiController]
    [Route("api/projects")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ProjectController(IProjectService projectService, IMessagePublisher messagePublisher) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectDTO>>> GetAll([FromQuery] string? status, [FromQuery] string? owner)
        {
            var projects = await projectService.GetAsync(status, owner);
            return Ok(projects);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectDTO>> GetById(string id)
        {
            var project = await projectService.GetByIdAsync(id);
            if (project is null)
                return NotFound();

            return Ok(project);
        }

        [HttpGet("{id}/stats")]
        public async Task<ActionResult<ProjectStatsDTO>> GetStats(string id)
        {
            var stats = await projectService.GetStatsByIdAsync(id);
            if (stats is null)
                return NotFound();

            return Ok(stats);
        }

        [HttpPost]
        public async Task<ActionResult<ProjectDTO>> Create([FromBody] ProjectDTO request)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var created = await projectService.CreateAsync(request);
            await messagePublisher.PublishAsync("ProjectCreated");

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ProjectDTO>> Update(string id, [FromBody] ProjectDTO request)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var updated = await projectService.UpdateAsync(id, request);
            if (updated is null)
                return NotFound();

            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var deleted = await projectService.DeleteAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}
