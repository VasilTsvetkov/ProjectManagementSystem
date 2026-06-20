namespace ProjectManagementSystem.Tests.Controllers
{
    using BL.DTOs.Dashboard;
    using BL.Interfaces;
    using BL.Models;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Web.Controllers;
    using Web.ViewModels.Dashboard;
    using Xunit;

    public class DashboardControllerTests
    {
        private readonly Mock<IDashboardService> _mockDashboardService;
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly DashboardController _controller;

        public DashboardControllerTests()
        {
            _mockDashboardService = new Mock<IDashboardService>();

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            _controller = new DashboardController(_mockDashboardService.Object, _mockUserManager.Object);
        }

        [Fact]
        public async Task Index_ReturnsUnauthorized_WhenUserIdIsNullOrEmpty()
        {
            _mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns((string?)null);

            var result = await _controller.Index(null, null);

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WithDashboardViewModel_WhenSuccessful()
        {
            _mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user-123");

            var dto = new DashboardDto
            {
                Year = 2026,
                Month = 6,
                SelectedUserId = "user-123",
                CanViewAllUsers = false,
                Stats = new MonthlyStatsDto
                {
                    TotalHours = 40.0,
                    TotalProjects = 2,
                    TotalTasks = 5,
                    TotalLogs = 10
                },
                ProjectBreakdown = [],
                UserBreakdown = []
            };

            _mockDashboardService
                .Setup(s => s.GetDashboardDataAsync(2026, 6, "user-123"))
                .ReturnsAsync(dto);

            var result = await _controller.Index(2026, 6);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<DashboardViewModel>(viewResult.Model);
            Assert.Equal(2026, model.Year);
            Assert.Equal(6, model.Month);
            Assert.Equal("user-123", model.SelectedUserId);
            Assert.False(model.CanViewAllUsers);
            Assert.Equal(12, model.AvailableMonths.Count);
            Assert.Equal(5, model.AvailableYears.Count);
        }
    }
}