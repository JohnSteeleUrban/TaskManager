using TaskManager.Api.Models;

namespace TaskManager.Api.Auth;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}
