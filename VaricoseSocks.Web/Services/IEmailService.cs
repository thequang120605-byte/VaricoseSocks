namespace VaricoseSocks.Web.Services;

public interface IEmailService
{
    Task<(bool success, string? error)> SendOtpEmailAsync(string toEmail, string fullName, string otpCode);
}
