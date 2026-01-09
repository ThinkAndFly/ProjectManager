using System.Threading.Tasks;
using ProjectManager.Application.DTO;

namespace ProjectManager.Application.Interfaces
{
    public interface IProjectService
    {
        Task<IEnumerable<ProjectDTO>> GetAsync(string? status, string? owner);

        Task<ProjectDTO?> GetByIdAsync(string id);

        Task<ProjectDTO> CreateAsync(ProjectDTO request);

        Task<ProjectDTO?> UpdateAsync(string id, ProjectDTO request);

        Task<bool> DeleteAsync(string id);

        Task<ProjectStatsDTO?> GetStatsByIdAsync(string id);
    }
}
