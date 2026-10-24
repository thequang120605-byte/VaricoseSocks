using VaricoseSocks.Application.DTOs;

namespace VaricoseSocks.Application.Interfaces;

public interface IInventoryService
{
    Task<IEnumerable<InventoryStatusDto>> GetInventoryStatusAsync();
    Task<(bool Success, string Message, int CurrentStock)> ImportStockAsync(InventoryImportDto dto);
    Task<IEnumerable<InventoryTransactionDto>> GetTransactionsAsync();
}
