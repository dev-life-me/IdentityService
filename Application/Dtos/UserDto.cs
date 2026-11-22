using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Dtos
{
    public record UserDto(Guid Id, string Email, string FullName, string Role, uint Version);
    public static class UserDtoExtensions
    {
        public static UserDto ToUserDto(this User user)
        {
            return new UserDto(user.Id, user.Email, user.FullName, user.Role.ToString(), user.Version);
        }

    }
}
