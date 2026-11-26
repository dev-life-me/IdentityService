using Application.Dtos.Auth;
using Application.Exceptions;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Moq;
using Xunit;

namespace UnitTest.Application
{
    public class AuthServiceTest
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<ITokenProvider> _tokenProviderMock;
        private readonly Mock<IPasswordHasher> _passwordHasherMock;

        private readonly AuthService _authService;

        public AuthServiceTest()
        {
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _tokenProviderMock = new Mock<ITokenProvider>();

            _authService = new AuthService(_userRepositoryMock.Object, _tokenProviderMock.Object, _passwordHasherMock.Object);
        }

        [Fact]
        public async Task Login_ShouldReturnLoginResponse_WhenCredentialsAreValid()
        {
            // Arrange
            var request = new LoginRequest("john.doe@example.com", "Password123!");
            var normalizedEmail = "JOHN.DOE@EXAMPLE.COM";
            var user = User.Create("john.doe@example.com", "John Doe", "hashed_password", Role.User);
            var expectedToken = "jwt-access-token";

            _userRepositoryMock
                .Setup(repo => repo.GetUserByNormalizeEmail(normalizedEmail, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            _passwordHasherMock
                .Setup(hasher => hasher.VerifyPassword(user.PasswordHash, request.Password))
                .Returns(true);
            _tokenProviderMock
                .Setup(tp => tp.GenerateToken(user))
                .Returns((expectedToken, "jti-123", DateTime.UtcNow.AddMinutes(15)));

            // Act
            var response = await _authService.Login(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(expectedToken, response.Token);
            _userRepositoryMock.Verify(repo => repo.GetUserByNormalizeEmail(normalizedEmail, It.IsAny<CancellationToken>()), Times.Once);
            _passwordHasherMock.Verify(hasher => hasher.VerifyPassword(user.PasswordHash, request.Password), Times.Once);
            _passwordHasherMock.Verify(hasher => hasher.HashPassword(It.IsAny<string>()), Times.Never);
            _tokenProviderMock.Verify(tp => tp.GenerateToken(user), Times.Once);
        }

        [Theory]
        [InlineData("  john.doe@example.com  ", "JOHN.DOE@EXAMPLE.COM")]
        [InlineData("John.Doe@Example.Com", "JOHN.DOE@EXAMPLE.COM")]
        [InlineData("  USER@DOMAIN.VN\t", "USER@DOMAIN.VN")]
        public async Task Login_ShouldNormalizeEmail_BeforeQueryingRepository(string rawEmail, string expectedNormalizedEmail)
        {
            // Arrange
            var request = new LoginRequest(rawEmail, "Password123!");
            var user = User.Create("john.doe@example.com", "John Doe", "hashed_password", Role.User);

            _userRepositoryMock
                .Setup(repo => repo.GetUserByNormalizeEmail(expectedNormalizedEmail, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            _passwordHasherMock
                .Setup(hasher => hasher.VerifyPassword(user.PasswordHash, request.Password))
                .Returns(true);
            _tokenProviderMock
                .Setup(tp => tp.GenerateToken(user))
                .Returns(("access-token", "jti", DateTime.UtcNow.AddMinutes(15)));

            // Act
            var response = await _authService.Login(request);

            // Assert
            Assert.NotNull(response);
            _userRepositoryMock.Verify(repo => repo.GetUserByNormalizeEmail(expectedNormalizedEmail, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Login_ShouldThrowUnAuthExceptionAndVerifyDummyHash_WhenUserNotFound()
        {
            // Arrange
            var request = new LoginRequest("nonexistent@example.com", "Password123!");
            var normalizedEmail = "NONEXISTENT@EXAMPLE.COM";
            var dummyHash = "dummy_hashed_value";

            _userRepositoryMock
                .Setup(repo => repo.GetUserByNormalizeEmail(normalizedEmail, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);
            _passwordHasherMock
                .Setup(hasher => hasher.HashPassword("Dumphash"))
                .Returns(dummyHash);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnAuthException>(() => _authService.Login(request));
            Assert.Equal("Incorrect email or password", exception.Message);

            _userRepositoryMock.Verify(repo => repo.GetUserByNormalizeEmail(normalizedEmail, It.IsAny<CancellationToken>()), Times.Once);
            _passwordHasherMock.Verify(hasher => hasher.HashPassword("Dumphash"), Times.Once);
            _passwordHasherMock.Verify(hasher => hasher.VerifyPassword(dummyHash, request.Password), Times.Once);
            _tokenProviderMock.Verify(tp => tp.GenerateToken(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task Login_ShouldThrowUnAuthException_WhenPasswordIsIncorrect()
        {
            // Arrange
            var request = new LoginRequest("john.doe@example.com", "WrongPassword!");
            var normalizedEmail = "JOHN.DOE@EXAMPLE.COM";
            var user = User.Create("john.doe@example.com", "John Doe", "hashed_password", Role.User);

            _userRepositoryMock
                .Setup(repo => repo.GetUserByNormalizeEmail(normalizedEmail, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            _passwordHasherMock
                .Setup(hasher => hasher.VerifyPassword(user.PasswordHash, request.Password))
                .Returns(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnAuthException>(() => _authService.Login(request));
            Assert.Equal("Incorrect email or password", exception.Message);

            _userRepositoryMock.Verify(repo => repo.GetUserByNormalizeEmail(normalizedEmail, It.IsAny<CancellationToken>()), Times.Once);
            _passwordHasherMock.Verify(hasher => hasher.VerifyPassword(user.PasswordHash, request.Password), Times.Once);
            _passwordHasherMock.Verify(hasher => hasher.HashPassword(It.IsAny<string>()), Times.Never);
            _tokenProviderMock.Verify(tp => tp.GenerateToken(It.IsAny<User>()), Times.Never);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task Login_ShouldThrowUnAuthException_WhenEmailIsEmptyOrWhitespace(string email)
        {
            // Arrange
            var request = new LoginRequest(email, "Password123!");
            var dummyHash = "dummy_hashed_value";

            _userRepositoryMock
                .Setup(repo => repo.GetUserByNormalizeEmail(string.Empty, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);
            _passwordHasherMock
                .Setup(hasher => hasher.HashPassword("Dumphash"))
                .Returns(dummyHash);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnAuthException>(() => _authService.Login(request));
            Assert.Equal("Incorrect email or password", exception.Message);
            _tokenProviderMock.Verify(tp => tp.GenerateToken(It.IsAny<User>()), Times.Never);
        }
    }
}
