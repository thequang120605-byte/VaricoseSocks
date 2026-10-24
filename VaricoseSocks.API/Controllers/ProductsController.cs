using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VaricoseSocks.Application.DTOs;
using VaricoseSocks.Application.Interfaces;
using VaricoseSocks.Domain.Entities;
using VaricoseSocks.Domain.Enums;

namespace VaricoseSocks.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _repo;

    public ProductsController(IProductRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? categoryId, [FromQuery] string? searchTerm, [FromQuery] CompressionLevel? compression, [FromQuery] SockStyle? style)
    {
        var products = await _repo.GetAllAsync(categoryId, searchTerm, compression, style);
        var dtos = products.Select(p => MapToDto(p));
        return Ok(dtos);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var p = await _repo.GetByIdAsync(id);
        if (p == null) return NotFound(new { message = "Không tìm thấy sản phẩm." });

        return Ok(MapToDto(p));
    }

    [HttpGet("slug/{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var p = await _repo.GetBySlugAsync(slug);
        if (p == null) return NotFound(new { message = "Không tìm thấy sản phẩm." });

        return Ok(MapToDto(p));
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {
        var product = new Product
        {
            Name = dto.Name,
            Slug = GenerateSlug(dto.Name),
            Sku = dto.Sku ?? ("VS-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper()),
            ShortDescription = dto.ShortDescription,
            Description = dto.Description,
            Price = dto.Price,
            DiscountPrice = dto.DiscountPrice,
            StockQuantity = dto.StockQuantity,
            CategoryId = dto.CategoryId,
            ImageUrl = dto.ImageUrl,
            CompressionLevel = dto.CompressionLevel,
            Style = dto.Style,
            ToeType = dto.ToeType,
            Brand = dto.Brand,
            Origin = dto.Origin,
            Material = dto.Material,
            IsFeatured = dto.IsFeatured
        };

        var created = await _repo.AddAsync(product);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToDto(created));
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateProductDto dto)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null) return NotFound(new { message = "Không tìm thấy sản phẩm." });

        existing.Name = dto.Name;
        existing.ShortDescription = dto.ShortDescription;
        existing.Description = dto.Description;
        existing.Price = dto.Price;
        existing.DiscountPrice = dto.DiscountPrice;
        existing.StockQuantity = dto.StockQuantity;
        existing.CategoryId = dto.CategoryId;
        if (!string.IsNullOrEmpty(dto.ImageUrl)) existing.ImageUrl = dto.ImageUrl;
        existing.CompressionLevel = dto.CompressionLevel;
        existing.Style = dto.Style;
        existing.ToeType = dto.ToeType;
        existing.Brand = dto.Brand;
        existing.Origin = dto.Origin;
        existing.Material = dto.Material;
        existing.IsFeatured = dto.IsFeatured;
        existing.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(existing);
        return Ok(MapToDto(existing));
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null) return NotFound();

        await _repo.DeleteAsync(id);
        return NoContent();
    }

    private static ProductDto MapToDto(Product p)
    {
        return new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Slug = p.Slug,
            Sku = p.Sku,
            ShortDescription = p.ShortDescription,
            Description = p.Description,
            Price = p.Price,
            DiscountPrice = p.DiscountPrice,
            StockQuantity = p.StockQuantity,
            ImageUrl = p.ImageUrl,
            CompressionLevel = p.CompressionLevel,
            Style = p.Style,
            ToeType = p.ToeType,
            Brand = p.Brand,
            Origin = p.Origin,
            Material = p.Material,
            UsageInstructions = p.UsageInstructions,
            SizeGuide = p.SizeGuide,
            IsFeatured = p.IsFeatured,
            CategoryId = p.CategoryId,
            CategoryName = p.Category?.Name,
            Variants = p.Variants.Select(v => new ProductVariantDto
            {
                Id = v.Id,
                ProductId = v.ProductId,
                Sku = v.Sku,
                Size = v.Size,
                Color = v.Color,
                StockQuantity = v.StockQuantity,
                PriceAdjustment = v.PriceAdjustment
            }).ToList()
        };
    }

    private static string GenerateSlug(string text)
    {
        return text.ToLower()
            .Replace(" ", "-")
            .Replace("đ", "d")
            .Replace("á", "a").Replace("à", "a").Replace("ả", "a").Replace("ã", "a").Replace("ạ", "a")
            .Replace("é", "e").Replace("è", "e").Replace("ẻ", "e").Replace("ẽ", "e").Replace("ẹ", "e")
            .Replace("í", "i").Replace("ì", "i").Replace("ỉ", "i").Replace("ĩ", "i").Replace("ị", "i")
            .Replace("ó", "o").Replace("ò", "o").Replace("ỏ", "o").Replace("õ", "o").Replace("ọ", "o")
            .Replace("ú", "u").Replace("ù", "u").Replace("ủ", "u").Replace("ũ", "u").Replace("ụ", "u");
    }
}