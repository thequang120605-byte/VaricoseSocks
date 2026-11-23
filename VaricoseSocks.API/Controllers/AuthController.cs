using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VaricoseSocks.Application.DTOs;
using VaricoseSocks.Application.Interfaces;
using VaricoseSocks.Domain.Entities;

namespace VaricoseSocks.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepo;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IJwtService _jwtService;

    public AuthController(
        IUserRepository userRepo,
        IPasswordHasher<User> passwordHasher,
        IJwtService jwtService)
    {
        _userRepo = userRepo;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest(new AuthResponseDto { Success = false, Message = "Vui lòng nhập đầy đủ email và mật khẩu." });
        }

        var user = await _userRepo.GetByEmailAsync(dto.Email);
        if (user == null)
        {
            return Unauthorized(new AuthResponseDto { Success = false, Message = "Email hoặc mật khẩu không chính xác." });
        }

        var verifyResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
        if (verifyResult == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new AuthResponseDto { Success = false, Message = "Email hoặc mật khẩu không chính xác." });
        }

        var roles = (await _userRepo.GetUserRolesAsync(user.Id)).ToList();
        var token = _jwtService.GenerateToken(user, roles);

        return Ok(new AuthResponseDto
        {
            Success = true,
            Token = token,
            Message = "Đăng nhập thành công.",
            User = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                Roles = roles
            }
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest(new AuthResponseDto { Success = false, Message = "Email và mật khẩu không được để trống." });
        }

        if (await _userRepo.CheckEmailExistsAsync(dto.Email))
        {
            return BadRequest(new AuthResponseDto { Success = false, Message = "Email này đã được đăng ký trong hệ thống." });
        }

        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address,
            IsActive = true
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

        var created = await _userRepo.CreateUserAsync(user, "Customer");
        var roles = new List<string> { "Customer" };
        var token = _jwtService.GenerateToken(created, roles);

        return Ok(new AuthResponseDto
        {
            Success = true,
            Token = token,
            Message = "Đăng ký tài khoản thành công.",
            User = new UserDto
            {
                Id = created.Id,
                FullName = created.FullName,
                Email = created.Email,
                Phone = created.Phone,
                Address = created.Address,
                Roles = roles
            }
        });
    }

    [HttpGet("check-email")]
    public async Task<IActionResult> CheckEmail([FromQuery] string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(new { message = "Email không được để trống." });
        }
        var exists = await _userRepo.CheckEmailExistsAsync(email);
        return Ok(new { exists });
    }

    [HttpGet("user/{id:int}")]
    public async Task<IActionResult> GetProfile(int id)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user == null) return NotFound(new { message = "Không tìm thấy người dùng." });

        var roles = await _userRepo.GetUserRolesAsync(user.Id);

        return Ok(new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            Address = user.Address,
            City = user.City,
            District = user.District,
            Roles = roles.ToList()
        });
    }

    [HttpPut("user/{id:int}")]
    public async Task<IActionResult> UpdateProfile(int id, [FromBody] UpdateProfileDto dto)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user == null) return NotFound(new { success = false, message = "Không tìm thấy người dùng." });

        if (!string.IsNullOrWhiteSpace(dto.FullName))
            user.FullName = dto.FullName.Trim();

        user.Phone = dto.Phone?.Trim();
        user.Address = dto.Address?.Trim();
        if (!string.IsNullOrWhiteSpace(dto.City)) user.City = dto.City.Trim();
        if (!string.IsNullOrWhiteSpace(dto.District)) user.District = dto.District.Trim();

        var updated = await _userRepo.UpdateUserAsync(user);
        if (!updated)
        {
            return StatusCode(500, new { success = false, message = "Không thể cập nhật hồ sơ vào cơ sở dữ liệu." });
        }

        var roles = await _userRepo.GetUserRolesAsync(user.Id);
        return Ok(new
        {
            success = true,
            message = "Cập nhật thông tin hồ sơ thành công.",
            user = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                City = user.City,
                District = user.District,
                Roles = roles.ToList()
            }
        });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.NewPassword))
        {
            return BadRequest(new { success = false, message = "Email và mật khẩu mới không được để trống." });
        }

        var user = await _userRepo.GetByEmailAsync(dto.Email.Trim().ToLower());
        if (user == null)
        {
            return NotFound(new { success = false, message = "Không tìm thấy tài khoản với địa chỉ email này." });
        }

        user.PasswordHash = _passwordHasher.HashPassword(user, dto.NewPassword);
        var updated = await _userRepo.UpdateUserAsync(user);
        if (!updated)
        {
            return StatusCode(500, new { success = false, message = "Không thể cập nhật mật khẩu vào cơ sở dữ liệu." });
        }

        return Ok(new { success = true, message = "Đặt lại mật khẩu thành công." });
    }
}
