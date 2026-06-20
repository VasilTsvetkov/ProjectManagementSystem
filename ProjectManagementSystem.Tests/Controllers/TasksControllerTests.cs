namespace ProjectManagementSystem.Tests.Controllers
{
    using BL.DTOs.Tasks;
    using BL.Enums.Task;
    using BL.Interfaces;
    using BL.Models;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Web.Controllers;
    using Web.ViewModels.Tasks;
    using Xunit;

    public class TasksControllerTests
    {
        private readonly Mock<ITaskService> _mockTaskService;
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly TasksController _controller;

        public TasksControllerTests()
        {
            _mockTaskService = new Mock<ITaskService>();

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            _controller = new TasksController(_mockTaskService.Object, _mockUserManager.Object);
        }

        [Fact]
        public async Task Index_ReturnsNotFound_WhenProjectDoesNotExist()
        {
            _mockTaskService.Setup(s => s.GetTasksByProjectAsync(1))
                .ReturnsAsync(((IEnumerable<TaskListDto> Tasks, string ProjectName)?)null);

            var result = await _controller.Index(1);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WithListOfTaskListViewModels()
        {
            var taskList = new List<TaskListDto>
            {
                new() { Id = 10, Tag = "T-1", Title = "Task 1", Type = Type.Task, Priority = Priority.Medium, Status = Status.ToDo, AssigneeName = "John" }
            };
            var resultTuple = (Tasks: (IEnumerable<TaskListDto>)taskList, ProjectName: "Project Alpha");
            _mockTaskService.Setup(s => s.GetTasksByProjectAsync(1)).ReturnsAsync(resultTuple);

            var result = await _controller.Index(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(1, _controller.ViewBag.ProjectId);
            Assert.Equal("Project Alpha", _controller.ViewBag.ProjectName);
            var model = Assert.IsAssignableFrom<List<TaskListViewModel>>(viewResult.ViewData.Model);
            Assert.Single(model);
        }

        [Fact]
        public async Task Create_Get_ReturnsViewResult_WithTaskViewModel()
        {
            var taskDto = new TaskDto
            {
                Title = string.Empty,
                Type = Type.Task,
                Priority = Priority.Low,
                Status = Status.ToDo,
                Users = [new() { Id = "u1", FullName = "User One" }]
            };
            _mockTaskService.Setup(s => s.GetTaskForCreateAsync()).ReturnsAsync(taskDto);

            var result = await _controller.Create(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(1, _controller.ViewBag.ProjectId);
            var model = Assert.IsType<TaskViewModel>(viewResult.Model);
            Assert.Single(model.Users);
        }

        [Fact]
        public async Task Create_Post_ReturnsViewWithModel_WhenModelStateIsInvalid()
        {
            _controller.ModelState.AddModelError("Title", "Required");
            var taskDto = new TaskDto
            {
                Title = string.Empty,
                Type = Type.Task,
                Priority = Priority.Low,
                Status = Status.ToDo,
                Users = [new() { Id = "u1", FullName = "User One" }]
            };
            _mockTaskService.Setup(s => s.GetTaskForCreateAsync()).ReturnsAsync(taskDto);
            var model = new TaskViewModel { Title = "", Type = Type.Task, Priority = Priority.Medium, Description = "Invalid" };

            var result = await _controller.Create(1, model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            Assert.Single(model.Users);
        }

        [Fact]
        public async Task Create_Post_ReturnsUnauthorized_WhenUserIdIsNull()
        {
            var model = new TaskViewModel { Title = "Valid", Type = Type.Task, Priority = Priority.Medium };
            _mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns((string?)null);

            var result = await _controller.Create(1, model);

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task Create_Post_RedirectsToIndex_WhenSuccessful()
        {
            var model = new TaskViewModel { Title = "Valid", Type = Type.Task, Priority = Priority.Medium, Description = "Desc" };
            _mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user-123");

            var result = await _controller.Create(1, model);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(TasksController.Index), redirectResult.ActionName);
            Assert.Equal(1, redirectResult.RouteValues?["projectId"]);
            _mockTaskService.Verify(s => s.CreateTaskAsync(1, It.IsAny<TaskDto>(), "user-123"), Times.Once);
        }

        [Fact]
        public async Task Edit_Get_ReturnsNotFound_WhenTaskDoesNotExist()
        {
            _mockTaskService.Setup(s => s.GetTaskForEditAsync(1)).ReturnsAsync((TaskDto?)null);

            var result = await _controller.Edit(1, 1);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Get_ReturnsViewResult_WithEditTaskViewModel()
        {
            var taskDto = new TaskDto { Id = 2, Title = "Edit", Type = Type.Task, Priority = Priority.Medium, AssigneeId = "u1", Users = [new() { Id = "u1", FullName = "User One" }] };
            _mockTaskService.Setup(s => s.GetTaskForEditAsync(2)).ReturnsAsync(taskDto);

            var result = await _controller.Edit(1, 2);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(1, _controller.ViewBag.ProjectId);
            var model = Assert.IsType<EditTaskViewModel>(viewResult.Model);
            Assert.Equal("Edit", model.Title);
        }

        [Fact]
        public async Task Edit_Post_ReturnsViewWithModel_WhenModelStateIsInvalid_AndTaskExists()
        {
            _controller.ModelState.AddModelError("Title", "Required");
            var taskDto = new TaskDto { Id = 2, Title = "Edit", Type = Type.Task, Priority = Priority.Medium, Users = [new() { Id = "u1", FullName = "User One" }] };
            _mockTaskService.Setup(s => s.GetTaskForEditAsync(2)).ReturnsAsync(taskDto);
            var model = new EditTaskViewModel { Title = "", Type = Type.Task, Priority = Priority.Medium, Status = Status.ToDo, Description = "Invalid" };

            var result = await _controller.Edit(1, 2, model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            Assert.Single(model.Users);
        }

        [Fact]
        public async Task Edit_Post_ReturnsViewWithModel_WhenModelStateIsInvalid_AndTaskDoesNotExist()
        {
            _controller.ModelState.AddModelError("Title", "Required");
            _mockTaskService.Setup(s => s.GetTaskForEditAsync(2)).ReturnsAsync((TaskDto?)null);
            var model = new EditTaskViewModel { Title = "", Type = Type.Task, Priority = Priority.Medium, Status = Status.ToDo, Description = "Invalid" };

            var result = await _controller.Edit(1, 2, model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            Assert.Empty(model.Users);
        }

        [Fact]
        public async Task Edit_Post_ReturnsUnauthorized_WhenUserIdIsNull()
        {
            var model = new EditTaskViewModel { Title = "Valid", Type = Type.Task, Priority = Priority.Medium, Status = Status.ToDo };
            var context = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext { HttpContext = context };

            var result = await _controller.Edit(1, 2, model);

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task Edit_Post_ReturnsNotFound_WhenUpdateFails()
        {
            var model = new EditTaskViewModel { Title = "Valid", Type = Type.Task, Priority = Priority.Medium, Status = Status.ToDo };
            var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "user-123")]));
            var context = new DefaultHttpContext { User = user };
            _controller.ControllerContext = new ControllerContext { HttpContext = context };
            _mockTaskService.Setup(s => s.UpdateTaskAsync(2, It.IsAny<TaskDto>(), "user-123")).ReturnsAsync(false);

            var result = await _controller.Edit(1, 2, model);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_RedirectsToIndex_WhenSuccessful()
        {
            var model = new EditTaskViewModel { Title = "Valid", Type = Type.Task, Priority = Priority.Medium, Status = Status.ToDo };
            var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "user-123")]));
            var context = new DefaultHttpContext { User = user };
            _controller.ControllerContext = new ControllerContext { HttpContext = context };
            _mockTaskService.Setup(s => s.UpdateTaskAsync(2, It.IsAny<TaskDto>(), "user-123")).ReturnsAsync(true);

            var result = await _controller.Edit(1, 2, model);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(TasksController.Index), redirectResult.ActionName);
            Assert.Equal(1, redirectResult.RouteValues?["projectId"]);
        }

        [Fact]
        public async Task Delete_Post_ReturnsUnauthorized_WhenUserIdIsNull()
        {
            var context = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext { HttpContext = context };

            var result = await _controller.Delete(1, 2);

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task Delete_Post_ReturnsNotFound_WhenDeleteFails()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "user-123")]));
            var context = new DefaultHttpContext { User = user };
            _controller.ControllerContext = new ControllerContext { HttpContext = context };
            _mockTaskService.Setup(s => s.DeleteTaskAsync(2, "user-123")).ReturnsAsync(false);

            var result = await _controller.Delete(1, 2);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_Post_RedirectsToIndex_WhenSuccessful()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "user-123")]));
            var context = new DefaultHttpContext { User = user };
            _controller.ControllerContext = new ControllerContext { HttpContext = context };
            _mockTaskService.Setup(s => s.DeleteTaskAsync(2, "user-123")).ReturnsAsync(true);

            var result = await _controller.Delete(1, 2);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(TasksController.Index), redirectResult.ActionName);
            Assert.Equal(1, redirectResult.RouteValues?["projectId"]);
        }

        [Fact]
        public async Task Details_ReturnsUnauthorized_WhenUserIdIsNull()
        {
            _mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns((string?)null);

            var result = await _controller.Details(1, 2);

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task Details_ReturnsNotFound_WhenDetailsDoNotExist()
        {
            _mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user-123");
            _mockTaskService.Setup(s => s.GetTaskDetailsAsync(1, 2, "user-123")).ReturnsAsync((TaskDetailsDto?)null);

            var result = await _controller.Details(1, 2);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_ReturnsViewResult_WithTaskDetailsViewModel()
        {
            _mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user-123");
            var detailsDto = new TaskDetailsDto
            {
                Id = 2,
                ProjectId = 1,
                Tag = "T-2",
                Title = "Title",
                Description = "Desc",
                Status = Status.ToDo,
                Priority = Priority.Medium,
                Type = Type.Task,
                AssigneeName = "John",
                ReporterName = "Admin",
                TotalHours = 2.5,
                Comments = [new() { Id = 5, Content = "C", AuthorName = "A", CanEdit = true }],
                TimeLogs = [new() { Id = 6, Hours = 2.5, UserName = "U", CanEdit = false }]
            };
            _mockTaskService.Setup(s => s.GetTaskDetailsAsync(1, 2, "user-123")).ReturnsAsync(detailsDto);

            var result = await _controller.Details(1, 2);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TaskDetailsViewModel>(viewResult.Model);
            Assert.Equal(2, model.Id);
            Assert.Single(model.Comments);
            Assert.Single(model.TimeLogs);
        }

        [Fact]
        public async Task UpdateStatus_ReturnsUnauthorized_WhenUserIdIsNull()
        {
            var context = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext { HttpContext = context };

            var result = await _controller.UpdateStatus(1, 2, Status.InProgress);

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task UpdateStatus_ReturnsNotFound_WhenStatusUpdateFails()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "user-123")]));
            var context = new DefaultHttpContext { User = user };
            _controller.ControllerContext = new ControllerContext { HttpContext = context };
            _mockTaskService.Setup(s => s.UpdateTaskStatusAsync(2, Status.InProgress, "user-123")).ReturnsAsync(false);

            var result = await _controller.UpdateStatus(1, 2, Status.InProgress);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdateStatus_ReturnsOk_WhenSuccessful()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "user-123")]));
            var context = new DefaultHttpContext { User = user };
            _controller.ControllerContext = new ControllerContext { HttpContext = context };
            _mockTaskService.Setup(s => s.UpdateTaskStatusAsync(2, Status.InProgress, "user-123")).ReturnsAsync(true);

            var result = await _controller.UpdateStatus(1, 2, Status.InProgress);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task Board_ReturnsNotFound_WhenBoardDataIsNull()
        {
            _mockTaskService.Setup(s => s.GetTasksForBoardAsync(1))
                .ReturnsAsync(((IEnumerable<TaskListDto> Tasks, string ProjectName, string ProjectTag)?)null);

            var result = await _controller.Board(1);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Board_ReturnsViewResult_WithListOfTaskListViewModels()
        {
            var taskList = new List<TaskListDto> { new() { Id = 30, Tag = "T-30", Title = "Board Task", Type = Type.Task, Priority = Priority.Medium, Status = Status.ToDo, AssigneeName = "John" } };
            var resultTuple = (Tasks: (IEnumerable<TaskListDto>)taskList, ProjectName: "Board Project", ProjectTag: "BPT");
            _mockTaskService.Setup(s => s.GetTasksForBoardAsync(1)).ReturnsAsync(resultTuple);

            var result = await _controller.Board(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(1, _controller.ViewBag.ProjectId);
            Assert.Equal("Board Project", _controller.ViewBag.ProjectName);
            Assert.Equal("BPT", _controller.ViewBag.ProjectTag);
            var model = Assert.IsAssignableFrom<List<TaskListViewModel>>(viewResult.ViewData.Model);
            Assert.Single(model);
        }
    }
}