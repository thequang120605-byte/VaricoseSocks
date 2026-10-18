using VaricoseSocks.Domain.Entities;
using VaricoseSocks.Domain.Enums;

namespace VaricoseSocks.Application.Interfaces;

public interface IOrderRepository
{
    Task<IEnumerable<Order>> GetAllAsync(OrderStatus? status = null);
    Task<IEnumerable<Order>> GetByUserIdAsync(int userId);
    Task<Order?> GetByIdAsync(int id);
    Task<Order?> GetByOrderCodeAsync(string orderCode);
    Task<Order> CreateOrderAsync(Order order);
    Task<bool> UpdateStatusAsync(int orderId, OrderStatus orderStatus, PaymentStatus? paymentStatus = null);
}
