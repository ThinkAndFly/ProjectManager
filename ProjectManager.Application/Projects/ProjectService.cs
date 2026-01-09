using AutoMapper;
using ProjectManager.Application.DTO;
using ProjectManager.Application.Interfaces;
using ProjectManager.Domain.Entities;
using ProjectManager.Domain.Interfaces;

namespace ProjectManager.Application.Projects
{
    public class ProjectService(IProjectRepository repository, IMapper mapper) : IProjectService
    {
        public async Task<IEnumerable<ProjectDTO>> GetAsync(string? status, string? ownerId)
        {
            var projects = await repository.GetAsync(status, ownerId);
            return projects.Select(mapper.Map<ProjectDTO>);
        }

        public async Task<ProjectDTO?> GetByIdAsync(string id)
        {
            var project = await repository.GetByIdAsync(id);
            return project is null ? null : mapper.Map<ProjectDTO>(project);
        }

        public async Task<ProjectDTO> CreateAsync(ProjectDTO project)
        {
            ArgumentNullException.ThrowIfNull(project);

            var entity = new Project
            {
                Id = Guid.NewGuid().ToString("N"),
                Name = project.Name,
                Description = project.Description,
                OwnerId = project.OwnerId,
                Status = project.Status,
                CreatedAtUtc = DateTime.UtcNow
            };

            var created = await repository.CreateAsync(entity);
            return mapper.Map<ProjectDTO>(created);
        }

        public async Task<ProjectDTO?> UpdateAsync(string id, ProjectDTO project)
        {
            ArgumentNullException.ThrowIfNull(project);

            var existing = await repository.GetByIdAsync(id);
            if (existing is null)
            {
                return null;
            }

            existing.Name = project.Name;
            existing.Description = project.Description;
            existing.OwnerId = project.OwnerId;
            existing.Status = project.Status;
            existing.UpdatedAtUtc = DateTime.UtcNow;

            var updated = await repository.UpdateAsync(existing);
            if (updated is null)
            {
                return null;
            }

            return mapper.Map<ProjectDTO>(updated);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var deleted = await repository.DeleteAsync(id);
            return deleted;
        }

        public async Task<ProjectStatsDTO?> GetStatsByIdAsync(string id)
        {
            var project = await GetByIdAsync(id);
            if (project is null)
                return null;

            var now = DateTime.UtcNow;
            var lastUpdate = project.UpdatedAtUtc ?? project.CreatedAtUtc;

            var end = lastUpdate > now ? lastUpdate : now;
            var daysActive = (end.Date - project.CreatedAtUtc.Date).Days;
            if (daysActive < 0)
                daysActive = 0;

            return new ProjectStatsDTO
            {
                ProjectId = project.Id,
                CreatedAtUtc = project.CreatedAtUtc,
                LastUpdateUtc = lastUpdate,
                DaysActive = daysActive,
                Status = project.Status
            };
        }
    }
}
