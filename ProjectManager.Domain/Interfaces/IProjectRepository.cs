using ProjectManager.Domain.Entities;

namespace ProjectManager.Domain.Interfaces
{
    public interface IProjectRepository
    {
        Task<IEnumerable<Project>> GetAsync(string? status, string? ownerId, CancellationToken cancellationToken = default);

        Task<Project?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

        Task<Project> CreateAsync(Project project, CancellationToken cancellationToken = default);

        Task<Project?> UpdateAsync(Project project, CancellationToken cancellationToken = default);

        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
    }
}