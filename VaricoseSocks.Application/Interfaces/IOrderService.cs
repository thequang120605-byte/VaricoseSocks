using VaricoseSocks.Application.DTOs;
using VaricoseSocks.Domain.Enums;

namespace VaricoseSocks.Application.Interfaces;

public interface IOrderService
{
    Task<(bool Success, string Message, OrderDto? Order)> CreateOrderAsync(CreateOrderDto dto);
    Task<(bool Success, string Message)> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus, PaymentStatus? newPaymentStatus = null);
    Task<IEnumerable<OrderDto>> GetOrdersAsync(OrderStatus? status = null);
    Task<OrderDto?> GetOrderByIdAsync(int id);
    Task<OrderDto?> GetOrderByCodeAsync(string orderCode);
    Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(int userId);
}
