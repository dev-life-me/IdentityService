using Application.Dtos;
using Application.Interfaces;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUserRepository _userRepository;
        public UserService(IPasswordHasher passwordHasher, IUserRepository userRepository)
        {
            _passwordHasher = passwordHasher;
            _userRepository = userRepository;
        }
        public async Task<UserDto> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
        {
           User newUser = User.Create(request.Email, request.FullName, request.Password, request.Role);
            await _userRepository.CreateUserAsync(newUser, cancellationToken);
           return newUser.ToUserDto();
        }

        public async Task DeleteUserAsync(Guid id, CancellationToken cancellationToken = default)
        {
            User? user = await _userRepository.GetUserByIdAsync(id, cancellationToken);
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }
            await _userRepository.SoftDeleteUserAsync(user, cancellationToken);
        }

        public Task<IEnumerable<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<UserDto> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            User? user = await _userRepository.GetUserByIdAsync(id, cancellationToken);
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }
            return user.ToUserDto();
        }

        public async Task<UserDto> UpdateUserProfileAsync(Guid id, UpdateProfileRequest request, CancellationToken cancellationToken = default)
        {
            User? user = await _userRepository.GetUserByIdAsync(id, cancellationToken);
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }
            user.UpdateProfile(request.FullName, request.Version);

            await _userRepository.UpdateUserProfileAsync( user, request.Version, cancellationToken);
            return user.ToUserDto();
        }
    }
}
