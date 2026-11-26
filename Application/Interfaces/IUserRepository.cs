using Application.Dtos;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces
{
    public interface IUserRepository 
    {
        public Task CreateUserAsync(User user, CancellationToken cancellaonToken = default);
        public Task<User?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);
        public Task UpdateUserProfileAsync( User user, uint expectedVersion, CancellationToken cancellationToken = default);
        public Task SoftDeleteUserAsync(User user, CancellationToken cancellationToken = default);
        public Task<IEnumerable<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default);

        public Task<User?> GetUserByNormalizeEmail(string email, CancellationToken cancellationToken = default);
    }
}
