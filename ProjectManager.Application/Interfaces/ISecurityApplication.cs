using ProjectManager.Application.DTO;
using System.Security.Claims;

namespace ProjectManager.Application.Interfaces
{
    public interface ISecurityApplication
    {
        public Task<string> Login(LoginDTO login);
    }
}
