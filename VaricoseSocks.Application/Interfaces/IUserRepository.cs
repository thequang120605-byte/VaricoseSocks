using VaricoseSocks.Domain.Entities;

namespace VaricoseSocks.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<User> CreateUserAsync(User user, string roleName = "Customer");
    Task<bool> CheckEmailExistsAsync(string email);
    Task<IEnumerable<string>> GetUserRolesAsync(int userId);
}
