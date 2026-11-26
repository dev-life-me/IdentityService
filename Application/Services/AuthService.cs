using Application.Dtos.Auth;
using Application.Exceptions;
using Application.Interfaces;


namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenProvider _tokenProvider;
        private readonly IPasswordHasher _passwordHasher;

        public AuthService(IUserRepository userRepository, ITokenProvider tokenProvider, IPasswordHasher passwordHasher)
        {
            _passwordHasher = passwordHasher;
            _tokenProvider = tokenProvider;
            _userRepository = userRepository;
        }
        public async Task<LoginResponse> Login(LoginRequest request)
        {
            var user = await _userRepository.GetUserByNormalizeEmail(request.Email.Trim().ToUpperInvariant());
            
            if (user == null)
            {
                string dumpHash = _passwordHasher.HashPassword("Dumphash");
                _passwordHasher.VerifyPassword( dumpHash , request.Password);
                throw new UnAuthException("Incorrect email or password");
            }

            bool isValid = _passwordHasher.VerifyPassword(user.PasswordHash, request.Password);

            if (!isValid)
            {
                throw new UnAuthException("Incorrect email or password");
            }

            (string accessToken, string jts, DateTime expiresAt) = _tokenProvider.GenerateToken(user);

            return new LoginResponse(accessToken);
        }
    }
}
