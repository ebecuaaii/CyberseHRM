using HRMCyberse.DTOs;
using HRMCyberse.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMCyberse.Controllers
{
    /// <summary>
    /// Attendance Management - Check-in/Check-out with GPS and Photos
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;

        public AttendanceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        /// <summary>
        /// Check in for attendance
        /// </summary>
        [HttpPost("check-in")]
        public async Task<ActionResult<AttendanceResponseDto>> CheckIn([FromBody] CheckInDto checkInDto)
        {
            try
            {
                var result = await _attendanceService.CheckInAsync(checkInDto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while checking in.", details = ex.Message });
            }
        }

        /// <summary>
        /// Check out from attendance
        /// </summary>
        [HttpPost("check-out")]
        public async Task<ActionResult<AttendanceResponseDto>> CheckOut([FromBody] CheckOutDto checkOutDto)
        {
            try
            {
                var result = await _attendanceService.CheckOutAsync(checkOutDto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while checking out.", details = ex.Message });
            }
        }

        /// <summary>
        /// Create manual attendance entry (Admin/Manager only)
        /// </summary>
        [HttpPost("manual")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<AttendanceResponseDto>> CreateManualAttendance([FromBody] ManualAttendanceDto manualAttendanceDto)
        {
            // Get current user ID from JWT token
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int currentUserId))
            {
                manualAttendanceDto.CreatedByManagerId = currentUserId;
            }

            try
            {
                var result = await _attendanceService.CreateManualAttendanceAsync(manualAttendanceDto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating manual attendance.", details = ex.Message });
            }
        }

        /// <summary>
        /// Get today's attendance for a user
        /// </summary>
        [HttpGet("today/{userId}")]
        public async Task<ActionResult<AttendanceResponseDto>> GetTodayAttendance(int userId)
        {
            try
            {
                var result = await _attendanceService.GetTodayAttendanceAsync(userId);
                if (result == null)
                {
                    return NotFound(new { message = "No attendance record found for today." });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving attendance.", details = ex.Message });
            }
        }

        /// <summary>
        /// Get attendance history for a user
        /// </summary>
        [HttpGet("history/{userId}")]
        public async Task<ActionResult<List<AttendanceResponseDto>>> GetUserAttendanceHistory(
            int userId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var result = await _attendanceService.GetUserAttendanceHistoryAsync(userId, startDate, endDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving attendance history.", details = ex.Message });
            }
        }

        /// <summary>
        /// Get attendance report (Admin/Manager only)
        /// </summary>
        [HttpPost("report")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<List<AttendanceReportDto>>> GetAttendanceReport([FromBody] AttendanceReportRequestDto request)
        {
            try
            {
                var result = await _attendanceService.GetAttendanceReportAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while generating the report.", details = ex.Message });
            }
        }

        /// <summary>
        /// Check if user can check in
        /// </summary>
        [HttpGet("can-check-in")]
        public async Task<ActionResult<bool>> CanCheckIn([FromQuery] int userId, [FromQuery] int shiftId)
        {
            try
            {
                var result = await _attendanceService.CanCheckInAsync(userId, shiftId);
                return Ok(new { canCheckIn = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while checking permissions.", details = ex.Message });
            }
        }

        /// <summary>
        /// Check if user can check out
        /// </summary>
        [HttpGet("can-check-out/{attendanceId}")]
        public async Task<ActionResult<bool>> CanCheckOut(int attendanceId)
        {
            try
            {
                var result = await _attendanceService.CanCheckOutAsync(attendanceId);
                return Ok(new { canCheckOut = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while checking permissions.", details = ex.Message });
            }
        }

        /// <summary>
        /// Get attendance summary for a user within date range
        /// </summary>
        [HttpGet("summary/{userId}")]
        public async Task<ActionResult<AttendanceSummary>> GetAttendanceSummary(
            int userId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                var result = await _attendanceService.GetAttendanceSummaryAsync(userId, startDate, endDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while generating summary.", details = ex.Message });
            }
        }
    }
}