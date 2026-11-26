using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Dtos.Auth
{
    public record LoginRequest(string Email, string Password);
}
