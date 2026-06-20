namespace ProjectManagementSystem.Tests.Services
{
    using BL.DTOs.Dashboard;
    using BL.Interfaces;
    using BL.Services;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Xunit;

    public class DashboardServiceTests
    {
        private readonly Mock<ITimeLogRepository> _mockTimeLogRepository;
        private readonly DashboardService _service;

        public DashboardServiceTests()
        {
            _mockTimeLogRepository = new Mock<ITimeLogRepository>();
            _service = new DashboardService(_mockTimeLogRepository.Object);
        }

        [Fact]
        public async Task GetDashboardDataAsync_UsesProvidedYearAndMonth_WhenParametersAreNotNull()
        {
            const int year = 2025;
            const int month = 5;
            const string userId = "user-123";

            var statsDto = new MonthlyStatsDto
            {
                TotalHours = 45.5,
                TotalProjects = 2,
                TotalTasks = 10,
                TotalLogs = 15
            };

            var projects = new List<ProjectTimeDto>
            {
                new()
                {
                    ProjectId = 1,
                    ProjectName = "Proj A",
                    ProjectTag = "PRJ-1",
                    TotalHours = 12.5,
                    TaskCount = 4,
                    LogCount = 6
                }
            };

            var users = new List<UserTimeDto>
            {
                new()
                {
                    UserId = "user-123",
                    UserName = "Vasil",
                    TotalHours = 12.5,
                    ProjectCount = 1,
                    TaskCount = 4
                }
            };

            _mockTimeLogRepository.Setup(r => r.GetMonthlyStatsAsync(year, month, It.IsAny<string?>())).ReturnsAsync(statsDto);
            _mockTimeLogRepository.Setup(r => r.GetProjectBreakdownAsync(year, month, It.IsAny<string?>())).ReturnsAsync(projects);
            _mockTimeLogRepository.Setup(r => r.GetUserBreakdownAsync(year, month, It.IsAny<int?>())).ReturnsAsync(users);

            var result = await _service.GetDashboardDataAsync(year, month, userId);

            Assert.NotNull(result);
            Assert.Equal(year, result.Year);
            Assert.Equal(month, result.Month);
            Assert.Equal(userId, result.SelectedUserId);
            Assert.Same(statsDto, result.Stats);
            Assert.Single(result.ProjectBreakdown);
            Assert.Single(result.UserBreakdown);
            Assert.True(result.CanViewAllUsers);
        }

        [Fact]
        public async Task GetDashboardDataAsync_FallsBackToCurrentDate_WhenParametersAreNull()
        {
            var currentYear = DateTime.UtcNow.Year;
            var currentMonth = DateTime.UtcNow.Month;
            const string userId = "user-456";

            var statsDto = new MonthlyStatsDto { TotalHours = 0, TotalProjects = 0, TotalTasks = 0, TotalLogs = 0 };

            _mockTimeLogRepository.Setup(r => r.GetMonthlyStatsAsync(currentYear, currentMonth, It.IsAny<string?>())).ReturnsAsync(statsDto);
            _mockTimeLogRepository.Setup(r => r.GetProjectBreakdownAsync(currentYear, currentMonth, It.IsAny<string?>())).ReturnsAsync([]);
            _mockTimeLogRepository.Setup(r => r.GetUserBreakdownAsync(currentYear, currentMonth, It.IsAny<int?>())).ReturnsAsync([]);

            var result = await _service.GetDashboardDataAsync(null, null, userId);

            Assert.NotNull(result);
            Assert.Equal(currentYear, result.Year);
            Assert.Equal(currentMonth, result.Month);
        }
    }
}