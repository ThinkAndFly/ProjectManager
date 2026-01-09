using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ProjectManager.Application.DTO;
using ProjectManager.Application.Interfaces;
using ProjectManager.Domain.Entities;
using ProjectManager.Domain.Interfaces;
using ProjectManager.Infraestructure.Security;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Claims;

namespace ProjectManager.Application.Security
{
    public class SecurityService(IConfiguration config, IUserRepository userRepository, IPasswordHasher<User> passwordHasher) : ISecurityService
    {
        public async Task<string> Login(LoginDTO login)
        {
            var user = await userRepository.GetByUsernameAsync(login.Username);

            if (user is null)
                throw new InvalidCredentialException();

            var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, login.Password);

            if (result == PasswordVerificationResult.Failed)
                throw new InvalidCredentialException();

            return BuildToken(user);
        }

        private string BuildToken(User user)
        {
            var creds = BuildCredentials();
            var claims = BuildClaims(user);
            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private SigningCredentials BuildCredentials()
        {
            var privateKey = RSAKeyHelper.GetPrivateKey(config);
            return new SigningCredentials(privateKey, SecurityAlgorithms.RsaSha256);
        }

        private static IReadOnlyCollection<Claim> BuildClaims(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            return claims;
        }
    }
}
