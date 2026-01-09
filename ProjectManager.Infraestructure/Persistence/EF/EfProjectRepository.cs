using Microsoft.EntityFrameworkCore;
using ProjectManager.Domain.Entities;
using ProjectManager.Domain.Interfaces;

namespace ProjectManager.Infraestructure.Persistence.EF
{
    public class EfProjectRepository(ProjectManagerDbContext dbContext) : IProjectRepository
    {
        public async Task<IEnumerable<Project>> GetAsync(string? status, string? ownerId, CancellationToken cancellationToken = default)
        {
            var query = dbContext.Projects.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(p => p.Status == status);

            if (!string.IsNullOrWhiteSpace(ownerId))
                query = query.Where(p => p.OwnerId == ownerId);

            return await query.ToListAsync(cancellationToken);
        }

        public async Task<Project?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;

            return await dbContext.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<Project> CreateAsync(Project project, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(project.Id))
                project.Id = Guid.NewGuid().ToString("N");

            project.CreatedAtUtc = DateTime.UtcNow;
            dbContext.Projects.Add(project);
            await dbContext.SaveChangesAsync(cancellationToken);
            return project;
        }

        public async Task<Project?> UpdateAsync(Project project, CancellationToken cancellationToken = default)
        {
            var existing = await dbContext.Projects.FirstOrDefaultAsync(p => p.Id == project.Id, cancellationToken);
            if (existing is null)
                return null;

            existing.Name = project.Name;
            existing.Description = project.Description;
            existing.OwnerId = project.OwnerId;
            existing.Status = project.Status;
            existing.UpdatedAtUtc = project.UpdatedAtUtc ?? DateTime.UtcNow;

            await dbContext.SaveChangesAsync(cancellationToken);
            return existing;
        }

        public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            var existing = await dbContext.Projects.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
            if (existing is null)
                return false;

            dbContext.Projects.Remove(existing);
            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}