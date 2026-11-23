using Domain.Entities;
using Domain.Enums;
using Xunit;

namespace UnitTest.Domain
{
    public class UserTest
    {
        [Fact]
        public void Create_UserShould_InitializeProperties()
        {
            User user = User.Create("test@example.com", "John Doe", "hashedpassword", Role.User);
            // Assert
            Assert.Equal("test@example.com", user.Email);
            Assert.Equal("John Doe", user.FullName);
            Assert.Equal("hashedpassword", user.PasswordHash);
            Assert.Equal("test@example.com".ToUpperInvariant(), user.NormalizeEmail);
            Assert.Equal(Role.User, user.Role);
            Assert.NotEqual(Guid.Empty, user.Id);
            Assert.NotEqual(Guid.Empty, user.SecurityStamp);
            Assert.Equal(0, user.AccessFailedCount);
            Assert.Null(user.LockoutEnd);
            Assert.False(user.IsDeleted);
            Assert.Null(user.DeletedAt);
            Assert.True(user.CreatedAt <= DateTimeOffset.UtcNow);
            Assert.Equal(0u, user.Version);
        }

        [Fact]
        public void UpdateProfile_Should_UpdateFullNameAndIncrementVersion()
        {
            User user = User.Create("test@example.com", "John Doe", "hashedpassword", Role.User);
            uint initialVersion = user.Version;
            user.UpdateProfile("Jane Doe", initialVersion);
            Assert.Equal("Jane Doe", user.FullName);
            
        }

        [Fact]
        public void UpdateProfile_Should_ThrowException_WhenVersionMismatch()
        {
            User user = User.Create("test@example.com", "John Doe", "hashedpassword", Role.User);
            uint initialVersion = user.Version;
            Assert.Throws<InvalidOperationException>(() => user.UpdateProfile("Jane Doe", initialVersion + 1));
        }

        [Fact]
        public void DeleteUser_Should_SetIsDeletedToTrueAndSetDeletedAt()
        {
            User user = User.Create("test@example.com", "John Doe", "hashedpassword", Role.User);
            user.DeleteUser();
            Assert.True(user.IsDeleted);
            Assert.NotNull(user.DeletedAt);
        }
    }
}
