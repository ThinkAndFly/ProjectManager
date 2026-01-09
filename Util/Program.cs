// See https://aka.ms/new-console-template for more information

using Microsoft.AspNetCore.Identity;
using ProjectManager.Domain.Entities;
using ProjectManager.Domain.Enums;

var user = new User
{
    Id = 1,
    Name = "Test User",
    UserName = "testuser",
    Email = "testuser@example.com",
    Role = RoleEnum.Admin // or whatever default
};

var password = "Test@123"; // password you want for the seeded user

var hasher = new PasswordHasher<User>();
var hash = hasher.HashPassword(user, password);
Console.WriteLine(hash);
Console.ReadLine();
