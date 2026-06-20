namespace ProjectManagementSystem.Tests.Controllers
{
    using BL.Constants;
    using BL.DTOs.TimeLogs;
    using BL.Interfaces;
    using BL.Models;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Web.Controllers;
    using Web.ViewModels.TimeLogs;
    using Xunit;

    public class TimeLogsControllerTests
    {
        private readonly Mock<ITimeLogService> _mockTimeLogService;
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<ITempDataDictionary> _mockTempData;
        private readonly TimeLogsController _controller;

        public TimeLogsControllerTests()
        {
            _mockTimeLogService = new Mock<ITimeLogService>();
            _mockTempData = new Mock<ITempDataDictionary>();

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            var mockTempDataFactory = new Mock<ITempDataDictionaryFactory>();
            var mockUrlHelperFactory = new Mock<IUrlHelperFactory>();
            var mockUrlHelper = new Mock<IUrlHelper>();

            mockUrlHelperFactory
                .Setup(f => f.GetUrlHelper(It.IsAny<ActionContext>()))
                .Returns(mockUrlHelper.Object);

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(ITempDataDictionaryFactory)))
                .Returns(mockTempDataFactory.Object);
            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(IUrlHelperFactory)))
                .Returns(mockUrlHelperFactory.Object);

            var context = new DefaultHttpContext
            {
                RequestServices = mockServiceProvider.Object
            };

            _controller = new TimeLogsController(_mockTimeLogService.Object, _mockUserManager.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = context },
                TempData = _mockTempData.Object,
                Url = mockUrlHelper.Object
            };
        }

        [Fact]
        public async Task Matrix_ReturnsNotFound_WhenDtoIsNull()
        {
            _mockTimeLogService
                .Setup(s => s.GetMonthlyMatrixAsync(1, It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync((MonthlyMatrixDto?)null);

            var result = await _controller.Matrix(1, null, null);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Matrix_ReturnsViewResult_WithMonthlyMatrixViewModel_WhenSuccessful()
        {
            var dailyHoursMock = new Dictionary<int, double>
            {
                { 1, 8.0 },
                { 2, 4.5 }
            };

            var dto = new MonthlyMatrixDto
            {
                ProjectId = 1,
                ProjectName = "Test Project",
                SelectedMonth = new DateTime(2026, 6, 1),
                DaysInMonth = [new DateTime(2026, 6, 1), new DateTime(2026, 6, 2)],
                Rows = [
                    new() { UserId = "u1", FullName = "User One", DailyHours = dailyHoursMock }
                ]
            };

            _mockTimeLogService
                .Setup(s => s.GetMonthlyMatrixAsync(1, 6, 2026))
                .ReturnsAsync(dto);

            var result = await _controller.Matrix(1, 6, 2026);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<MonthlyMatrixViewModel>(viewResult.Model);
            Assert.Equal(1, model.ProjectId);
            Assert.Equal("Test Project", model.ProjectName);
            Assert.Equal(dto.SelectedMonth, model.SelectedMonth);
            Assert.Equal(dto.DaysInMonth, model.DaysInMonth);
            Assert.Single(model.Rows);
            Assert.Equal("u1", model.Rows[0].UserId);
            Assert.Equal(dailyHoursMock, model.Rows[0].DailyHours);
        }

        [Fact]
        public async Task Create_Post_RedirectsToDetails_WhenModelStateIsInvalid()
        {
            _controller.ModelState.AddModelError("Hours", "Invalid");
            var model = new TimeLogViewModel { ProjectId = 1, TaskId = 2 };

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
            var model = new TimeLogViewModel { ProjectId = 1, TaskId = 2 };
            _mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns((string?)null);

            var result = await _controller.Create(model);

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task Create_Post_SetsTempDataAndRedirects_WhenDateIsFuture()
        {
            var model = new TimeLogViewModel
            {
                ProjectId = 1,
                TaskId = 2,
                Date = DateTime.UtcNow.AddDays(1)
            };
            _mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user-123");

            var result = await _controller.Create(model);

            _mockTempData.VerifySet(t => t[NotificationKeys.Error] = "You cannot log time for future dates.", Times.Once);
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(1, redirectResult.RouteValues!["projectId"]);
        }

        [Fact]
        public async Task Create_Post_SetsTempDataAndRedirects_WhenTimeIsZero()
        {
            var model = new TimeLogViewModel
            {
                ProjectId = 1,
                TaskId = 2,
                Date = DateTime.UtcNow.AddDays(-1),
                Days = 0,
                Hours = 0,
                Minutes = 0
            };
            _mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user-123");

            var result = await _controller.Create(model);

            _mockTempData.VerifySet(t => t[NotificationKeys.Error] = "Please enter the amount of time worked.", Times.Once);
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(2, redirectResult.RouteValues!["id"]);
        }

        [Fact]
        public async Task Create_Post_SetsLimitErrorTempData_WhenServiceReturnsFalse()
        {
            var model = new TimeLogViewModel
            {
                ProjectId = 1,
                TaskId = 2,
                Date = DateTime.UtcNow.AddDays(-1),
                Hours = 5
            };
            _mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user-123");
            _mockTimeLogService.Setup(s => s.CreateTimeLogAsync(It.IsAny<TimeLogDto>(), "user-123")).ReturnsAsync(false);

            var result = await _controller.Create(model);

            _mockTempData.VerifySet(t => t[NotificationKeys.Error] = It.Is<string>(s => s.Contains("Daily limit reached")), Times.Once);
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(1, redirectResult.RouteValues!["projectId"]);
        }

        [Fact]
        public async Task Create_Post_RedirectsSuccessfully_WithoutLimitError_WhenServiceReturnsTrue()
        {
            var model = new TimeLogViewModel
            {
                ProjectId = 1,
                TaskId = 2,
                Date = DateTime.UtcNow.AddDays(-1),
                Hours = 5
            };
            _mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user-123");
            _mockTimeLogService.Setup(s => s.CreateTimeLogAsync(It.IsAny<TimeLogDto>(), "user-123")).ReturnsAsync(true);

            var result = await _controller.Create(model);

            _mockTempData.VerifySet(t => t[NotificationKeys.Error] = It.IsAny<string>(), Times.Never);
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
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
        public async Task Delete_Post_ReturnsNotFound_WhenServiceReturnsNull()
        {
            _mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user-123");
            _mockTimeLogService.Setup(s => s.DeleteTimeLogAsync(1, "user-123")).ReturnsAsync(((bool Success, int TaskId)?)null);

            var result = await _controller.Delete(1, 1);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_Post_RedirectsToDetails_WhenSuccessful()
        {
            _mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user-123");
            _mockTimeLogService.Setup(s => s.DeleteTimeLogAsync(1, "user-123")).ReturnsAsync((Success: true, TaskId: 5));

            var result = await _controller.Delete(1, 1);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(TaskConstants.DetailsAction, redirectResult.ActionName);
            Assert.Equal(TaskConstants.Controller, redirectResult.ControllerName);
            Assert.Equal(1, redirectResult.RouteValues!["projectId"]);
            Assert.Equal(5, redirectResult.RouteValues!["id"]);
        }
    }
}