namespace ProjectManagementSystem.Tests.Controllers
{
    using BL.DTOs;
    using BL.Interfaces;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
    using Moq;
    using System;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Web.Controllers;
    using Web.ViewModels.Home;
    using Xunit;

    public class HomeControllerTests
    {
        private readonly Mock<IHomeService> _mockHomeService;
        private readonly HomeController _controller;

        public HomeControllerTests()
        {
            _mockHomeService = new Mock<IHomeService>();
            var mockTempDataFactory = new Mock<ITempDataDictionaryFactory>();

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(ITempDataDictionaryFactory)))
                .Returns(mockTempDataFactory.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity([
                new Claim(ClaimTypes.NameIdentifier, "user-123")
            ], "mock"));

            var context = new DefaultHttpContext
            {
                RequestServices = mockServiceProvider.Object,
                User = user
            };

            _controller = new HomeController(_mockHomeService.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = context }
            };
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WithIndexViewModel()
        {
            var dto = new HomeIndexDto
            {
                MyPendingTasksCount = 5,
                OverdueTasksCount = 2,
                UrgentTasks = [],
                RecentActivities = []
            };

            _mockHomeService
                .Setup(s => s.GetHomeIndexDataAsync("user-123"))
                .ReturnsAsync(dto);

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<IndexViewModel>(viewResult.Model);
            Assert.Equal(5, model.MyPendingTasksCount);
            Assert.Equal(2, model.OverdueTasksCount);
            Assert.Empty(model.UrgentTasks);
            Assert.Empty(model.RecentActivities);
        }

        [Fact]
        public void Error_ReturnsViewResult_WithErrorViewModel()
        {
            var result = _controller.Error(404);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ErrorViewModel>(viewResult.Model);
            Assert.Equal(404, model.StatusCode);
            Assert.False(string.IsNullOrEmpty(model.RequestId));
        }
    }
}