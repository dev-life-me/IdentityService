using Application.Interfaces;


namespace Infrastructure.Services.Securities
{
    public class PasswordHasher : IPasswordHasher
    {
        public string DumpHash => throw new NotImplementedException();

        public string HashPassword(string password)
        {
            string hash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
            return hash;
        }

        public bool VerifyPassword(string hashedPassword, string providedPassword, out bool needsRehash)
        {
            throw new NotImplementedException();
        }

        public bool VerifyPassword(string hashedPassword, string providedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword);
        }
    }
}
