using System.Net;
using System.Net.Mail;

namespace VaricoseSocks.Web.Services;

public class GmailEmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<GmailEmailService> _logger;

    public GmailEmailService(IConfiguration config, ILogger<GmailEmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task<(bool success, string? error)> SendOtpEmailAsync(string toEmail, string fullName, string otpCode)
    {
        var smtpServer = _config["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
        var port = int.TryParse(_config["EmailSettings:Port"], out var p) ? p : 587;
        var senderEmail = _config["EmailSettings:SenderEmail"] ?? "";
        var appPassword = _config["EmailSettings:AppPassword"] ?? "";
        var senderName = _config["EmailSettings:SenderName"] ?? "Varicose Socks Y Khoa";

        if (string.IsNullOrWhiteSpace(senderEmail) || string.IsNullOrWhiteSpace(appPassword))
        {
            _logger.LogWarning("Chưa cấu hình SenderEmail hoặc AppPassword trong appsettings.json.");
            return (false, "Chưa thiết lập SenderEmail và Google App Password trong appsettings.json.");
        }

        try
        {
            using var client = new SmtpClient(smtpServer, port)
            {
                Credentials = new NetworkCredential(senderEmail.Trim(), appPassword.Trim().Replace(" ", "")),
                EnableSsl = true,
                Timeout = 15000
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail.Trim(), senderName),
                Subject = $"[{otpCode}] Mã xác thực OTP đăng ký tài khoản - Varicose Socks Y Khoa",
                IsBodyHtml = true,
                Body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"" />
</head>
<body style=""background-color: #f1f5f9; font-family: 'Segoe UI', Arial, sans-serif; padding: 30px 10px; margin: 0;"">
    <div style=""max-width: 580px; margin: 0 auto; background: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 20px rgba(0,0,0,0.08); border: 1px solid #e2e8f0;"">
        <!-- Header -->
        <div style=""background: linear-gradient(135deg, #0284c7, #1e40af); padding: 30px 24px; text-align: center; color: #ffffff;"">
            <h1 style=""margin: 0; font-size: 24px; font-weight: 800; letter-spacing: -0.5px;"">VARICOSE SOCKS MEDICAL</h1>
            <p style=""margin: 6px 0 0; font-size: 13px; opacity: 0.9; letter-spacing: 0.5px;"">HỆ THỐNG VỚ Y KHOA ĐIỀU TRỊ SUY GIÃN TĨNH MẠCH</p>
        </div>

        <!-- Content -->
        <div style=""padding: 32px 28px;"">
            <h3 style=""color: #0f172a; margin-top: 0; font-size: 18px;"">Kính gửi Quý khách {fullName},</h3>
            <p style=""color: #475569; font-size: 14px; line-height: 1.6; margin-bottom: 24px;"">
                Bạn vừa yêu cầu đăng ký tài khoản tại hệ thống phân phối vớ y khoa chuyên biệt <strong>Varicose Socks</strong>. Vui lòng sử dụng mã xác thực OTP dưới đây để hoàn tất quá trình kích hoạt:
            </p>

            <!-- OTP Box -->
            <div style=""text-align: center; margin: 30px 0;"">
                <div style=""display: inline-block; background: #f0fdf4; border: 2px dashed #16a34a; border-radius: 12px; padding: 18px 40px;"">
                    <span style=""font-size: 38px; font-weight: 800; letter-spacing: 8px; color: #15803d; font-family: monospace;"">{otpCode}</span>
                </div>
                <div style=""margin-top: 10px; color: #dc2626; font-size: 12px; font-weight: 600;"">
                    ⏱️ Mã OTP có hiệu lực trong vòng 5 phút (Tuyệt đối không chia sẻ mã này)
                </div>
            </div>

            <p style=""color: #64748b; font-size: 13px; line-height: 1.5;"">
                Sau khi kích hoạt tài khoản, bạn có thể bổ sung hồ sơ kích thước vòng chân (cB, cC, cG) để nhận phác đồ vớ điều trị chuẩn quốc tế <strong>RAL-GZ 387</strong>.
            </p>
        </div>

        <!-- Footer -->
        <div style=""background-color: #f8fafc; padding: 20px 24px; text-align: center; border-top: 1px solid #e2e8f0; font-size: 12px; color: #94a3b8; line-height: 1.5;"">
            Hotline bác sĩ tư vấn: <strong style=""color: #0284c7;"">1900 6868</strong> | Trụ sở y tế: 123 Hai Bà Trưng, Quận 1, TP.HCM<br />
            Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email.
        </div>
    </div>
</body>
</html>"
            };

            mailMessage.To.Add(toEmail.Trim());
            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("Đã gửi email OTP thành công tới: {Email}", toEmail);
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi gửi email OTP tới {Email}", toEmail);
            return (false, ex.Message);
        }
    }

    public async Task<(bool success, string? error)> SendForgotPasswordOtpAsync(string toEmail, string fullName, string otpCode)
    {
        var smtpServer = _config["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
        var port = int.TryParse(_config["EmailSettings:Port"], out var p) ? p : 587;
        var senderEmail = _config["EmailSettings:SenderEmail"] ?? "";
        var appPassword = _config["EmailSettings:AppPassword"] ?? "";
        var senderName = _config["EmailSettings:SenderName"] ?? "Varicose Socks Y Khoa";

        if (string.IsNullOrWhiteSpace(senderEmail) || string.IsNullOrWhiteSpace(appPassword))
        {
            _logger.LogWarning("Chưa cấu hình SenderEmail hoặc AppPassword trong appsettings.json.");
            return (false, "Chưa thiết lập SenderEmail và Google App Password trong appsettings.json.");
        }

        try
        {
            using var client = new SmtpClient(smtpServer, port)
            {
                Credentials = new NetworkCredential(senderEmail.Trim(), appPassword.Trim().Replace(" ", "")),
                EnableSsl = true,
                Timeout = 15000
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail.Trim(), senderName),
                Subject = $"[{otpCode}] Mã xác thực OTP khôi phục mật khẩu - Varicose Socks Y Khoa",
                IsBodyHtml = true,
                Body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"" />
</head>
<body style=""background-color: #f1f5f9; font-family: 'Segoe UI', Arial, sans-serif; padding: 30px 10px; margin: 0;"">
    <div style=""max-width: 580px; margin: 0 auto; background: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 20px rgba(0,0,0,0.08); border: 1px solid #e2e8f0;"">
        <!-- Header -->
        <div style=""background: linear-gradient(135deg, #0284c7, #1e40af); padding: 30px 24px; text-align: center; color: #ffffff;"">
            <h1 style=""margin: 0; font-size: 24px; font-weight: 800; letter-spacing: -0.5px;"">VARICOSE SOCKS MEDICAL</h1>
            <p style=""margin: 6px 0 0; font-size: 13px; opacity: 0.9; letter-spacing: 0.5px;"">KHÔI PHỤC MẬT KHẨU TÀI KHOẢN AN TOÀN</p>
        </div>

        <!-- Content -->
        <div style=""padding: 32px 28px;"">
            <h3 style=""color: #0f172a; margin-top: 0; font-size: 18px;"">Kính gửi {fullName},</h3>
            <p style=""color: #475569; font-size: 14px; line-height: 1.6; margin-bottom: 24px;"">
                Hệ thống <strong>Varicose Socks Y Khoa</strong> vừa nhận được yêu cầu cấp lại mật khẩu cho tài khoản đăng ký với email này. Vui lòng sử dụng mã OTP 6 chữ số dưới đây để đặt lại mật khẩu mới:
            </p>

            <!-- OTP Box -->
            <div style=""text-align: center; margin: 30px 0;"">
                <div style=""display: inline-block; background: #eff6ff; border: 2px dashed #0284c7; border-radius: 12px; padding: 18px 40px;"">
                    <span style=""font-size: 38px; font-weight: 800; letter-spacing: 8px; color: #0369a1; font-family: monospace;"">{otpCode}</span>
                </div>
                <div style=""margin-top: 10px; color: #dc2626; font-size: 12px; font-weight: 600;"">
                    ⏱️ Mã OTP có hiệu lực trong vòng 5 phút (Tuyệt đối không chia sẻ mã này cho bất kỳ ai)
                </div>
            </div>

            <p style=""color: #64748b; font-size: 13px; line-height: 1.5;"">
                Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email hoặc đổi mật khẩu tài khoản ngay lập tức để bảo vệ thông tin hồ sơ y tế.
            </p>
        </div>

        <!-- Footer -->
        <div style=""background-color: #f8fafc; padding: 20px 24px; text-align: center; border-top: 1px solid #e2e8f0; font-size: 12px; color: #94a3b8; line-height: 1.5;"">
            Hotline bác sĩ tư vấn: <strong style=""color: #0284c7;"">1900 6868</strong> | Trụ sở y tế: 123 Hai Bà Trưng, Quận 1, TP.HCM<br />
            Hệ thống phân phối vớ y khoa chuyên biệt Sejoy MedTech Clinical.
        </div>
    </div>
</body>
</html>"
            };

            mailMessage.To.Add(toEmail.Trim());
            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("Đã gửi email OTP khôi phục mật khẩu thành công tới: {Email}", toEmail);
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi gửi email OTP khôi phục mật khẩu tới {Email}", toEmail);
            return (false, ex.Message);
        }
    }
}
