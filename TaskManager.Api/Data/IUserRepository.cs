using TaskManager.Api.Models;

namespace TaskManager.Api.Data;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User> CreateAsync(User user);
}
