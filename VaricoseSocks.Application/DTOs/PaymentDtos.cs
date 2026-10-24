namespace VaricoseSocks.Application.DTOs;

public class VietQrRequestDto
{
    public string OrderCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class VNPayRequestDto
{
    public string OrderCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class PaymentCallbackDto
{
    public string OrderCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int PaymentMethod { get; set; }
    public string? TransactionId { get; set; }
    public bool Success { get; set; }
    public string? RawResponse { get; set; }
    public string? Signature { get; set; } // Cryptographic verification signature
}

public class PaymentMethodDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsOnline { get; set; }
}
