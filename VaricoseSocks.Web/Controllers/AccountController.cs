using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using VaricoseSocks.Web.Models;
using VaricoseSocks.Web.Services;

namespace VaricoseSocks.Web.Controllers;

public class AccountController : Controller
{
    private readonly IApiService _apiService;
    private readonly IEmailService _emailService;

    public AccountController(IApiService apiService, IEmailService emailService)
    {
        _apiService = apiService;
        _emailService = emailService;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var authResult = await _apiService.LoginAsync(model.Email, model.Password);
        if (authResult != null && authResult.Success && authResult.User != null)
        {
            var user = authResult.User;

            // Create Claims for Cookie Authentication
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.FullName),
                new(ClaimTypes.Email, user.Email),
                new("Phone", user.Phone ?? ""),
                new("Address", user.Address ?? "")
            };

            if (!string.IsNullOrEmpty(authResult.Token))
            {
                claims.Add(new Claim("JwtToken", authResult.Token));
            }

            foreach (var role in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var authProps = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProps);

            // Store in Session for instant accessibility
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetString("UserEmail", user.Email);
            if (user.Roles.Contains("Admin"))
            {
                HttpContext.Session.SetString("IsAdmin", "true");
            }

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            if (user.Roles.Contains("Admin"))
            {
                return RedirectToAction("Index", "Admin");
            }

            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, authResult?.Message ?? "Email hoặc mật khẩu không chính xác.");
        return View(model);
    }

    [HttpGet]
    public IActionResult Register(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new RegisterViewModel { Step = "form", ReturnUrl = returnUrl });
    }

    [HttpPost]
    public async Task<IActionResult> SendRegisterOtp([FromBody] RegisterViewModel model)
    {
        if (model == null || string.IsNullOrWhiteSpace(model.FullName) || string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
        {
            return Json(new { success = false, message = "Vui lòng điền đầy đủ họ tên, địa chỉ Gmail và mật khẩu." });
        }

        if (model.Password != model.ConfirmPassword)
        {
            return Json(new { success = false, message = "Mật khẩu xác nhận không khớp. Vui lòng kiểm tra lại." });
        }

        if (model.Password.Length < 6)
        {
            return Json(new { success = false, message = "Mật khẩu phải có tối thiểu 6 ký tự để bảo mật thông tin." });
        }

        // Kiểm tra xem Email đã tồn tại trong MySQL Database hay chưa
        var emailExists = await _apiService.CheckEmailExistsAsync(model.Email.Trim().ToLower());
        if (emailExists)
        {
            return Json(new { success = false, message = "Địa chỉ Gmail này đã được đăng ký tài khoản trong hệ thống. Bạn có thể bấm Đăng nhập hoặc Quên mật khẩu." });
        }

        // Tạo mã OTP 6 chữ số ngẫu nhiên
        var otp = Random.Shared.Next(100000, 999999).ToString();

        // Lưu thông tin đăng ký tạm và mã OTP vào Session
        HttpContext.Session.SetString("Register_Otp", otp);
        HttpContext.Session.SetString("Register_Otp_Email", model.Email.Trim().ToLower());
        HttpContext.Session.SetString("Register_FullName", model.FullName.Trim());
        HttpContext.Session.SetString("Register_Password", model.Password);
        HttpContext.Session.SetString("Register_Otp_Time", DateTime.UtcNow.ToString("o"));
        HttpContext.Session.SetString("Register_ReturnUrl", model.ReturnUrl ?? "");

        // Gửi trực tiếp email OTP qua Google SMTP Service
        var emailResult = await _emailService.SendOtpEmailAsync(model.Email.Trim().ToLower(), model.FullName.Trim(), otp);

        if (!emailResult.success)
        {
            return Json(new { success = false, message = $"Không thể gửi email qua Google SMTP: {emailResult.error}. Vui lòng thử lại." });
        }

        return Json(new { success = true, email = model.Email.Trim().ToLower(), message = $"Đã gửi thành công mã xác thực OTP trực tiếp vào hòm thư Gmail: {model.Email}." });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        // GIAI ĐOẠN 1: Người dùng nhập thông tin rút gọn (FullName, Email, Password, ConfirmPassword)
        if (model.Step != "otp")
        {
            if (string.IsNullOrWhiteSpace(model.FullName) || string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError(string.Empty, "Vui lòng điền đầy đủ họ tên, email và mật khẩu.");
                model.Step = "form";
                return View(model);
            }

            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Mật khẩu xác nhận không khớp.");
                model.Step = "form";
                return View(model);
            }

            if (model.Password.Length < 6)
            {
                ModelState.AddModelError("Password", "Mật khẩu phải có tối thiểu 6 ký tự.");
                model.Step = "form";
                return View(model);
            }

            // Kiểm tra xem Email đã tồn tại trong MySQL Database hay chưa
            var emailExists = await _apiService.CheckEmailExistsAsync(model.Email);
            if (emailExists)
            {
                ModelState.AddModelError("Email", "Địa chỉ Gmail này đã được đăng ký tài khoản trong hệ thống.");
                model.Step = "form";
                return View(model);
            }

            // Tạo mã OTP 6 chữ số ngẫu nhiên
            var otp = Random.Shared.Next(100000, 999999).ToString();

            // Lưu thông tin đăng ký tạm và mã OTP vào Session (chưa lưu vào Database)
            HttpContext.Session.SetString("Register_Otp", otp);
            HttpContext.Session.SetString("Register_Otp_Email", model.Email.Trim().ToLower());
            HttpContext.Session.SetString("Register_FullName", model.FullName.Trim());
            HttpContext.Session.SetString("Register_Password", model.Password);
            HttpContext.Session.SetString("Register_Otp_Time", DateTime.UtcNow.ToString("o"));
            HttpContext.Session.SetString("Register_ReturnUrl", model.ReturnUrl ?? "");

            // Gửi trực tiếp email OTP qua Google SMTP Service
            var emailResult = await _emailService.SendOtpEmailAsync(model.Email, model.FullName, otp);

            // Chuyển sang Bước 2: Nhập OTP
            model.Step = "otp";

            if (emailResult.success)
            {
                TempData["OtpSentMessage"] = $"Đã gửi thành công mã xác thực OTP trực tiếp vào hòm thư Gmail: {model.Email}. Bạn vui lòng mở Hộp thư đến (hoặc Thư rác/Spam) trên Gmail để lấy mã xác thực.";
            }
            else
            {
                TempData["OtpSentMessage"] = $"Không thể gửi email qua Google SMTP: {emailResult.error}. Vui lòng thử lại.";
            }
            return View(model);
        }

        // GIAI ĐOẠN 2: Người dùng nhập mã OTP để xác nhận và lưu vào Database
        var sessionOtp = HttpContext.Session.GetString("Register_Otp");
        var sessionEmail = HttpContext.Session.GetString("Register_Otp_Email");
        var sessionName = HttpContext.Session.GetString("Register_FullName") ?? model.FullName;
        var sessionPassword = HttpContext.Session.GetString("Register_Password") ?? model.Password;

        if (string.IsNullOrWhiteSpace(model.OtpCode) || string.IsNullOrWhiteSpace(sessionOtp))
        {
            ModelState.AddModelError("OtpCode", "Vui lòng nhập mã OTP 6 chữ số.");
            model.Step = "otp";
            return View(model);
        }

        if (model.OtpCode.Trim() != sessionOtp)
        {
            ModelState.AddModelError("OtpCode", "Mã xác thực OTP không chính xác. Vui lòng kiểm tra lại Gmail.");
            model.Step = "otp";
            return View(model);
        }

        // Mã OTP hợp lệ -> TIẾN HÀNH LƯU TÀI KHOẢN VÀO CƠ SỞ DỮ LIỆU MYSQL
        var registerPayload = new RegisterViewModel
        {
            FullName = sessionName,
            Email = sessionEmail ?? model.Email,
            Password = sessionPassword
        };

        var authResult = await _apiService.RegisterAsync(registerPayload);
        if (authResult != null && authResult.Success && authResult.User != null)
        {
            // Xóa OTP khỏi Session
            HttpContext.Session.Remove("Register_Otp");
            HttpContext.Session.Remove("Register_Otp_Email");
            HttpContext.Session.Remove("Register_FullName");
            HttpContext.Session.Remove("Register_Password");

            var user = authResult.User;
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.FullName),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, "Customer"),
                new("Phone", user.Phone ?? ""),
                new("Address", user.Address ?? "")
            };

            if (!string.IsNullOrEmpty(authResult.Token))
            {
                claims.Add(new Claim("JwtToken", authResult.Token));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetString("UserEmail", user.Email);

            TempData["SuccessMessage"] = "Xác thực OTP thành công! Tài khoản của bạn đã được kích hoạt.";
            var targetReturnUrl = model.ReturnUrl ?? HttpContext.Session.GetString("Register_ReturnUrl");
            if (!string.IsNullOrEmpty(targetReturnUrl) && Url.IsLocalUrl(targetReturnUrl))
            {
                return Redirect(targetReturnUrl);
            }
            return RedirectToAction("Profile", "Account");
        }

        ModelState.AddModelError(string.Empty, authResult?.Message ?? "Đăng ký không thành công. Vui lòng thử lại.");
        model.Step = "otp";
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ResendOtp()
    {
        var sessionEmail = HttpContext.Session.GetString("Register_Otp_Email");
        var sessionName = HttpContext.Session.GetString("Register_FullName") ?? "Quý khách";
        if (string.IsNullOrEmpty(sessionEmail))
        {
            return Json(new { success = false, message = "Phiên đăng ký đã hết hạn. Vui lòng quay lại nhập lại thông tin." });
        }

        var newOtp = Random.Shared.Next(100000, 999999).ToString();
        HttpContext.Session.SetString("Register_Otp", newOtp);
        HttpContext.Session.SetString("Register_Otp_Time", DateTime.UtcNow.ToString("o"));

        var emailResult = await _emailService.SendOtpEmailAsync(sessionEmail, sessionName, newOtp);
        var msg = emailResult.success
            ? $"Đã gửi lại email OTP thành công vào hòm thư Gmail: {sessionEmail}!"
            : $"Đã cấp lại mã OTP mới: {newOtp} cho {sessionEmail}";

        return Json(new
        {
            success = true,
            otp = newOtp,
            emailSent = emailResult.success,
            message = msg
        });
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View(new ForgotPasswordViewModel { Step = "email" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (model.Step != "otp")
        {
            // BƯỚC 1: Nhập email đã đăng ký
            if (string.IsNullOrWhiteSpace(model.Email))
            {
                ModelState.AddModelError("Email", "Vui lòng nhập địa chỉ Email đã đăng ký.");
                return View(model);
            }

            var emailClean = model.Email.Trim().ToLower();

            // Kiểm tra xem email có tồn tại trong hệ thống MySQL không
            var exists = await _apiService.CheckEmailExistsAsync(emailClean);
            if (!exists)
            {
                ModelState.AddModelError("Email", "Địa chỉ email này chưa được đăng ký trong hệ thống. Vui lòng kiểm tra lại.");
                return View(model);
            }

            // Tạo mã OTP 6 chữ số
            var otp = Random.Shared.Next(100000, 999999).ToString();
            HttpContext.Session.SetString("Forgot_Otp", otp);
            HttpContext.Session.SetString("Forgot_Email", emailClean);
            HttpContext.Session.SetString("Forgot_Time", DateTime.UtcNow.ToString("o"));

            // Gửi OTP trực tiếp qua Google SMTP
            var emailResult = await _emailService.SendForgotPasswordOtpAsync(emailClean, "Quý khách", otp);

            model.Step = "otp";
            model.Email = emailClean;

            if (emailResult.success)
            {
                TempData["OtpSentMessage"] = $"Mã xác thực OTP đã được gửi đến email: {emailClean}. Bạn vui lòng kiểm tra Hộp thư đến (hoặc Thư rác/Spam).";
            }
            else
            {
                TempData["OtpSentMessage"] = $"Hệ thống đã gửi yêu cầu cấp mã OTP cho email {emailClean}. Vui lòng kiểm tra email của bạn.";
            }

            return View(model);
        }
        else
        {
            // BƯỚC 2: Xác thực OTP và đặt mật khẩu mới
            var sessionOtp = HttpContext.Session.GetString("Forgot_Otp");
            var sessionEmail = HttpContext.Session.GetString("Forgot_Email");

            if (string.IsNullOrEmpty(sessionEmail) || string.IsNullOrEmpty(sessionOtp))
            {
                ModelState.AddModelError(string.Empty, "Phiên khôi phục mật khẩu đã hết hạn. Vui lòng thực hiện lại từ đầu.");
                model.Step = "email";
                return View(model);
            }

            model.Email = sessionEmail;

            if (string.IsNullOrWhiteSpace(model.OtpCode))
            {
                ModelState.AddModelError("OtpCode", "Vui lòng nhập mã OTP 6 chữ số.");
                return View(model);
            }

            if (model.OtpCode.Trim() != sessionOtp)
            {
                ModelState.AddModelError("OtpCode", "Mã xác thực OTP không chính xác. Vui lòng kiểm tra lại Gmail.");
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(model.NewPassword) || model.NewPassword.Length < 6)
            {
                ModelState.AddModelError("NewPassword", "Mật khẩu mới phải có tối thiểu 6 ký tự.");
                return View(model);
            }

            if (model.NewPassword != model.ConfirmNewPassword)
            {
                ModelState.AddModelError("ConfirmNewPassword", "Mật khẩu xác nhận không trùng khớp.");
                return View(model);
            }

            // Gọi API đổi mật khẩu trong MySQL Database
            var success = await _apiService.ResetPasswordAsync(sessionEmail, model.NewPassword);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi cập nhật mật khẩu vào hệ thống. Vui lòng thử lại sau.");
                return View(model);
            }

            // Xóa session
            HttpContext.Session.Remove("Forgot_Otp");
            HttpContext.Session.Remove("Forgot_Email");
            HttpContext.Session.Remove("Forgot_Time");

            TempData["SuccessMessage"] = "Đặt lại mật khẩu thành công! Vui lòng đăng nhập với mật khẩu mới.";
            return RedirectToAction("Login");
        }
    }

    [HttpPost]
    public async Task<IActionResult> ResendForgotOtp()
    {
        var sessionEmail = HttpContext.Session.GetString("Forgot_Email");
        if (string.IsNullOrEmpty(sessionEmail))
        {
            return Json(new { success = false, message = "Phiên khôi phục đã hết hạn. Vui lòng nhập lại email." });
        }

        var newOtp = Random.Shared.Next(100000, 999999).ToString();
        HttpContext.Session.SetString("Forgot_Otp", newOtp);
        HttpContext.Session.SetString("Forgot_Time", DateTime.UtcNow.ToString("o"));

        var emailResult = await _emailService.SendForgotPasswordOtpAsync(sessionEmail, "Quý khách", newOtp);
        var msg = emailResult.success
            ? $"Đã gửi lại mã OTP thành công vào hòm thư Gmail: {sessionEmail}!"
            : $"Đã cấp lại mã OTP mới: {newOtp} cho {sessionEmail}";

        return Json(new
        {
            success = true,
            otp = newOtp,
            emailSent = emailResult.success,
            message = msg
        });
    }

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return RedirectToAction("Login", new { returnUrl = "/Account/Profile" });
        }

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return RedirectToAction("Login");
        }

        // GỬI HTTP GET ĐẾN BACKEND API LẤY TRỰC TIẾP TỪ MYSQL DATABASE
        var apiEndpoint = $"http://localhost:5046/api/auth/user/{userId}";
        var userAccount = await _apiService.GetProfileAsync(userId);

        ViewBag.SourceApiUrl = apiEndpoint;
        ViewBag.DatabaseSource = $"MySQL Server 8.0 (Bảng: Users, Khóa chính Id = {userId})";
        ViewBag.FetchTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        ViewBag.IsDirectDatabaseFetch = userAccount != null;

        if (userAccount == null)
        {
            userAccount = new UserAccountViewModel
            {
                Id = userId,
                FullName = User.Identity.Name ?? "Khách hàng",
                Email = User.FindFirst(ClaimTypes.Email)?.Value ?? "",
                Phone = User.FindFirst("Phone")?.Value ?? "",
                Address = User.FindFirst("Address")?.Value ?? "",
                Roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList()
            };
        }

        var orders = await _apiService.GetUserOrdersAsync(userId);
        var healthRecords = await _apiService.GetUserHealthRecordsAsync(userId);

        var model = new UserProfileViewModel
        {
            User = userAccount,
            Orders = orders,
            HealthRecords = healthRecords
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(UpdateProfileViewModel model)
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return RedirectToAction("Login");
        }

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return RedirectToAction("Login");
        }

        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Vui lòng nhập đầy đủ họ tên, số điện thoại và địa chỉ nhận hàng hợp lệ.";
            return RedirectToAction(nameof(Profile));
        }

        var success = await _apiService.UpdateProfileAsync(userId, model);
        if (success)
        {
            TempData["SuccessMessage"] = "Cập nhật thông tin liên hệ và địa chỉ nhận hàng vào Database thành công!";
        }
        else
        {
            TempData["ErrorMessage"] = "Không thể cập nhật hồ sơ vào cơ sở dữ liệu. Vui lòng thử lại.";
        }

        return RedirectToAction(nameof(Profile));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveHealthRecord(CreateHealthRecordViewModel model)
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return RedirectToAction("Login");
        }

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return RedirectToAction("Login");
        }

        model.UserId = userId;
        var success = await _apiService.SaveHealthRecordAsync(model);

        if (success)
        {
            TempData["SuccessMessage"] = "Đã lưu thông số đo và hồ sơ theo dõi tĩnh mạch thành công!";
        }
        else
        {
            TempData["ErrorMessage"] = "Có lỗi xảy ra khi lưu hồ sơ. Vui lòng kiểm tra lại số đo.";
        }

        return RedirectToAction(nameof(Profile));
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
