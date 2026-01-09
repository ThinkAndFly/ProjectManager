using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ProjectManager.Application.DTO;
using ProjectManager.Application.MapProfiles;
using ProjectManager.Application.Projects;
using ProjectManager.Domain.Entities;
using ProjectManager.Domain.Interfaces;

namespace ProjectManager.Test.Projects
{
    [TestClass]
    public class ProjectServiceTests
    {
        private Mock<IProjectRepository> repositoryMock = null!;
        private IMapper mapper = null!; 
        private ProjectService service = null!;

        [TestInitialize]
        public void SetUp()
        {
            repositoryMock = new Mock<IProjectRepository>(MockBehavior.Strict);

            // Configure AutoMapper with your profile
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<AutoMapperProfile>();
            }, NullLoggerFactory.Instance);

            // Optionally validate configuration once
            config.AssertConfigurationIsValid();

            mapper = config.CreateMapper();

            service = new ProjectService(repositoryMock.Object, mapper);
        }

        [TestMethod]
        public async Task GetByIdAsync_ReturnsMappedDto_WhenProjectExists()
        {
            // Arrange
            var id = "project-1";
            var entity = new Project
            {
                Id = id,
                Name = "Test",
                Description = "Desc",
                OwnerId = "owner",
                Status = "Open",
                CreatedAtUtc = DateTime.UtcNow
            };

            repositoryMock
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(entity);

            // Act
            var result = await service.GetByIdAsync(id);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(entity.Id, result!.Id);
            Assert.AreEqual(entity.Name, result.Name);
            Assert.AreEqual(entity.Description, result.Description);
            Assert.AreEqual(entity.OwnerId, result.OwnerId);
            Assert.AreEqual(entity.Status, result.Status);

            repositoryMock.VerifyAll();
        }

        [TestMethod]
        public async Task GetByIdAsync_ReturnsNull_WhenProjectDoesNotExist()
        {
            // Arrange
            var id = "missing";
            repositoryMock
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync((Project?)null);

            // Act
            var result = await service.GetByIdAsync(id);

            // Assert
            Assert.IsNull(result);
            repositoryMock.VerifyAll();
        }

        [TestMethod]
        public async Task GetAsync_ReturnsMappedDtos()
        {
            // Arrange
            var status = "Open";
            var ownerId = "owner";

            var entities = new[]
            {
                new Project
                {
                    Id = "p1",
                    Name = "P1",
                    Description = "D1",
                    OwnerId = ownerId,
                    Status = status,
                    CreatedAtUtc = DateTime.UtcNow
                },
                new Project
                {
                    Id = "p2",
                    Name = "P2",
                    Description = "D2",
                    OwnerId = ownerId,
                    Status = status,
                    CreatedAtUtc = DateTime.UtcNow
                }
            };

            repositoryMock
                .Setup(r => r.GetAsync(status, ownerId))
                .ReturnsAsync(entities);

            // Act
            var result = (await service.GetAsync(status, ownerId)).ToArray();

            // Assert
            Assert.HasCount(2, result);
            Assert.AreEqual("p1", result[0].Id);
            Assert.AreEqual("p2", result[1].Id);

            repositoryMock.VerifyAll();
        }

        [TestMethod]
        public async Task CreateAsync_CreatesProjectAndReturnsDto()
        {
            // Arrange
            var input = new ProjectDTO
            {
                Name = "New",
                Description = "Desc",
                OwnerId = "owner",
                Status = "Open"
            };

            Project? capturedEntity = null;

            var createdEntity = new Project
            {
                Id = "generated-id",
                Name = input.Name,
                Description = input.Description,
                OwnerId = input.OwnerId,
                Status = input.Status,
                CreatedAtUtc = DateTime.UtcNow
            };

            repositoryMock
                .Setup(r => r.CreateAsync(It.IsAny<Project>()))
                .Callback<Project>(p => capturedEntity = p)
                .ReturnsAsync(createdEntity);

            // Act – this will use ProjectService + real AutoMapper
            var result = await service.CreateAsync(input);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(createdEntity.Id, result.Id);
            Assert.AreEqual(createdEntity.Name, result.Name);
            Assert.AreEqual(createdEntity.Description, result.Description);
            Assert.AreEqual(createdEntity.OwnerId, result.OwnerId);
            Assert.AreEqual(createdEntity.Status, result.Status);

            Assert.IsNotNull(capturedEntity);
            Assert.AreEqual(input.Name, capturedEntity!.Name);
            Assert.AreEqual(input.Description, capturedEntity.Description);
            Assert.AreEqual(input.OwnerId, capturedEntity.OwnerId);
            Assert.AreEqual(input.Status, capturedEntity.Status);
            Assert.IsFalse(string.IsNullOrWhiteSpace(capturedEntity.Id));

            repositoryMock.VerifyAll();
        }

        [TestMethod]
        public async Task UpdateAsync_ReturnsNull_WhenProjectDoesNotExist()
        {
            // Arrange
            var id = "missing";
            var dto = new ProjectDTO
            {
                Name = "Updated",
                Description = "Updated desc",
                OwnerId = "owner",
                Status = "Closed"
            };

            repositoryMock
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync((Project?)null);

            // Act
            var result = await service.UpdateAsync(id, dto);

            // Assert
            Assert.IsNull(result);
            repositoryMock.VerifyAll();
        }

        [TestMethod]
        public async Task UpdateAsync_UpdatesAndReturnsDto_WhenProjectExists()
        {
            // Arrange
            var id = "project-1";
            var existing = new Project
            {
                Id = id,
                Name = "Old",
                Description = "Old desc",
                OwnerId = "old-owner",
                Status = "Open",
                CreatedAtUtc = DateTime.UtcNow.AddDays(-1)
            };

            var dto = new ProjectDTO
            {
                Name = "New",
                Description = "New desc",
                OwnerId = "owner",
                Status = "Closed"
            };

            Project? updatedPassedToRepo = null;

            var updatedEntity = new Project
            {
                Id = id,
                Name = dto.Name,
                Description = dto.Description,
                OwnerId = dto.OwnerId,
                Status = dto.Status,
                CreatedAtUtc = existing.CreatedAtUtc,
                UpdatedAtUtc = DateTime.UtcNow
            };

            repositoryMock
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(existing);

            repositoryMock
                .Setup(r => r.UpdateAsync(It.IsAny<Project>()))
                .Callback<Project>(p => updatedPassedToRepo = p)
                .ReturnsAsync(updatedEntity);

            // Act
            var result = await service.UpdateAsync(id, dto);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(updatedEntity.Id, result!.Id);
            Assert.AreEqual(updatedEntity.Name, result.Name);
            Assert.AreEqual(updatedEntity.Description, result.Description);
            Assert.AreEqual(updatedEntity.OwnerId, result.OwnerId);
            Assert.AreEqual(updatedEntity.Status, result.Status);

            Assert.IsNotNull(updatedPassedToRepo);
            Assert.AreEqual(dto.Name, updatedPassedToRepo!.Name);
            Assert.AreEqual(dto.Description, updatedPassedToRepo.Description);
            Assert.AreEqual(dto.OwnerId, updatedPassedToRepo.OwnerId);
            Assert.AreEqual(dto.Status, updatedPassedToRepo.Status);
            Assert.IsNotNull(updatedPassedToRepo.UpdatedAtUtc);

            repositoryMock.VerifyAll();
        }

        [TestMethod]
        public async Task DeleteAsync_ReturnsRepositoryResult()
        {
            // Arrange
            var id = "project-1";
            repositoryMock
                .Setup(r => r.DeleteAsync(id))
                .ReturnsAsync(true);

            // Act
            var result = await service.DeleteAsync(id);

            // Assert
            Assert.IsTrue(result);
            repositoryMock.VerifyAll();
        }
    }
}
