namespace ProjectManagementSystem.Tests.Services
{
    using BL.Constants;
    using BL.Models;
    using BL.Services;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore.Query;
    using Microsoft.Extensions.Logging;
    using Moq;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    public class AdminServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<ILogger<AdminService>> _mockLogger;
        private readonly AdminService _service;

        public AdminServiceTests()
        {
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            _mockLogger = new Mock<ILogger<AdminService>>();

            _service = new AdminService(_mockUserManager.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetAllUsersWithRolesAsync_ReturnsUsersWithAssignedRoles()
        {
            var users = new List<ApplicationUser>
            {
                new() { Id = "u1", Email = "admin@test.com", FirstName = "Admin", LastName = "User" },
                new() { Id = "u2", Email = null, FirstName = "NoEmail", LastName = "User" }
            };

            var mockQueryable = CreateMockQueryable(users);
            _mockUserManager.Setup(m => m.Users).Returns(mockQueryable.Object);

            _mockUserManager.Setup(m => m.GetRolesAsync(It.Is<ApplicationUser>(u => u.Id == "u1")))
                .ReturnsAsync([Roles.Admin]);
            _mockUserManager.Setup(m => m.GetRolesAsync(It.Is<ApplicationUser>(u => u.Id == "u2")))
                .ReturnsAsync([]);

            var result = (await _service.GetAllUsersWithRolesAsync()).ToList();

            Assert.Equal(2, result.Count);

            var adminDto = result.First(u => u.UserId == "u1");
            Assert.Equal("admin@test.com", adminDto.Email);
            Assert.Equal("Admin User", adminDto.FullName);
            Assert.Equal(Roles.Admin, adminDto.CurrentRole);
            Assert.True(adminDto.IsAdmin);

            var fallbackDto = result.First(u => u.UserId == "u2");
            Assert.Equal(MessageConstants.NoEmailProvided, fallbackDto.Email);
            Assert.Equal(Roles.Member, fallbackDto.CurrentRole);
            Assert.False(fallbackDto.IsAdmin);
        }

        [Fact]
        public async Task ChangeUserRoleAsync_ReturnsUserNotFound_WhenUserDoesNotExist()
        {
            _mockUserManager.Setup(m => m.FindByIdAsync("invalid-id")).ReturnsAsync((ApplicationUser?)null);

            var (success, message) = await _service.ChangeUserRoleAsync("invalid-id", Roles.Admin);

            Assert.False(success);
            Assert.Equal(MessageConstants.UserNotFound, message);
        }

        [Fact]
        public async Task ChangeUserRoleAsync_ReturnsCannotChangeAdminRole_WhenUserIsAlreadyAdmin()
        {
            var user = new ApplicationUser { Id = "u1", Email = "admin@test.com" };
            _mockUserManager.Setup(m => m.FindByIdAsync("u1")).ReturnsAsync(user);
            _mockUserManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync([Roles.Admin]);

            var (success, message) = await _service.ChangeUserRoleAsync("u1", Roles.Member);

            Assert.False(success);
            Assert.Equal(MessageConstants.CannotChangeAdminRole, message);
        }

        [Fact]
        public async Task ChangeUserRoleAsync_ChangesRoleSuccessfully_WhenUserIsNotAdmin()
        {
            var user = new ApplicationUser { Id = "u2", FirstName = "Regular", LastName = "User" };
            _mockUserManager.Setup(m => m.FindByIdAsync("u2")).ReturnsAsync(user);
            _mockUserManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync([Roles.Member]);
            _mockUserManager.Setup(m => m.RemoveFromRoleAsync(user, Roles.Member)).ReturnsAsync(IdentityResult.Success);
            _mockUserManager.Setup(m => m.AddToRoleAsync(user, Roles.Admin)).ReturnsAsync(IdentityResult.Success);

            var (success, message) = await _service.ChangeUserRoleAsync("u2", Roles.Admin);

            Assert.True(success);
            Assert.Equal(string.Format(MessageConstants.RoleChangedSuccessfully, Roles.Admin, "Regular User"), message);
            _mockUserManager.Verify(m => m.RemoveFromRoleAsync(user, Roles.Member), Times.Once);
            _mockUserManager.Verify(m => m.AddToRoleAsync(user, Roles.Admin), Times.Once);
        }

        [Fact]
        public async Task ChangeUserRoleAsync_ReturnsRoleUpdateFailed_WhenAddToRoleFails()
        {
            var user = new ApplicationUser { Id = "u3", FirstName = "Test", LastName = "User" };
            _mockUserManager.Setup(m => m.FindByIdAsync("u3")).ReturnsAsync(user);
            _mockUserManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync([]);
            _mockUserManager.Setup(m => m.AddToRoleAsync(user, Roles.Admin))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Error" }));

            var (success, message) = await _service.ChangeUserRoleAsync("u3", Roles.Admin);

            Assert.False(success);
            Assert.Equal(MessageConstants.RoleUpdateFailed, message);
        }

        private static Mock<IQueryable<T>> CreateMockQueryable<T>(IEnumerable<T> source)
        {
            var queryable = source.AsQueryable();
            var mock = new Mock<IQueryable<T>>();
            mock.As<IAsyncEnumerable<T>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(new TestAsyncEnumerator<T>(queryable.GetEnumerator()));
            mock.Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
            mock.Setup(m => m.Expression).Returns(queryable.Expression);
            mock.Setup(m => m.ElementType).Returns(queryable.ElementType);
            mock.Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            return mock;
        }

        private class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
        {
            private readonly IQueryProvider _inner;

            internal TestAsyncQueryProvider(IQueryProvider inner) => _inner = inner;

            public IQueryable CreateQuery(Expression expression) => new TestAsyncEnumerable<TEntity>(expression);

            public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => new TestAsyncEnumerable<TElement>(expression);

            public object? Execute(Expression expression) => _inner.Execute(expression);

            public TResult Execute<TResult>(Expression expression) => _inner.Execute<TResult>(expression);

            public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
            {
                var expectedResultType = typeof(TResult).GetGenericArguments()[0];
                var executionResult = typeof(IQueryProvider).GetMethods()
                    .First(m => m.Name == nameof(IQueryProvider.Execute) && m.IsGenericMethod)
                    .MakeGenericMethod(expectedResultType)
                    .Invoke(_inner, [expression]);
                return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!.MakeGenericMethod(expectedResultType).Invoke(null, [executionResult])!;
            }
        }

        private class TestAsyncEnumerable<T>(Expression expression) : EnumerableQuery<T>(expression), IAsyncEnumerable<T>, IQueryable<T>
        {
            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) => new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());

            IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
        }

        private class TestAsyncEnumerator<T>(IEnumerator<T> inner) : IAsyncEnumerator<T>
        {
            private readonly IEnumerator<T> _inner = inner;

            public T Current => _inner.Current;

            public ValueTask DisposeAsync()
            { _inner.Dispose(); return ValueTask.CompletedTask; }

            public ValueTask<bool> MoveNextAsync() => ValueTask.FromResult(_inner.MoveNext());
        }
    }
}