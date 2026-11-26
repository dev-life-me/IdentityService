using Application.Dtos;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence.AppDbContext;
using IntergrationTest.Configs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace IntergrationTest.Apis
{
    public class UserControllerIntergrationTest : IClassFixture<PostgreSqlFixture>, IAsyncLifetime
    {
        private readonly PostgreSqlFixture _postgreSqlFixture;
        private HttpClient _httpClient;
        private readonly CustomeWebApplicationFactory _factory;

        public UserControllerIntergrationTest(PostgreSqlFixture postgreSqlFixture)
        {
            _postgreSqlFixture = postgreSqlFixture;
            _factory = new CustomeWebApplicationFactory(_postgreSqlFixture.PostgreSqlContainer.GetConnectionString());

        }
        public async ValueTask InitializeAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<IdentityServiceDbContext>();

            await context.Database.MigrateAsync(TestContext.Current.CancellationToken);
            _httpClient = _factory.CreateClient();
        }

        [Fact]
        public async Task CreateUser_ReturnsCreated_WhenUserIsValid()
        {
            // Arrange

            var ct = TestContext.Current.CancellationToken;

            var request = new CreateUserRequest(
                "John",
                "john@example.com",
                "password123");

            // Act
            using var response = await _httpClient.PostAsJsonAsync(
                "/api/users", request, ct);

            var dto = await response.Content.ReadFromJsonAsync<UserDto>(ct);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            using var context = _factory.Services.CreateScope().ServiceProvider.GetRequiredService<IdentityServiceDbContext>();
            var userInDb = await context.Users.FirstOrDefaultAsync(u => u.Id == dto!.Id, ct);
            Assert.NotNull(userInDb);
            Assert.Equal(request.Email, userInDb!.Email);
            Assert.Equal(request.FullName, userInDb.FullName);
            Assert.Equal(request.Role, userInDb.Role);
            Assert.NotEqual(Guid.Empty, userInDb.Id);
            Assert.NotNull(userInDb.PasswordHash);
            //Assert.NotEqual(request.Password, userInDb.PasswordHash); not implenented yet
            Assert.False(userInDb.IsDeleted);
            Assert.NotEqual(default, userInDb.CreatedAt);
            Assert.Equal(default, userInDb.DeletedAt);
        }

        [Fact]
        public async Task CreateUser_ReturnsConflict_WhenEmailAlreadyExists()
        {
            // Arrange
            var ct = TestContext.Current.CancellationToken;
            using var db = _factory.Services.CreateScope().ServiceProvider.GetRequiredService<IdentityServiceDbContext>();
            var existingUser = User.Create("test@example.com", "John Doe", "password123", Role.User);
            await db.Users.AddAsync(existingUser, ct);
            await db.SaveChangesAsync(ct);
            // Act
            var request = new CreateUserRequest(
                existingUser.Email,
                "Jane Doe",
                "password456");
            // Act
            using var response = await _httpClient.PostAsJsonAsync(
                "/api/users", request, ct);
            var body = await response.Content.ReadFromJsonAsync<ProblemDetails>(ct);
            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.NotNull(body);
            Assert.Equal("Conflict", body!.Title);
            Assert.Equal(409, body!.Status);
        }

        [Fact]
        public async Task CreateUser_ReturnsConflict_WithConcurrencyRequest()
        {
            // Arrange
            var ct = TestContext.Current.CancellationToken;
            CreateUserRequest request = new CreateUserRequest("test1@email", "Test", "TestPassword");
            var task1 = _httpClient.PostAsJsonAsync("/api/users", request, ct);
             var task2 = _httpClient.PostAsJsonAsync("/api/users", request, ct);
            // Act

            using var response1 = await task1;
            using var response2 = await task2;

            // Assert
            var statusCodes = new[]
            {
                  response1.StatusCode,
                  response2.StatusCode
            };

            Assert.Contains(HttpStatusCode.Created, statusCodes);
            Assert.Contains(HttpStatusCode.Conflict, statusCodes);

            using var db = _factory.Services.CreateScope().ServiceProvider.GetRequiredService<IdentityServiceDbContext>();
            var count = await db.Users.CountAsync(x => x.Email == request.Email, ct);
            Assert.Equal(1, count);

            

        }

        [Fact]
        public async Task CreateUser_ReturnsBadRequest_WhenEmailIsEmpty()
        {
            // Arrange
            var ct = TestContext.Current.CancellationToken;
            var request = new CreateUserRequest(
                "",
                "John Doe",
                "password123");
            // Act
            using var response = await _httpClient.PostAsJsonAsync(
                "/api/users", request, ct);
            var body = await response.Content.ReadFromJsonAsync<ProblemDetails>(ct);
            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.NotNull(body);
            Assert.Equal("Invalid Request", body!.Title);
            Assert.Equal(400, body!.Status);
        }

        [Fact]
        public async Task GetUserById_ReturnsOk_WhenUserExists()
        {
            // Arrange
            var ct = TestContext.Current.CancellationToken;
            using var db = _factory.Services.CreateScope().ServiceProvider.GetRequiredService<IdentityServiceDbContext>();
            var user = User.Create("test1@gmail.com", "John Doe", "password123", Role.User);
            await db.Users.AddAsync(user, ct);
            await db.SaveChangesAsync(ct);
            // Act
            using var response = await _httpClient.GetAsync(
                $"/api/users/{user.Id}", ct);
            // Assert
            var body = await response.Content.ReadFromJsonAsync<UserDto>(ct);
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
            Assert.Equal(user.Id, body!.Id);
            Assert.Equal(user.Email, body!.Email);
            Assert.Equal(user.FullName, body!.FullName);
            Assert.Equal(user.Role.ToString(), body!.Role);
            Assert.Equal(user.Version, body!.Version);
        }

        [Fact]
        public async Task GetUserById_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var ct = TestContext.Current.CancellationToken;
            var nonExistentUserId = Guid.NewGuid();
            // Act
            using var response = await _httpClient.GetAsync(
                $"/api/users/{nonExistentUserId}", ct);
            var body = await response.Content.ReadFromJsonAsync<ProblemDetails>(ct);
            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.NotNull(body);
            Assert.Equal("Not Found", body!.Title);
            Assert.Equal(404, body!.Status);
        }

        [Fact]
        public async Task GetAllUsers_ReturnsOk()
        {
            // Arrange
            var ct = TestContext.Current.CancellationToken;
            // Act
            using var response = await _httpClient.GetAsync(
                "/api/users", ct);
            var body = await response.Content.ReadFromJsonAsync<IEnumerable<UserDto>>(ct);
            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.NotNull(body);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            //Assert.IsType<IEnumerable<UserDto>>(body, exactMatch: false);
        }

        [Fact]
        public async Task UpdateUserProfile_ReturnsOk_WhenUserExists()
        {
            // Arrange
            var ct = TestContext.Current.CancellationToken;
            using var db = _factory.Services.CreateScope().ServiceProvider.GetRequiredService<IdentityServiceDbContext>();
            var user = User.Create("test2@gmail.com", "John Doe", "password123",Role.User);
            await db.Users.AddAsync(user, ct);
            await db.SaveChangesAsync(ct);
            // Act
            using var response = await _httpClient.PutAsJsonAsync(
                $"/api/users/{user.Id}", new UpdateProfileRequest("test", user.Version), ct);
            var body = await response.Content.ReadFromJsonAsync<UserDto>(ct);
           
            var userInDb = await db.Users.FirstOrDefaultAsync(u => u.Id == user.Id, ct);


            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(user.Id, body!.Id);
            Assert.Equal(user.Email, body!.Email);
            Assert.Equal("test", body!.FullName);
            Assert.Equal(user.Role.ToString(), body!.Role);
            Assert.Equal(user.Version + 1, body!.Version);
        }

        [Fact]
        public async Task UpdateUserProfile_ReturnsConlict_WithConcurrencyUpdate()
        {
            //Arrange
            var ct = TestContext.Current.CancellationToken;
            using var db = _factory.Services.CreateScope().ServiceProvider.GetRequiredService<IdentityServiceDbContext>();
            var user = User.Create("test222@gmail.com", "John Doe", "password123", Role.User);
            await db.Users.AddAsync(user, ct);
            await db.SaveChangesAsync(ct);

             var task1 = _httpClient.PutAsJsonAsync(
                $"/api/users/{user.Id}", new UpdateProfileRequest("test1", user.Version), ct);
            var task2 = _httpClient.PutAsJsonAsync(
                $"/api/users/{user.Id}", new UpdateProfileRequest("test2", user.Version), ct);

            // Act
            using var response1 = await task1;
            using var response2 = await task2;

            // Assert
            var statusCodes = new[]
            {
                  response1.StatusCode,
                  response2.StatusCode
            };

            Assert.Contains(HttpStatusCode.OK, statusCodes);
            Assert.Contains(HttpStatusCode.Conflict, statusCodes);
        }

        [Fact]
        public async Task DeleteUser_ReturnsNoContent_WithUserExist()
        {
            //Arrange
            var ct = TestContext.Current.CancellationToken;
            using var db = _factory.Services.CreateScope().ServiceProvider.GetRequiredService<IdentityServiceDbContext>();
            var user = User.Create("test2222@gmail.com", "John Doe", "password123", Role.User);
            await db.Users.AddAsync(user, ct);
            await db.SaveChangesAsync(ct);

            //Act
            using var response = await _httpClient.DeleteAsync($"api/users/{user.Id}", ct);

            //Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        public ValueTask DisposeAsync()
        {
           _httpClient!.Dispose();
            _factory.Dispose();
            return ValueTask.CompletedTask;
        }


    }
}
