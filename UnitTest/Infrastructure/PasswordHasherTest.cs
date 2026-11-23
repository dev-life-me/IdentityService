using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace UnitTest.Infrastructure
{
    public class PasswordHasherTest
    {
        [Theory]
        [InlineData("password")]
        public void HashPassword_ReturnHashString(string password)
        {
            //Arrange & Act
            string hashString = BCrypt.Net.BCrypt.HashPassword(password, 12);

            //Assert
            Assert.NotEqual(hashString, password);
            
        }

        [Theory]
        [InlineData("password")]
        public void VerifyPassword_ReturnsTrue_WithValidPassword(string password)
        {
            //Arrange & Act
            string hashString = BCrypt.Net.BCrypt.HashPassword(password, 12);
            bool isValid = BCrypt.Net.BCrypt.Verify(password, hashString);
            //Assert
            Assert.True(isValid);

        }

        [Theory]
        [InlineData("password")]
        public void VerifyPassword_ReturnsFalse_WithInValidPassword(string password)
        {
            //Arrange & Act
            string hashString = BCrypt.Net.BCrypt.HashPassword(password, 12);
            bool isValid = BCrypt.Net.BCrypt.Verify("wrong_password", hashString);
            //Assert
            Assert.False(isValid);

        }
    }
}
