using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HRMCyberse.Services;

namespace HRMCyberse.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttendancePayrollController : ControllerBase
{
    private readonly IAttendancePayrollService _service;
    private readonly ILogger<AttendancePayrollController> _logger;

    public AttendancePayrollController(
        IAttendancePayrollService service,
        ILogger<AttendancePayrollController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Tính lương cho một attendance (sau khi checkout)
    /// </summary>
    [HttpPost("calculate/{attendanceId}")]
    public async Task<ActionResult<AttendancePayrollDto>> CalculatePayroll(int attendanceId)
    {
        try
        {
            var result = await _service.CalculateAttendancePayrollAsync(attendanceId);
            if (result == null)
            {
                return BadRequest(new { message = "Không thể tính lương cho attendance này" });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Lỗi khi tính lương attendance {attendanceId}");
            return StatusCode(500, new { message = "Lỗi khi tính lương" });
        }
    }

    /// <summary>
    /// Lấy lương theo ngày của user
    /// </summary>
    [HttpGet("daily/{userId}")]
    public async Task<ActionResult<List<AttendancePayrollDto>>> GetDailyPayroll(
        int userId,
        [FromQuery] DateTime date)
    {
        try
        {
            var result = await _service.GetDailyPayrollAsync(userId, date);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Lỗi khi lấy lương ngày {date:yyyy-MM-dd} của user {userId}");
            return StatusCode(500, new { message = "Lỗi khi lấy dữ liệu" });
        }
    }

    /// <summary>
    /// Lấy tổng lương theo tháng của user
    /// </summary>
    [HttpGet("monthly/{userId}")]
    public async Task<ActionResult<MonthlyPayrollSummaryDto>> GetMonthlyPayroll(
        int userId,
        [FromQuery] int month,
        [FromQuery] int year)
    {
        try
        {
            var result = await _service.GetMonthlyPayrollSummaryAsync(userId, month, year);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Lỗi khi lấy lương tháng {month}/{year} của user {userId}");
            return StatusCode(500, new { message = "Lỗi khi lấy dữ liệu" });
        }
    }

    /// <summary>
    /// Lấy chi tiết attendance payroll
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<AttendancePayrollDto>> GetPayroll(int id)
    {
        try
        {
            var result = await _service.GetAttendancePayrollAsync(id);
            if (result == null)
            {
                return NotFound(new { message = "Không tìm thấy payroll" });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Lỗi khi lấy payroll {id}");
            return StatusCode(500, new { message = "Lỗi khi lấy dữ liệu" });
        }
    }
}
