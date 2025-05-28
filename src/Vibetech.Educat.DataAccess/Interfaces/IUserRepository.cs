using Vibetech.Educat.DataAccess.Models;

namespace Vibetech.Educat.DataAccess.Interfaces;

public interface IUserRepository
{
    Task<User?> GetUserByIdAsync(int id);
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User?> GetUserByEmailAsync(string email);
    Task<IEnumerable<User>> GetUsersByRoleAsync(string role);
    Task<User> UpdateUserAsync(User user);
} 