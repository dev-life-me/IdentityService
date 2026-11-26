
using Domain.Enums;

namespace Domain.Entities
{
    public class User
    {
        public Guid Id { get; init; } = Guid.CreateVersion7();
        public string Email { get; set; } = default!;

        public string FullName { get; private set; } = default!;
        public string NormalizeEmail { get; private set; } = default!;
        public string PasswordHash { get; private set; } = default!;
        public Role Role { get; private set; } = Role.User;
        public Guid SecurityStamp { get; private set; } = Guid.NewGuid();
        public int AccessFailedCount { get; private set; } = 0;
        public DateTimeOffset? LockoutEnd { get; private set; }
        public bool IsDeleted { get; private set; } = false;
        public DateTimeOffset? DeletedAt { get; private set; }
        public DateTimeOffset CreatedAt { get;  private set; } = DateTimeOffset.UtcNow;
        public uint Version { get;  private set; }

        public ICollection<UserSession> UserSessions { get; private set; } = new List<UserSession>();

        private User() { }

        public static User Create(string? email, string fullName, string passwordHash, Role role)
        {
            if(string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email cannot be null or empty");
            }
            return new User
            {
                Email = email.Trim(),
                FullName = fullName.Trim(),
                NormalizeEmail = email.Trim().ToUpperInvariant(),
                PasswordHash = passwordHash,
                Role = role,
            };
        }

        public void UpdateProfile(string fullName, uint version) 
        {
            if (version != Version)
            {
                throw new InvalidOperationException("Version mismatch");
            }
            FullName = fullName.Trim();
   
        }

        public void DeleteUser() 
        {
            IsDeleted = true;
            DeletedAt = DateTimeOffset.UtcNow;
        }
    }
}
