namespace ProjectManagementSystem.Tests.Services
{
    using BL.Constants;
    using BL.DTOs.Projects;
    using BL.Enums;
    using BL.Interfaces;
    using BL.Models;
    using BL.Services;
    using Microsoft.Extensions.Logging;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    public class ProjectServiceTests
    {
        private readonly Mock<IProjectRepository> _mockProjectRepository;
        private readonly Mock<IActivityService> _mockActivityService;
        private readonly Mock<ILogger<ProjectService>> _mockLogger;
        private readonly ProjectService _service;

        public ProjectServiceTests()
        {
            _mockProjectRepository = new Mock<IProjectRepository>();
            _mockActivityService = new Mock<IActivityService>();
            _mockLogger = new Mock<ILogger<ProjectService>>();

            _service = new ProjectService(
                _mockProjectRepository.Object,
                _mockActivityService.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task GetAllProjectsAsync_ReturnsMappedProjectDisplayDtos()
        {
            var projects = new List<Project>
            {
                new() { Id = 1, Number = 101, Name = "Project One", Description = "Desc One", CreatedAt = DateTime.UtcNow, CreatorId = "u1" },
                new() { Id = 2, Number = 102, Name = "Project Two", Description = null, CreatedAt = DateTime.UtcNow, CreatorId = "u1" }
            };
            _mockProjectRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(projects);

            var result = await _service.GetAllProjectsAsync();

            var list = Assert.IsAssignableFrom<IEnumerable<ProjectDisplayDto>>(result).ToList();
            Assert.Equal(2, list.Count);
            Assert.Equal("Project One", list[0].Name);
            Assert.Equal(101, list[0].Number);
            Assert.Equal("Project Two", list[1].Name);
            Assert.Null(list[1].Description);
        }

        [Fact]
        public async Task GetProjectForEditAsync_ReturnsNull_WhenProjectNotFound()
        {
            _mockProjectRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Project?)null);

            var result = await _service.GetProjectForEditAsync(1);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetProjectForEditAsync_ReturnsProjectDto_WhenProjectExists()
        {
            var project = new Project { Id = 1, Number = 101, Name = "Edit Me", Description = "Edit Desc", CreatedAt = DateTime.UtcNow, CreatorId = "u1" };
            _mockProjectRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(project);

            var result = await _service.GetProjectForEditAsync(1);

            var dto = Assert.IsType<ProjectDto>(result);
            Assert.Equal("Edit Me", dto.Name);
            Assert.Equal("Edit Desc", dto.Description);
        }

        [Fact]
        public async Task GetProjectForDeleteAsync_ReturnsNull_WhenProjectNotFound()
        {
            _mockProjectRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Project?)null);

            var result = await _service.GetProjectForDeleteAsync(1);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetProjectForDeleteAsync_ReturnsProjectDisplayDto_WhenProjectExists()
        {
            var project = new Project { Id = 1, Number = 105, Name = "Delete Me", Description = "Del", CreatedAt = DateTime.UtcNow, CreatorId = "u1" };
            _mockProjectRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(project);

            var result = await _service.GetProjectForDeleteAsync(1);

            var dto = Assert.IsType<ProjectDisplayDto>(result);
            Assert.Equal(1, dto.Id);
            Assert.Equal(105, dto.Number);
            Assert.Equal("Delete Me", dto.Name);
            Assert.Equal("Del", dto.Description);
        }

        [Fact]
        public async Task CreateProjectAsync_SavesProjectAndLogsActivity()
        {
            var dto = new ProjectDto { Name = "New Project", Description = "New Desc" };
            _mockProjectRepository.Setup(r => r.AddAsync(It.IsAny<Project>())).Returns(Task.CompletedTask);

            var result = await _service.CreateProjectAsync(dto, "user-1");

            Assert.True(result);
            _mockProjectRepository.Verify(r => r.AddAsync(It.Is<Project>(p => p.Name == "New Project" && p.CreatorId == "user-1" && p.Description == "New Desc")), Times.Once);
            _mockActivityService.Verify(a => a.LogAsync("user-1", It.Is<string>(s => s.Contains("New Project")), ActivityType.ProjectAction), Times.Once);
        }

        [Fact]
        public async Task CreateProjectAsync_UsesUntitledName_WhenNameIsEmpty()
        {
            var dto = new ProjectDto { Name = " ", Description = "Desc" };

            var result = await _service.CreateProjectAsync(dto, "user-1");

            Assert.True(result);
            _mockProjectRepository.Verify(r => r.AddAsync(It.Is<Project>(p => p.Name == MessageConstants.UntitledProject)), Times.Once);
            _mockActivityService.Verify(a => a.LogAsync("user-1", It.Is<string>(s => s.Contains(MessageConstants.UntitledProject)), ActivityType.ProjectAction), Times.Once);
        }

        [Fact]
        public async Task UpdateProjectAsync_ReturnsFalse_WhenRepositoryUpdateFails()
        {
            var dto = new ProjectDto { Name = "Updated", Description = "Desc" };
            _mockProjectRepository.Setup(r => r.UpdateProjectAsync(1, "Updated", "Desc")).ReturnsAsync(false);

            var result = await _service.UpdateProjectAsync(1, dto, "user-1");

            Assert.False(result);
            _mockActivityService.Verify(a => a.LogAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ActivityType>()), Times.Never);
        }

        [Fact]
        public async Task UpdateProjectAsync_ReturnsTrueAndLogsActivity_WhenSuccessful()
        {
            var dto = new ProjectDto { Name = "Updated", Description = "Desc" };
            _mockProjectRepository.Setup(r => r.UpdateProjectAsync(1, "Updated", "Desc")).ReturnsAsync(true);

            var result = await _service.UpdateProjectAsync(1, dto, "user-1");

            Assert.True(result);
            _mockActivityService.Verify(a => a.LogAsync("user-1", It.Is<string>(s => s.Contains("Updated")), ActivityType.ProjectAction), Times.Once);
        }

        [Fact]
        public async Task UpdateProjectAsync_UsesUntitledName_WhenNameIsEmpty()
        {
            var dto = new ProjectDto { Name = string.Empty, Description = "Desc" };
            _mockProjectRepository.Setup(r => r.UpdateProjectAsync(1, MessageConstants.UntitledProject, "Desc")).ReturnsAsync(true);

            var result = await _service.UpdateProjectAsync(1, dto, "user-1");

            Assert.True(result);
            _mockProjectRepository.Verify(r => r.UpdateProjectAsync(1, MessageConstants.UntitledProject, "Desc"), Times.Once);
        }

        [Fact]
        public async Task DeleteProjectAsync_ReturnsFalse_WhenRepositoryDeleteFails()
        {
            _mockProjectRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Project?)null);
            _mockProjectRepository.Setup(r => r.DeleteAsync(1)).ReturnsAsync(false);

            var result = await _service.DeleteProjectAsync(1, "user-1");

            Assert.False(result);
            _mockActivityService.Verify(a => a.LogAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ActivityType>()), Times.Never);
        }

        [Fact]
        public async Task DeleteProjectAsync_ReturnsTrueAndLogsActivity_WhenSuccessful()
        {
            var project = new Project { Id = 1, Number = 101, Name = "To Delete", CreatedAt = DateTime.UtcNow, CreatorId = "u1" };
            _mockProjectRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(project);
            _mockProjectRepository.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

            var result = await _service.DeleteProjectAsync(1, "user-1");

            Assert.True(result);
            _mockActivityService.Verify(a => a.LogAsync("user-1", It.Is<string>(s => s.Contains("To Delete")), ActivityType.ProjectAction), Times.Once);
        }
    }
}