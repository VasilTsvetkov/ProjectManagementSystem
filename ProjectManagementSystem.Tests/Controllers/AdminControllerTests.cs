namespace ProjectManagementSystem.Tests.Controllers
{
    using BL.DTOs;
    using BL.Interfaces;
    using Microsoft.AspNetCore.Antiforgery;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Web.Controllers;
    using Web.ViewModels.Admin;
    using Xunit;

    public class AdminControllerTests
    {
        private readonly Mock<IAdminService> _mockAdminService;
        private readonly Mock<IAntiforgery> _mockAntiforgery;
        private readonly AdminController _controller;

        public AdminControllerTests()
        {
            _mockAdminService = new Mock<IAdminService>();
            _mockAntiforgery = new Mock<IAntiforgery>();
            var mockTempDataFactory = new Mock<ITempDataDictionaryFactory>();

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(IAntiforgery)))
                .Returns(_mockAntiforgery.Object);
            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(ITempDataDictionaryFactory)))
                .Returns(mockTempDataFactory.Object);

            var context = new DefaultHttpContext
            {
                RequestServices = mockServiceProvider.Object
            };

            _controller = new AdminController(_mockAdminService.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = context }
            };
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WithListOfUserRoleViewModels()
        {
            List<UserRoleDto> dtos = [
                new() { UserId = "u1", Email = "a@t.com", FullName = "Admin User", CurrentRole = "Admin", IsAdmin = true },
                new() { UserId = "u2", Email = "u@t.com", FullName = "Regular User", CurrentRole = "User", IsAdmin = false }
            ];
            _mockAdminService.Setup(s => s.GetAllUsersWithRolesAsync()).ReturnsAsync(dtos);

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<UserRoleViewModel>>(viewResult.ViewData.Model);
            Assert.Equal(2, model.Count);
            Assert.Equal("u1", model[0].UserId);
            Assert.Equal("u2", model[1].UserId);
        }

        [Fact]
        public async Task ChangeRole_ReturnsBadRequest_WhenAntiforgeryValidationFails()
        {
            _mockAntiforgery
                .Setup(a => a.ValidateRequestAsync(It.IsAny<HttpContext>()))
                .ThrowsAsync(new AntiforgeryValidationException("Invalid token"));

            var result = await _controller.ChangeRole("u1", "Admin");

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid security token.", badRequestResult.Value);
        }

        [Fact]
        public async Task ChangeRole_ReturnsBadRequest_WhenRoleUpdateFails()
        {
            _mockAntiforgery
                .Setup(a => a.ValidateRequestAsync(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            _mockAdminService
                .Setup(s => s.ChangeUserRoleAsync("u1", "Admin"))
                .ReturnsAsync((false, "Error message"));

            var result = await _controller.ChangeRole("u1", "Admin");

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Could not update user role.", badRequestResult.Value);
        }

        [Fact]
        public async Task ChangeRole_ReturnsOk_WhenSuccessful()
        {
            _mockAntiforgery
                .Setup(a => a.ValidateRequestAsync(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            _mockAdminService
                .Setup(s => s.ChangeUserRoleAsync("u1", "Admin"))
                .ReturnsAsync((true, string.Empty));

            var result = await _controller.ChangeRole("u1", "Admin");

            Assert.IsType<OkResult>(result);
            _mockAdminService.Verify(s => s.ChangeUserRoleAsync("u1", "Admin"), Times.Once);
        }
    }
}