using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Services.Securities
{
    public class PasswordHasher : IPasswordHasher
    {
        public string DumpHash => throw new NotImplementedException();

        public string HashPassword(string password)
        {
            throw new NotImplementedException();
        }

        public bool VerifyPassword(string hashedPassword, string providedPassword, out bool needsRehash)
        {
            throw new NotImplementedException();
        }
    }
}
