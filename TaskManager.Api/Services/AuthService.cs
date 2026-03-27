using TaskManager.Api.Auth;
using TaskManager.Api.Data;
using TaskManager.Api.DTOs;
using TaskManager.Api.Models;

namespace TaskManager.Api.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    //Returning http status codes not best practice.  Technically this should be at the controller level but for the sake of time I just included them here 
    public async Task<(AuthResponse? Response, string? Error, int StatusCode)> RegisterAsync(RegisterRequest request)
    {
        var existing = await _userRepository.GetByEmailAsync(request.Email);
        if (existing != null)
        {
            _logger.LogWarning("Registration attempt with existing email: {Email}", request.Email);
            return (null, "An account with this email already exists.", 409);
        }

        var user = new User
        {
            // v7 guids are time-ordered, so they don't fragment clustered indexes like random v4s would
            //
            Id = Guid.CreateVersion7(),
            Email = request.Email.ToLowerInvariant(),
            PasswordHash = _passwordHasher.Hash(request.Password),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(user);
        _logger.LogInformation("User registered: {UserId}", user.Id);

        var token = _jwtTokenService.GenerateToken(user);

        return (BuildAuthResponse(token, user), null, 201);
    }

    public async Task<(AuthResponse? Response, string? Error, int StatusCode)> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email.ToLowerInvariant());
        if (user == null)
        {
            _logger.LogWarning("Login attempt for non-existent email: {Email}", request.Email);
            return (null, "Invalid email or password.", 401);
        }

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for user: {UserId}", user.Id);
            return (null, "Invalid email or password.", 401);
        }

        _logger.LogInformation("User logged in: {UserId}", user.Id);

        var token = _jwtTokenService.GenerateToken(user);

        return (BuildAuthResponse(token, user), null, 200);
    }

    private static AuthResponse BuildAuthResponse(string token, User user)
    {
        return new AuthResponse
        {
            Token = token,
            User = user
        };
    }
}
