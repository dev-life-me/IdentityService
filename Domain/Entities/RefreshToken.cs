

namespace Domain.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; init; } = Guid.CreateVersion7();
        public Guid UserId { get; init; }
        public Guid SeesionId { get; init; }
        public string TokenHash { get; init; } = default!;
        public DateTimeOffset ExpiresAt { get; init; }
        public DateTimeOffset? RevokeAt { get; set; }
        public string? ReplacedByToken { get; set; }
        public DateTimeOffset? ReplacedAt { get; set; }

        private RefreshToken() { }

    }
}
