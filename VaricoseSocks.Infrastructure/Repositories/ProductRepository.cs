using Microsoft.EntityFrameworkCore;
using VaricoseSocks.Application.Interfaces;
using VaricoseSocks.Domain.Entities;
using VaricoseSocks.Domain.Enums;
using VaricoseSocks.Infrastructure.Data;

namespace VaricoseSocks.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;

    public ProductRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Product>> GetAllAsync(int? categoryId = null, string? searchTerm = null, CompressionLevel? compression = null, SockStyle? style = null)
    {
        var query = _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Variants)
            .AsQueryable();

        if (categoryId.HasValue && categoryId.Value > 0)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(term) 
                                  || (p.Brand != null && p.Brand.ToLower().Contains(term))
                                  || (p.Description != null && p.Description.ToLower().Contains(term)));
        }

        if (compression.HasValue)
        {
            query = query.Where(p => p.CompressionLevel == compression.Value);
        }

        if (style.HasValue)
        {
            query = query.Where(p => p.Style == style.Value);
        }

        return await query.OrderByDescending(p => p.IsFeatured).ThenByDescending(p => p.Id).ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Variants)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Product?> GetBySlugAsync(string slug)
    {
        return await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Variants)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Slug == slug);
    }

    public async Task<Product> AddAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task UpdateAsync(Product product)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
    }
}