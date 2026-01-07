using ProjectManager.Domain.Enums;

namespace ProjectManager.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public RoleEnum Role { get; set; }
    }
}
