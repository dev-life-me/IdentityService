using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Services.Securities.Configs;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Services.Securities
{
    public class TokenService : ITokenProvider
    {
        private readonly RsaSecurityKey _rsaSecurityKey;
        private readonly JwtOptions _options;
        public TokenService(IOptions<JwtOptions> opt)
        {
            _options = opt.Value;
            using var rsa = RSA.Create();
            rsa.ImportFromPem(File.ReadAllText(opt.Value.PrivateKeyPath));
            _rsaSecurityKey = new RsaSecurityKey(rsa.ExportParameters(true));
        }
        public (string RawToken, string TokenHash) GenerateRefreshToken()
        {
            throw new NotImplementedException();
        }

        public (string AccessToken, string Jts, DateTime ExpiresAt) GenerateToken(User user)
        {
            var crendentials = new SigningCredentials(_rsaSecurityKey, SecurityAlgorithms.RsaSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())

            };
            var token = new JwtSecurityToken
                (
                    issuer: _options.Issuer,
                    audience: _options.Audience,
                    claims: claims,
                    notBefore: DateTime.UtcNow,
                    expires: DateTime.UtcNow.AddMinutes(_options.ExpireTimeMinute),
                    signingCredentials: crendentials
                );
            string accessToken = new JwtSecurityTokenHandler().WriteToken(token);
            return (accessToken, "", DateTime.UtcNow.AddMinutes(15));

        }

        public object GetJwks()
        {
            throw new NotImplementedException();
        }

        public string HashToken(string token)
        {
            throw new NotImplementedException();
        }
    }
}
