using ProjectManager.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectManager.Application.DTO
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public RoleEnum Role { get; set; }
    }
}
