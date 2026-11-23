using System.ComponentModel.DataAnnotations;
using VaricoseSocks.Web.Services;

namespace VaricoseSocks.Web.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập địa chỉ Email.")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; } = true;
    public string? ReturnUrl { get; set; }
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập họ và tên của bạn.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên từ 2 đến 100 ký tự.")]
    [Display(Name = "Họ và tên")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập địa chỉ Email.")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
    [Display(Name = "Địa chỉ Email")]
    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }
    public string? Address { get; set; }

    [Required(ErrorMessage = "Vui lòng tạo mật khẩu.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
    [Display(Name = "Xác nhận mật khẩu")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Display(Name = "Mã xác thực OTP")]
    public string? OtpCode { get; set; }

    public string? ReturnUrl { get; set; }
    public string? Step { get; set; } = "form"; // "form" hoặc "otp"
}

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập địa chỉ Email đã đăng ký.")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
    [Display(Name = "Địa chỉ Email")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Mã xác thực OTP")]
    public string? OtpCode { get; set; }

    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu mới phải có tối thiểu 6 ký tự.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu mới")]
    public string? NewPassword { get; set; }

    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
    [Display(Name = "Xác nhận mật khẩu mới")]
    public string? ConfirmNewPassword { get; set; }

    public string Step { get; set; } = "email"; // "email" hoặc "otp"
}

public class SendOtpRequestModel
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}

public class UpdateProfileViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
    [Display(Name = "Họ và tên")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại liên hệ")]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    [Display(Name = "Số điện thoại")]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập địa chỉ nhận hàng")]
    [Display(Name = "Địa chỉ nhận hàng (Số nhà, tên đường)")]
    public string? Address { get; set; }

    [Display(Name = "Tỉnh / Thành phố")]
    public string? City { get; set; } = "Hồ Chí Minh";

    [Display(Name = "Quận / Huyện")]
    public string? District { get; set; }
}

public class AuthResponseViewModel
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string? Message { get; set; }
    public UserAccountViewModel? User { get; set; }
}

public class UserAccountViewModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public List<string> Roles { get; set; } = new();
}

public class UserProfileViewModel
{
    public UserAccountViewModel User { get; set; } = new();
    public List<OrderDetailViewModel> Orders { get; set; } = new();
    public List<HealthRecordViewModel> HealthRecords { get; set; } = new();
}
