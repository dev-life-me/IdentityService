using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class UserSession
    {
        public Guid Id { get; init; } = Guid.CreateVersion7();
        public Guid UserId { get; set; }
        public string DeviceId { get; set; } = "Unkown";
        public string OperationSystem { get; set; } = "Unkown";
        public string DeviceName { get; set; } = "Unkown";
        public string UserAgent { get; set; } = "Unkown";
        public string Browser { get; set; } = "Unkown";
        public string CreatedIpAddress { get; set; } = "Unkown";
        public string LastIpAddress { get; set; } = "Unkown";

        public bool IsRevoke { get; set; } = false;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;
        public DateTimeOffset? RevokedAt { get; set; }
        public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();

        private UserSession() { }

    }
}
