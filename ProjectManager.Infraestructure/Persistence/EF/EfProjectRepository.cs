using Microsoft.EntityFrameworkCore;
using ProjectManager.Domain.Entities;
using ProjectManager.Domain.Interfaces;

namespace ProjectManager.Infraestructure.Persistence.EF
{
    public class EfProjectRepository(ProjectManagerDbContext dbContext) : IProjectRepository
    {
        public async Task<IEnumerable<Project>> GetAsync(string? status, string? ownerId)
        {
            var query = dbContext.Projects.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(p => p.Status == status);

            if (!string.IsNullOrWhiteSpace(ownerId))
                query = query.Where(p => p.OwnerId == ownerId);

            return await query.ToListAsync();
        }

        public async Task<Project?> GetByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;

            return await dbContext.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Project> CreateAsync(Project project)
        {
            if (string.IsNullOrWhiteSpace(project.Id))
                project.Id = Guid.NewGuid().ToString("N");

            project.CreatedAtUtc = DateTime.UtcNow;
            dbContext.Projects.Add(project);
            await dbContext.SaveChangesAsync();
            return project;
        }

        public async Task<Project?> UpdateAsync(Project project)
        {
            var existing = await dbContext.Projects.FirstOrDefaultAsync(p => p.Id == project.Id);
            if (existing is null)
                return null;

            existing.Name = project.Name;
            existing.Description = project.Description;
            existing.OwnerId = project.OwnerId;
            existing.Status = project.Status;
            existing.UpdatedAtUtc = project.UpdatedAtUtc ?? DateTime.UtcNow;

            await dbContext.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var existing = await dbContext.Projects.FirstOrDefaultAsync(p => p.Id == id);
            if (existing is null)
                return false;

            dbContext.Projects.Remove(existing);
            await dbContext.SaveChangesAsync();
            return true;
        }
    }
}