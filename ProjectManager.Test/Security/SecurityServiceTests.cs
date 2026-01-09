using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ProjectManager.Application.DTO;
using ProjectManager.Application.Security;
using ProjectManager.Domain.Entities;
using ProjectManager.Domain.Enums;
using ProjectManager.Domain.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Threading;

namespace ProjectManager.Test.Security
{
    [TestClass]
    public class SecurityServiceTests
    {
        private IConfiguration configuration = null!;
        private Mock<IUserRepository> userRepositoryMock = null!;
        private Mock<IPasswordHasher<User>> passwordHasherMock = null!;
        private SecurityService service = null!;

        [TestInitialize]
        public void SetUp()
        {
            // In-memory configuration, simulating your API appsettings
            // Use your real test key here. It must be a valid PEM PKCS#8 private key string.
            var jwtPrivateKeyPem = "-----BEGIN PRIVATE KEY-----\nMIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQDGy3jIStJHu9/a\nDo3jYUceqyURzOF82gUqgVSzjOk8eq1TRG7Jsyh01wf0NTUArucvhiXPsGBuP5SG\n5WB9oRZe3vqsL9WOxEHTqaYkiICvIT+HZ5QbSAMqb/8QhW4nzWXvVJPJKf+2Yg15\ntFsQHtPnYrnYbAT5EujbwmEugoli3Sxd2DaDOmeu9977ZWfehAUqQkJN09Y3C1sP\n6wmPR4GjmaBsiZ0Tfn8mezmYQCpjCKhDR99mVKV/wek8SAlZtH4DuclV+JLOTHKI\njLON7VZQy/VW6tvE9h1JI6Xk34ODBLY/qlVIIGZiX3rBvqOdjaSMllRBvn1sdUyG\n0YxTUSeLAgMBAAECggEABFmLaQaKF1u7CDBtVl5YiglApGURQgQbPNTbn5ojuFkO\n1dWXfv5WkkfqOqO3Zy5sjJOo3CSF16O4gkMem2Ec9jJ21bGuQJN2xUTfB8mc1zgp\nBbN0gCxRNWqB8ECbKm/KHTSCj1JF8B2xIcqae33RMSzt2Bh0+2Y4hiZ4reXZT7px\nQ+U/QqQdRSyKxDjHMHCjdrrsDCPj7b7eXO03XSTwt3aDrVR/mFidvlIWGfJwElZ3\ntSlXD4Ex+eKAaSzZBP6Omn6A8C89SDbVoYe6/SIew3EvQv9uwn87fFNFg5M3BOZr\nILfQHOiDKAzHcwkuirHeRdNRJxK5P5lcVIKi0Fx3cQKBgQD5hY8/Q9n9cVBbL+GM\nxg9WdJFIHzPxbRRJDL0NppFyYBJ/eiqikb7R6skq0qTDnyQTPY0HUULBfhrOxZ2G\nOkUHXtpeYQqCEH2PX7YhuEyZKUokGLO/Oc10VOYaZqlbhndlFjFcsG4wR7x+Ir6x\nnpkxGvo5znlI06E6x9seoOV6NQKBgQDL9MHMELcxPbAYMEhVs3F8erXWScVe9nQE\nH9F1PUav3HrJJFNjU6cGU6XXyAKVZkBViO9KL3IHKTMC9NWt/wUi6INm6AtwK2ZN\nIuHnA30NkTN/zfrQgYUjJE5ayLQWHSSyyflbuMofJyUKOMXSmR/WZkH0LU/b/h60\nOUGHOd5SvwKBgDVWSGWUom9bnnqvhH4sBDFN35RUHy1XTMPEtlDJr0OMp8eaHKz9\njJWgo3nE5zVtui9ms9PBmgx0YVSbx21e3UyTCQito6pjzgMsyWjx1WXT/qYypZGV\n0IYyc7FnCoKm/rScBtcyW4t0eiVYVfzv0v09MAnVSfW4TzmaaQtmB0eVAoGAfyDW\n023v4h+DbfBahiDNsjuCsElXXzPbaN60XpGNR/z0BABCgf0YdRcann+rLJiJrUcn\ncGRWfSQvCb62mjgFaZbooPIufwJuR9JgYPCJuDUzlow7tE+nPxpYRLoplkcgItlG\nNufeBMEPk2mD1Rtg/vDKV3sO9h9V4Bx4PePSMl0CgYEA3c6lfO3E7Gs062HFQbJw\nBDJyErG1QkFarykaPP7nGpRHyL+Ht08IpIjyoIiVRcU7ZzRKKCT5YqHQjCdWogrg\nOVGcsX2IuaNkMaerwJHDgqzRMdNrJ92bZ7Qzz48WSIpNmNSFDctAzZNfClVPPj/D\n9yExqDBoTE+8ipMDJeg7vfM=\n-----END PRIVATE KEY-----";

            configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "Jwt:Issuer", "issuer" },
                    { "Jwt:Audience", "audience" },
                    { "Jwt:PrivateKeyPem", jwtPrivateKeyPem },
                    // add Jwt:PublicKeyPem or others if RSAKeyHelper needs them
                })
                .Build();

            userRepositoryMock = new Mock<IUserRepository>(MockBehavior.Strict);
            passwordHasherMock = new Mock<IPasswordHasher<User>>(MockBehavior.Strict);

            service = new SecurityService(configuration, userRepositoryMock.Object, passwordHasherMock.Object);
        }

        [TestMethod]
        public async Task Login_ThrowsInvalidCredentialException_WhenUserNotFound()
        {
            // Arrange
            var login = new LoginDTO
            {
                Username = "unknown",
                Password = "pwd"
            };

            userRepositoryMock
                .Setup(r => r.GetByUsernameAsync(login.Username))
                .ReturnsAsync((User?)null);

            // Act / Assert
            await Assert.ThrowsAsync<InvalidCredentialException>(() => service.Login(login));
        }

        [TestMethod]
        public async Task Login_ThrowsInvalidCredentialException_WhenPasswordVerificationFails()
        {
            // Arrange
            var login = new LoginDTO
            {
                Username = "user",
                Password = "wrong"
            };

            var user = new User
            {
                Id = 1,
                UserName = login.Username,
                Name = "User",
                Email = "user@test.com",
                Role = RoleEnum.User,
                PasswordHash = "hash"
            };

            userRepositoryMock
                .Setup(r => r.GetByUsernameAsync(login.Username))
                .ReturnsAsync(user);

            passwordHasherMock
                .Setup(h => h.VerifyHashedPassword(user, user.PasswordHash, login.Password))
                .Returns(PasswordVerificationResult.Failed);

            // Act / Assert
            await Assert.ThrowsAsync<InvalidCredentialException>(() => service.Login(login));
        }

        [TestMethod]
        public async Task Login_ReturnsToken_WhenCredentialsAreValid()
        {
            // Arrange
            var login = new LoginDTO
            {
                Username = "user",
                Password = "correct"
            };

            var user = new User
            {
                Id = 1,
                UserName = login.Username,
                Name = "User",
                Email = "user@test.com",
                Role = RoleEnum.User,
                PasswordHash = "hash"
            };

            userRepositoryMock
                .Setup(r => r.GetByUsernameAsync(login.Username))
                .ReturnsAsync(user);

            passwordHasherMock
                .Setup(h => h.VerifyHashedPassword(user, user.PasswordHash, login.Password))
                .Returns(PasswordVerificationResult.Success);

            // Act
            var tokenString = await service.Login(login);

            // Assert
            Assert.IsFalse(string.IsNullOrWhiteSpace(tokenString));

            var handler = new JwtSecurityTokenHandler();
            Assert.IsTrue(handler.CanReadToken(tokenString));

            userRepositoryMock.VerifyAll();
            passwordHasherMock.VerifyAll();
        }
    }
}
