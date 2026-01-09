using ProjectManager.Application.DTO;

namespace ProjectManager.Application.Interfaces
{
    public interface IProjectService
    {
        Task<IEnumerable<ProjectDTO>> GetAsync(string? status, string? ownerId);

        Task<ProjectDTO?> GetByIdAsync(string id);

        Task<ProjectDTO> CreateAsync(ProjectDTO project);

        Task<ProjectDTO?> UpdateAsync(string id, ProjectDTO project);

        Task<bool> DeleteAsync(string id);
    }
}
