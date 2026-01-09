using ProjectManager.Domain.Entities;

namespace ProjectManager.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username);
    }
}
