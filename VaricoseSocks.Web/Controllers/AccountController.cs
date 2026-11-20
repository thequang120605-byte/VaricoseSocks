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
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new RegisterViewModel { Step = "form" });
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

            TempData["SuccessMessage"] = "Xác thực OTP thành công! Tài khoản của bạn đã được lưu vào cơ sở dữ liệu và kích hoạt.";
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
