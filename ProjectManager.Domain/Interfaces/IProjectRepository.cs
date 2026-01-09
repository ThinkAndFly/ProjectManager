using ProjectManager.Domain.Entities;

namespace ProjectManager.Domain.Interfaces
{
    public interface IProjectRepository
    {
        Task<IEnumerable<Project>> GetAsync(string? status, string? ownerId);

        Task<Project?> GetByIdAsync(string id);

        Task<Project> CreateAsync(Project project);

        Task<Project?> UpdateAsync(Project project);

        Task<bool> DeleteAsync(string id);
    }
}