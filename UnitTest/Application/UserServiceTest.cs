

using Application.Dtos;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Moq;
using Xunit;

namespace UnitTest.Application
{
    public class UserServiceTest
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IPasswordHasher> _passwordHasherMock;
        private readonly IUserService _userService;

        public UserServiceTest()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _userService = new UserService(_passwordHasherMock.Object, _userRepositoryMock.Object);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task CreateUserAsync_ShouldThrowArgumentException_WhenEmailIsEmpty(string? email)
        {
        
            // Arrange
            var request = new CreateUserRequest(email!, "John Doe", "password123", Role.User);
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(  () => _userService.CreateUserAsync(request, TestContext.Current.CancellationToken));
            _userRepositoryMock.Verify(repo => repo.CreateUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldCreateUser_WhenRequestIsValid()
        {
            // Arrange
            var request = new CreateUserRequest("john.doe@example.com", "John Doe", "password123", Role.User);
            // Act & Assert
            var userDto = await _userService.CreateUserAsync(request, TestContext.Current.CancellationToken);
            Assert.NotNull(userDto);
            Assert.Equal(request.Email, userDto.Email);
            Assert.Equal(request.FullName, userDto.FullName);
            Assert.Equal(request.Role.ToString(), userDto.Role);
            Assert.NotEqual(Guid.Empty, userDto.Id);
            _userRepositoryMock.Verify(repo => repo.CreateUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldThrowInvalidOperationException_WhenUserNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            object kk = _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);
            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.GetUserByIdAsync(userId, TestContext.Current.CancellationToken));
            _userRepositoryMock.Verify(repo => repo.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnUserDto_WhenUserExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = User.Create("john.doe@example.com", "John Doe", "password123", Role.User);
            _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            // Act & Assert
            var result = await _userService.GetUserByIdAsync(userId, TestContext.Current.CancellationToken);
            Assert.NotNull(result);
            _userRepositoryMock.Verify(repo => repo.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateUserProfileAsync_ShouldThrowInvalidOperationException_WhenUserNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new UpdateProfileRequest("Jane Doe", 0);
            _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);
            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.UpdateUserProfileAsync(userId, request, TestContext.Current.CancellationToken));
           
            _userRepositoryMock.Verify(repo => repo.UpdateUserProfileAsync(It.IsAny<User>(), It.IsAny<uint>(), It.IsAny<CancellationToken>()), Times.Never);
        }


        [Fact]
        public async Task UpdateUserProfileAsync_ShouldUpdateUserProfile_WhenUserExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = User.Create("john.doe@example.com", "John Doe", "password123", Role.User);
            var request = new UpdateProfileRequest("Jane Doe", 0);
            _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            // Act & Assert
            await _userService.UpdateUserProfileAsync(userId, request, TestContext.Current.CancellationToken);
            Assert.Equal(request.FullName, user.FullName);
            Assert.Equal(request.Version + 1, user.Version);
            Assert.Equal("Jane Doe", user.FullName);
            Assert.Equal(1u, user.Version);
            _userRepositoryMock.Verify(repo => repo.UpdateUserProfileAsync(user, request.Version, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteUserAsync_ShouldThrowInvalidOperationException_WhenUserNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);
            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.DeleteUserAsync(userId, TestContext.Current.CancellationToken));
            _userRepositoryMock.Verify(repo => repo.SoftDeleteUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DeleteUserAsync_ShouldDeleteUser_WhenUserExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = User.Create("john.doe@example.com", "John Doe", "password123", Role.User);
            _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            // Act & Assert
            await _userService.DeleteUserAsync(userId, TestContext.Current.CancellationToken);
            _userRepositoryMock.Verify(repo => repo.SoftDeleteUserAsync(user, It.IsAny<CancellationToken>()), Times.Once);  

        }
    }
}
