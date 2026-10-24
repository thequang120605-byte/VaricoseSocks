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

    public AccountController(IApiService apiService)
    {
        _apiService = apiService;
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

        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var authResult = await _apiService.RegisterAsync(model);
        if (authResult != null && authResult.Success && authResult.User != null)
        {
            // Auto sign in user upon successful registration
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

            TempData["SuccessMessage"] = "Đăng ký tài khoản thành công! Chào mừng bạn đến với Varicose Socks.";
            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, authResult?.Message ?? "Đăng ký không thành công. Vui lòng thử lại.");
        return View(model);
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

        var userAccount = await _apiService.GetProfileAsync(userId) ?? new UserAccountViewModel
        {
            Id = userId,
            FullName = User.Identity.Name ?? "Khách hàng",
            Email = User.FindFirst(ClaimTypes.Email)?.Value ?? "",
            Phone = User.FindFirst("Phone")?.Value ?? "",
            Address = User.FindFirst("Address")?.Value ?? "",
            Roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList()
        };

        var orders = await _apiService.GetUserOrdersAsync(userId);

        var model = new UserProfileViewModel
        {
            User = userAccount,
            Orders = orders
        };

        return View(model);
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
