using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces
{
    public interface ITokenProvider
    {
        (string AccessToken, string Jts, DateTime ExpiresAt) GenerateToken(User user);
        (string RawToken, string TokenHash) GenerateRefreshToken();
        string HashToken(string token);
        object GetJwks();
    }
}
