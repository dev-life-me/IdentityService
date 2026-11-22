using Application.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces
{
    public interface IUserService
    {
        public Task<UserDto> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
        public Task<UserDto> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);
        public Task<UserDto> UpdateUserProfileAsync(Guid id, UpdateProfileRequest request, CancellationToken cancellationToken = default);
        public Task DeleteUserAsync(Guid id, CancellationToken cancellationToken = default);

        public Task<IEnumerable<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default);
    }
}
