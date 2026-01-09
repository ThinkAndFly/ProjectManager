using AutoMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ProjectManager.Application.DTO;
using ProjectManager.Application.Projects;
using ProjectManager.Domain.Entities;
using ProjectManager.Domain.Interfaces;
using System.Threading;

namespace ProjectManager.Test.Projects
{
    [TestClass]
    public class ProjectServiceTests
    {
        private Mock<IProjectRepository> repositoryMock = null!;
        private Mock<IMapper> mapperMock = null!;
        private ProjectService service = null!;

        [TestInitialize]
        public void SetUp()
        {
            repositoryMock = new Mock<IProjectRepository>(MockBehavior.Strict);
            mapperMock = new Mock<IMapper>(MockBehavior.Strict);

            service = new ProjectService(repositoryMock.Object, mapperMock.Object);
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

            var dto = new ProjectDTO
            {
                Id = id,
                Name = "Test",
                Description = "Desc",
                OwnerId = "owner",
                Status = "Open"
            };

            repositoryMock
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(entity);

            mapperMock
                .Setup(m => m.Map<ProjectDTO>(entity))
                .Returns(dto);

            // Act
            var result = await service.GetByIdAsync(id);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(dto.Id, result!.Id);
            Assert.AreEqual(dto.Name, result.Name);

            repositoryMock.VerifyAll();
            mapperMock.VerifyAll();
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
            mapperMock.Verify(m => m.Map<ProjectDTO>(It.IsAny<Project>()), Times.Never);
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

            var dtos = new[]
            {
                new ProjectDTO
                {
                    Id = "p1",
                    Name = "P1",
                    Description = "D1",
                    OwnerId = ownerId,
                    Status = status
                },
                new ProjectDTO
                {
                    Id = "p2",
                    Name = "P2",
                    Description = "D2",
                    OwnerId = ownerId,
                    Status = status
                }
            };

            repositoryMock
                .Setup(r => r.GetAsync(status, ownerId))
                .ReturnsAsync(entities);

            mapperMock
                .Setup(m => m.Map<ProjectDTO>(entities[0]))
                .Returns(dtos[0]);

            mapperMock
                .Setup(m => m.Map<ProjectDTO>(entities[1]))
                .Returns(dtos[1]);

            // Act
            var result = (await service.GetAsync(status, ownerId)).ToArray();

            // Assert
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(dtos[0].Id, result[0].Id);
            Assert.AreEqual(dtos[1].Id, result[1].Id);

            repositoryMock.VerifyAll();
            mapperMock.VerifyAll();
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

            var mappedDto = new ProjectDTO
            {
                Id = createdEntity.Id,
                Name = createdEntity.Name,
                Description = createdEntity.Description,
                OwnerId = createdEntity.OwnerId,
                Status = createdEntity.Status
            };

            repositoryMock
                .Setup(r => r.CreateAsync(It.IsAny<Project>()))
                .Callback<Project>((p) => capturedEntity = p)
                .ReturnsAsync(createdEntity);

            mapperMock
                .Setup(m => m.Map<ProjectDTO>(createdEntity))
                .Returns(mappedDto);

            // Act
            var result = await service.CreateAsync(input);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(mappedDto.Id, result.Id);

            Assert.IsNotNull(capturedEntity);
            Assert.AreEqual(input.Name, capturedEntity!.Name);
            Assert.AreEqual(input.Description, capturedEntity.Description);
            Assert.AreEqual(input.OwnerId, capturedEntity.OwnerId);
            Assert.AreEqual(input.Status, capturedEntity.Status);
            Assert.IsFalse(string.IsNullOrWhiteSpace(capturedEntity.Id));

            repositoryMock.VerifyAll();
            mapperMock.VerifyAll();
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
            repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Project>()), Times.Never);
            mapperMock.Verify(m => m.Map<ProjectDTO>(It.IsAny<Project>()), Times.Never);
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

            var mappedDto = new ProjectDTO
            {
                Id = updatedEntity.Id,
                Name = updatedEntity.Name,
                Description = updatedEntity.Description,
                OwnerId = updatedEntity.OwnerId,
                Status = updatedEntity.Status
            };

            repositoryMock
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(existing);

            repositoryMock
                .Setup(r => r.UpdateAsync(It.IsAny<Project>()))
                .Callback<Project>(p => updatedPassedToRepo = p)
                .ReturnsAsync(updatedEntity);

            mapperMock
                .Setup(m => m.Map<ProjectDTO>(updatedEntity))
                .Returns(mappedDto);

            // Act
            var result = await service.UpdateAsync(id, dto);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(mappedDto.Id, result!.Id);
            Assert.AreEqual(dto.Name, updatedPassedToRepo!.Name);
            Assert.AreEqual(dto.Description, updatedPassedToRepo.Description);
            Assert.AreEqual(dto.OwnerId, updatedPassedToRepo.OwnerId);
            Assert.AreEqual(dto.Status, updatedPassedToRepo.Status);
            Assert.IsNotNull(updatedPassedToRepo.UpdatedAtUtc);

            repositoryMock.VerifyAll();
            mapperMock.VerifyAll();
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
