using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.AppDbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private IdentityServiceDbContext _dbContext;
        public UserRepository(IdentityServiceDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task CreateUserAsync(User user, CancellationToken cancellationToken = default)
        {
             await _dbContext.Users.AddAsync(user, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }


        public async Task<IEnumerable<User>> GetAllUsersAsync(CancellationToken cancellationToken = default)
        {
           return await _dbContext.Users.ToListAsync(cancellationToken);
        }

        public async Task<User?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Users.Where(u => u.Id == id).FirstOrDefaultAsync(cancellationToken);
        }

        public Task SoftDeleteUserAsync(User user, CancellationToken cancellationToken = default)
        {
            user.DeleteUser();
            return _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateUserProfileAsync(User user, uint expectedVersion, CancellationToken cancellationToken = default)
        {
            await _dbContext.Users.Where(u => u.Id == user.Id && u.Version == expectedVersion)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(u => u.FullName, user.FullName)
                    .SetProperty(u => u.Version, user.Version + 1), cancellationToken);
        }


    }
}
