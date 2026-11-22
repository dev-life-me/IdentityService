using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Dtos
{
    public record CreateUserRequest(string Email, string FullName, string Password, Role Role = Role.User);
}
