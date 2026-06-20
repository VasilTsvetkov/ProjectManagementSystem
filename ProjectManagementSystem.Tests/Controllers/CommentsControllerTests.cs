namespace ProjectManagementSystem.Tests.Controllers
{
    using BL.Constants;
    using BL.DTOs;
    using BL.Interfaces;
    using BL.Models;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Web.Controllers;
    using Web.ViewModels.Comments;
    using Xunit;

    public class CommentsControllerTests
    {
        private readonly Mock<ICommentService> _mockCommentService;
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly CommentsController _controller;

        public CommentsControllerTests()
        {
            _mockCommentService = new Mock<ICommentService>();

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            _controller = new CommentsController(_mockCommentService.Object, _mockUserManager.Object);
        }

        [Fact]
        public async Task Create_Post_RedirectsToDetails_WhenModelStateIsInvalid()
        {
            _controller.ModelState.AddModelError("Content", "Required");
            var model = new CommentViewModel { ProjectId = 1, TaskId = 2, Content = "" };

            var result = await _controller.Create(model);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(TaskConstants.DetailsAction, redirectResult.ActionName);
            Assert.Equal(TaskConstants.Controller, redirectResult.ControllerName);
            Assert.Equal(1, redirectResult.RouteValues!["projectId"]);
            Assert.Equal(2, redirectResult.RouteValues!["id"]);
        }

        [Fact]
        public async Task Create_Post_ReturnsUnauthorized_WhenUserIdIsNull()
        {
            var model = new CommentViewModel { ProjectId = 1, TaskId = 2, Content = "Valid comment" };
            _mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns((string?)null);

            var result = await _controller.Create(model);

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task Create_Post_RedirectsToDetails_WhenSuccessful()
        {
            var model = new CommentViewModel { ProjectId = 1, TaskId = 2, Content = "Valid comment" };
            _mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user-123");

            var result = await _controller.Create(model);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(TaskConstants.DetailsAction, redirectResult.ActionName);
            Assert.Equal(TaskConstants.Controller, redirectResult.ControllerName);
            Assert.Equal(1, redirectResult.RouteValues!["projectId"]);
            Assert.Equal(2, redirectResult.RouteValues!["id"]);
            _mockCommentService.Verify(s => s.CreateCommentAsync(It.IsAny<CommentDto>(), "user-123"), Times.Once);
        }

        [Fact]
        public async Task Edit_Get_ReturnsUnauthorized_WhenUserIdIsNull()
        {
            _mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns((string?)null);

            var result = await _controller.Edit(1);

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task Edit_Get_ReturnsNotFound_WhenCommentDoesNotExist()
        {
            _mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user-123");
            _mockCommentService.Setup(s => s.GetCommentForEditAsync(1, "user-123")).ReturnsAsync((CommentDto?)null);

            var result = await _controller.Edit(1);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Get_ReturnsViewResult_WithCommentViewModel()
        {
            _mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user-123");
            var dto = new CommentDto { Content = "Edit me", TaskId = 2, ProjectId = 1, AuthorName = "Author" };
            _mockCommentService.Setup(s => s.GetCommentForEditAsync(1, "user-123")).ReturnsAsync(dto);

            var result = await _controller.Edit(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CommentViewModel>(viewResult.Model);
            Assert.Equal("Edit me", model.Content);
            Assert.Equal(2, model.TaskId);
            Assert.Equal(1, model.ProjectId);
        }

        [Fact]
        public async Task Edit_Post_ReturnsViewWithModel_WhenModelStateIsInvalid()
        {
            _controller.ModelState.AddModelError("Content", "Required");
            var model = new CommentViewModel { ProjectId = 1, TaskId = 2, Content = "" };

            var result = await _controller.Edit(1, model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
        }

        [Fact]
        public async Task Edit_Post_ReturnsUnauthorized_WhenUserIdIsNull()
        {
            var model = new CommentViewModel { ProjectId = 1, TaskId = 2, Content = "Updated context" };
            _mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns((string?)null);

            var result = await _controller.Edit(1, model);

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task Edit_Post_ReturnsNotFound_WhenUpdateFails()
        {
            var model = new CommentViewModel { ProjectId = 1, TaskId = 2, Content = "Updated context" };
            _mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user-123");
            _mockCommentService.Setup(s => s.UpdateCommentAsync(1, It.IsAny<CommentDto>(), "user-123")).ReturnsAsync(false);

            var result = await _controller.Edit(1, model);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_RedirectsToDetails_WhenSuccessful()
        {
            var model = new CommentViewModel { ProjectId = 1, TaskId = 2, Content = "Updated context" };
            _mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user-123");
            _mockCommentService.Setup(s => s.UpdateCommentAsync(1, It.IsAny<CommentDto>(), "user-123")).ReturnsAsync(true);

            var result = await _controller.Edit(1, model);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(TaskConstants.DetailsAction, redirectResult.ActionName);
            Assert.Equal(TaskConstants.Controller, redirectResult.ControllerName);
            Assert.Equal(1, redirectResult.RouteValues!["projectId"]);
            Assert.Equal(2, redirectResult.RouteValues!["id"]);
        }

        [Fact]
        public async Task Delete_Post_ReturnsUnauthorized_WhenUserIdIsNull()
        {
            _mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns((string?)null);

            var result = await _controller.Delete(1, 1);

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task Delete_Post_ReturnsNotFound_WhenDeleteFails()
        {
            _mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user-123");
            _mockCommentService.Setup(s => s.DeleteCommentAsync(1, "user-123")).ReturnsAsync(((bool Success, int TaskId)?)null);

            var result = await _controller.Delete(1, 1);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_Post_RedirectsToDetails_WhenSuccessful()
        {
            _mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user-123");
            _mockCommentService.Setup(s => s.DeleteCommentAsync(1, "user-123")).ReturnsAsync((Success: true, TaskId: 2));

            var result = await _controller.Delete(1, 1);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(TaskConstants.DetailsAction, redirectResult.ActionName);
            Assert.Equal(TaskConstants.Controller, redirectResult.ControllerName);
            Assert.Equal(1, redirectResult.RouteValues!["projectId"]);
            Assert.Equal(2, redirectResult.RouteValues!["id"]);
        }
    }
}