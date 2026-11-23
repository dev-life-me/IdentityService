using Application.Dtos;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Exceptions;
using Infrastructure.Persistence.AppDbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
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
            try
            {
                await _dbContext.Users.AddAsync(user, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }catch (DbUpdateException ex)
            {
                System.Console.WriteLine(ex.Message);
                throw new ConflictException(ex.Message);
            }
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default)
        {
           return await _dbContext.Users.Select(u => u.ToUserDto()).ToListAsync(cancellationToken);
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
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConflictException("The user profile has been modified by another process.");
            }
        }

    }
}
