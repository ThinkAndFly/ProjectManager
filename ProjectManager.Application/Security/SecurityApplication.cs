using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ProjectManager.Application.DTO;
using ProjectManager.Application.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ProjectManager.Application.Security
{
    public class SecurityApplication(IConfiguration config) : ISecurityApplication
    {
        public async Task<string> Login(LoginDTO login)
        {
            var privateKey = RSAKeyHelper.GetPrivateKey(config);
            var creds = new SigningCredentials(privateKey, SecurityAlgorithms.RsaSha256);

            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
