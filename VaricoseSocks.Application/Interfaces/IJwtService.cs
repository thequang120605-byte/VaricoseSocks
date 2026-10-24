using VaricoseSocks.Domain.Entities;

namespace VaricoseSocks.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user, IEnumerable<string> roles);
}
