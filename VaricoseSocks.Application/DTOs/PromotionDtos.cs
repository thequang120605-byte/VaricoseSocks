namespace VaricoseSocks.Application.DTOs;

public class ValidateCouponDto
{
    public string Code { get; set; } = string.Empty;
    public decimal OrderAmount { get; set; }
}

public class CouponResultDto
{
    public bool Valid { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public string Message { get; set; } = string.Empty;
}
