using VaricoseSocks.Domain.Entities;
using VaricoseSocks.Domain.Enums;

namespace VaricoseSocks.Application.Interfaces;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync(int? categoryId = null, string? searchTerm = null, CompressionLevel? compression = null, SockStyle? style = null);
    Task<Product?> GetByIdAsync(int id);
    Task<Product?> GetBySlugAsync(string slug);
    Task<Product> AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(int id);
}