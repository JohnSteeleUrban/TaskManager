using TaskManager.Api.DTOs;

namespace TaskManager.Api.Services;

public interface IAuthService
{
    Task<(AuthResponse? Response, string? Error, int StatusCode)> RegisterAsync(RegisterRequest request);
    Task<(AuthResponse? Response, string? Error, int StatusCode)> LoginAsync(LoginRequest request);
}
