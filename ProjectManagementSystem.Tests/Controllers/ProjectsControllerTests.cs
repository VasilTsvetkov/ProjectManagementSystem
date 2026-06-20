namespace ProjectManagementSystem.Tests.Controllers
{
    using BL.DTOs.Projects;
    using BL.Interfaces;
    using BL.Models;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using ProjectManagementSystem.Web.ViewModels.Projects;
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Web.Controllers;
    using Xunit;

    public class ProjectsControllerTests
    {
        private readonly Mock<IProjectService> _mockProjectService;
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly ProjectsController _controller;

        public ProjectsControllerTests()
        {
            _mockProjectService = new Mock<IProjectService>();

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            _controller = new ProjectsController(_mockProjectService.Object, _mockUserManager.Object);
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WithListOfProjectDisplayViewModels()
        {
            var dtos = new List<ProjectDisplayDto>
            {
                new() { Id = 1, Number = 101, Name = "P1", Tag = "TAG", Description = "D1", CreatedAt = DateTime.UtcNow }
            };
            _mockProjectService.Setup(s => s.GetAllProjectsAsync()).ReturnsAsync(dtos);

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<ProjectDisplayViewModel>>(viewResult.ViewData.Model);
            Assert.Single(model);
        }

        [Fact]
        public void Create_Get_ReturnsViewResult()
        {
            var result = _controller.Create();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Create_Post_ReturnsViewWithModel_WhenModelStateIsInvalid()
        {
            _controller.ModelState.AddModelError("Name", "Required");
            var model = new ProjectViewModel { Name = "Invalid", Description = "Test" };

            var result = await _controller.Create(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
        }

        [Fact]
        public async Task Create_Post_ReturnsUnauthorized_WhenUserIdIsNull()
        {
            var model = new ProjectViewModel { Name = "New Project" };
            _mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns((string?)null);

            var result = await _controller.Create(model);

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task Create_Post_RedirectsToIndex_WhenSuccessful()
        {
            var model = new ProjectViewModel { Name = "New Project", Description = "Desc" };
            _mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user-123");

            var result = await _controller.Create(model);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(ProjectsController.Index), redirectResult.ActionName);
            _mockProjectService.Verify(s => s.CreateProjectAsync(It.IsAny<ProjectDto>(), "user-123"), Times.Once);
        }

        [Fact]
        public async Task Edit_Get_ReturnsNotFound_WhenProjectDoesNotExist()
        {
            _mockProjectService.Setup(s => s.GetProjectForEditAsync(1)).ReturnsAsync((ProjectDto?)null);

            var result = await _controller.Edit(1);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Get_ReturnsViewResult_WithProjectViewModel()
        {
            var dto = new ProjectDto { Name = "Existing", Description = "Desc" };
            _mockProjectService.Setup(s => s.GetProjectForEditAsync(1)).ReturnsAsync(dto);

            var result = await _controller.Edit(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectViewModel>(viewResult.Model);
            Assert.Equal(dto.Name, model.Name);
        }

        [Fact]
        public async Task Edit_Post_ReturnsViewWithModel_WhenModelStateIsInvalid()
        {
            _controller.ModelState.AddModelError("Name", "Required");
            var model = new ProjectViewModel { Name = "Invalid", Description = "Desc" };

            var result = await _controller.Edit(1, model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
        }

        [Fact]
        public async Task Edit_Post_ReturnsUnauthorized_WhenUserIdIsNull()
        {
            var model = new ProjectViewModel { Name = "Edit" };
            var context = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext { HttpContext = context };

            var result = await _controller.Edit(1, model);

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task Edit_Post_ReturnsNotFound_WhenUpdateFails()
        {
            var model = new ProjectViewModel { Name = "Edit", Description = "Desc" };
            var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "user-123")]));
            var context = new DefaultHttpContext { User = user };
            _controller.ControllerContext = new ControllerContext { HttpContext = context };

            _mockProjectService.Setup(s => s.UpdateProjectAsync(1, It.IsAny<ProjectDto>(), "user-123")).ReturnsAsync(false);

            var result = await _controller.Edit(1, model);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_RedirectsToIndex_WhenSuccessful()
        {
            var model = new ProjectViewModel { Name = "Edit", Description = "Desc" };
            var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "user-123")]));
            var context = new DefaultHttpContext { User = user };
            _controller.ControllerContext = new ControllerContext { HttpContext = context };

            _mockProjectService.Setup(s => s.UpdateProjectAsync(1, It.IsAny<ProjectDto>(), "user-123")).ReturnsAsync(true);

            var result = await _controller.Edit(1, model);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(ProjectsController.Index), redirectResult.ActionName);
        }

        [Fact]
        public async Task Delete_Post_ReturnsUnauthorized_WhenUserIdIsNull()
        {
            var context = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext { HttpContext = context };

            var result = await _controller.Delete(1);

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task Delete_Post_ReturnsNotFound_WhenDeleteFails()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "user-123")]));
            var context = new DefaultHttpContext { User = user };
            _controller.ControllerContext = new ControllerContext { HttpContext = context };

            _mockProjectService.Setup(s => s.DeleteProjectAsync(1, "user-123")).ReturnsAsync(false);

            var result = await _controller.Delete(1);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_Post_RedirectsToIndex_WhenSuccessful()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "user-123")]));
            var context = new DefaultHttpContext { User = user };
            _controller.ControllerContext = new ControllerContext { HttpContext = context };

            _mockProjectService.Setup(s => s.DeleteProjectAsync(1, "user-123")).ReturnsAsync(true);

            var result = await _controller.Delete(1);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(ProjectsController.Index), redirectResult.ActionName);
        }
    }
}