using Microsoft.EntityFrameworkCore;
using ProjectManager.Domain.Entities;
using ProjectManager.Domain.Interfaces;

namespace ProjectManager.Infraestructure.Persistence.EF
{
    public class EfUserRepository(ProjectManagerDbContext dbContext) : IUserRepository
    {
        public async Task<User?> GetByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            return await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserName == username);
        }
    }
}