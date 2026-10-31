using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VaricoseSocks.Application.DTOs;
using VaricoseSocks.Application.Interfaces;

namespace VaricoseSocks.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthRecordsController : ControllerBase
{
    private readonly IHealthRecordService _healthRecordService;

    public HealthRecordsController(IHealthRecordService healthRecordService)
    {
        _healthRecordService = healthRecordService;
    }

    private int? GetCurrentUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(idClaim, out var id) ? id : null;
    }

    private bool IsAdminOrStaff()
    {
        return User.IsInRole("Admin") || User.IsInRole("Staff");
    }

    [HttpGet("ceap-assessment")]
    public IActionResult AssessCeap(
        [FromQuery] int stage = 1,
        [FromQuery] bool hasUlcer = false,
        [FromQuery] bool hasSkinChanges = false,
        [FromQuery] bool hasEdema = false,
        [FromQuery] bool hasVaricose = false)
    {
        var result = _healthRecordService.AssessCeap(stage, hasUlcer, hasSkinChanges, hasEdema, hasVaricose);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetUserRecords(int userId)
    {
        var currentUserId = GetCurrentUserId();
        if (!IsAdminOrStaff() && (!currentUserId.HasValue || currentUserId.Value != userId))
        {
            return Forbid();
        }

        var records = await _healthRecordService.GetRecordsByUserIdAsync(userId);
        return Ok(records);
    }

    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var record = await _healthRecordService.GetByIdAsync(id);
        if (record == null) return NotFound(new { message = "Không tìm thấy hồ sơ theo dõi." });

        var currentUserId = GetCurrentUserId();
        if (!IsAdminOrStaff() && (!currentUserId.HasValue || currentUserId.Value != record.UserId))
        {
            return Forbid();
        }

        return Ok(record);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateRecord([FromBody] CreateHealthRecordDto dto)
    {
        var currentUserId = GetCurrentUserId();
        var targetUserId = currentUserId ?? dto.UserId ?? 0;

        if (targetUserId == 0)
        {
            return Unauthorized(new { message = "Vui lòng đăng nhập để lưu hồ sơ theo dõi sức khỏe." });
        }

        var result = await _healthRecordService.CreateRecordAsync(targetUserId, dto);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Record!.Id }, result.Record);
    }

    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var currentUserId = GetCurrentUserId() ?? 0;
        var result = await _healthRecordService.DeleteRecordAsync(id, currentUserId, IsAdminOrStaff());

        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        return Ok(new { message = result.Message });
    }
}
