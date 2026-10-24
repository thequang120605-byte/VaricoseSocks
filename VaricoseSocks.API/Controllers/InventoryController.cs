using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VaricoseSocks.Application.DTOs;
using VaricoseSocks.Application.Interfaces;

namespace VaricoseSocks.API.Controllers;

[Authorize(Roles = "Admin,Staff")]
[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetInventoryStatus()
    {
        var status = await _inventoryService.GetInventoryStatusAsync();
        return Ok(status);
    }

    [HttpPost("import")]
    public async Task<IActionResult> ImportStock([FromBody] InventoryImportDto dto)
    {
        var result = await _inventoryService.ImportStockAsync(dto);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        return Ok(new { message = result.Message, currentStock = result.CurrentStock });
    }

    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions()
    {
        var list = await _inventoryService.GetTransactionsAsync();
        return Ok(list);
    }
}
