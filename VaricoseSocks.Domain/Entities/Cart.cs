namespace VaricoseSocks.Domain.Entities;

public class Cart
{
    public int Id { get; set; }
    public int? UserId { get; set; }                       // Nullable nếu khách vãng lai (Guest)
    public string? SessionId { get; set; }                 // Mã session hoặc cookie token
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public User? User { get; set; }
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}
