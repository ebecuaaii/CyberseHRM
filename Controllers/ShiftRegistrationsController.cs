using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using HRMCyberse.DTOs;
using HRMCyberse.Data;
using HRMCyberse.Models;

namespace HRMCyberse.Controllers
{
    /// <summary>
    /// Controller for simple shift registration (1 shift per request)
    /// Employee registers for a specific shift on a specific date
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize] // Tạm thời bỏ để test
    public class ShiftRegistrationsController : ControllerBase
    {
        private readonly CybersehrmContext _context;
        private readonly ILogger<ShiftRegistrationsController> _logger;

        public ShiftRegistrationsController(CybersehrmContext context, ILogger<ShiftRegistrationsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Employee registers for a specific shift on a specific date
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ShiftRegistrationDto>> RegisterShift(CreateShiftRegistrationDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Get current user ID (hardcode for testing)
                var userIdClaim = User.FindFirst("UserId")?.Value;
                int userId = 1; // Hardcode admin user for testing
                if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int parsedUserId))
                {
                    userId = parsedUserId;
                }

                // Check if shift exists
                var shift = await _context.Shifts.FindAsync(dto.ShiftId);
                if (shift == null)
                {
                    return BadRequest("Ca làm việc không tồn tại");
                }

                // Check if already registered for this shift on this date
                var existingRegistration = await _context.Shiftregistrations
                    .FirstOrDefaultAsync(r => r.Userid == userId 
                        && r.Shiftid == dto.ShiftId 
                        && r.Registrationdate.HasValue
                        && DateOnly.FromDateTime(r.Registrationdate.Value) == dto.RequestedDate
                        && r.Status == "pending");

                if (existingRegistration != null)
                {
                    return BadRequest("Bạn đã đăng ký ca này cho ngày này rồi");
                }

                // Create registration
                var registration = new Shiftregistration
                {
                    Userid = userId,
                    Shiftid = dto.ShiftId,
                    Registrationdate = new DateTime(dto.RequestedDate.Year, dto.RequestedDate.Month, dto.RequestedDate.Day, 0, 0, 0, DateTimeKind.Utc),
                    Status = "pending",
                    Approvedby = null
                };

                _context.Shiftregistrations.Add(registration);
                await _context.SaveChangesAsync();

                // Load related data
                await _context.Entry(registration)
                    .Reference(r => r.User)
                    .LoadAsync();
                await _context.Entry(registration)
                    .Reference(r => r.Shift)
                    .LoadAsync();

                var response = MapToDto(registration);

                _logger.LogInformation("User {UserId} registered for shift {ShiftId} on {Date}", 
                    userId, dto.ShiftId, dto.RequestedDate);

                return CreatedAtAction(nameof(GetRegistrationById), new { id = registration.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating shift registration");
                return StatusCode(500, "Lỗi server khi đăng ký ca làm việc");
            }
        }

        /// <summary>
        /// Get all registrations (Admin/Manager see all, Employee see own)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<ShiftRegistrationDto>>> GetAllRegistrations(
            [FromQuery] string? status = null,
            [FromQuery] DateOnly? fromDate = null,
            [FromQuery] DateOnly? toDate = null)
        {
            try
            {
                var userIdClaim = User.FindFirst("UserId")?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest("Không thể xác định người dùng");
                }

                var query = _context.Shiftregistrations
                    .Include(r => r.User)
                    .Include(r => r.Shift)
                    .Include(r => r.ApprovedbyNavigation)
                    .AsQueryable();

                // Employee only sees their own registrations
                if (userRole == "Employee")
                {
                    query = query.Where(r => r.Userid == userId);
                }

                // Filter by status
                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(r => r.Status == status);
                }

                // Filter by date range
                if (fromDate.HasValue)
                {
                    var fromDateTime = fromDate.Value.ToDateTime(TimeOnly.MinValue);
                    query = query.Where(r => r.Registrationdate >= fromDateTime);
                }

                if (toDate.HasValue)
                {
                    var toDateTime = toDate.Value.ToDateTime(TimeOnly.MaxValue);
                    query = query.Where(r => r.Registrationdate <= toDateTime);
                }

                var registrations = await query
                    .OrderByDescending(r => r.Registrationdate)
                    .ThenBy(r => r.User.Fullname)
                    .ToListAsync();

                var response = registrations.Select(MapToDto).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving shift registrations");
                return StatusCode(500, "Lỗi server khi lấy danh sách đăng ký");
            }
        }

        /// <summary>
        /// Get registration by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ShiftRegistrationDto>> GetRegistrationById(int id)
        {
            try
            {
                var registration = await _context.Shiftregistrations
                    .Include(r => r.User)
                    .Include(r => r.Shift)
                    .Include(r => r.ApprovedbyNavigation)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (registration == null)
                {
                    return NotFound("Không tìm thấy đăng ký");
                }

                // Check permission
                var userIdClaim = User.FindFirst("UserId")?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (userRole == "Employee" && registration.Userid.ToString() != userIdClaim)
                {
                    return Forbid();
                }

                return Ok(MapToDto(registration));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving registration {Id}", id);
                return StatusCode(500, "Lỗi server khi lấy thông tin đăng ký");
            }
        }

        /// <summary>
        /// Admin/Manager approves or rejects registration
        /// </summary>
        [HttpPost("{id}/review")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ShiftRegistrationDto>> ReviewRegistration(int id, ReviewShiftRegistrationDto dto)
        {
            try
            {
                var registration = await _context.Shiftregistrations
                    .Include(r => r.User)
                    .Include(r => r.Shift)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (registration == null)
                {
                    return NotFound("Không tìm thấy đăng ký");
                }

                // Get reviewer ID
                var reviewerIdClaim = User.FindFirst("UserId")?.Value;
                if (!int.TryParse(reviewerIdClaim, out int reviewerId))
                {
                    return BadRequest("Không thể xác định người duyệt");
                }

                // Update status
                registration.Status = dto.Status;
                registration.Approvedby = reviewerId;

                await _context.SaveChangesAsync();

                // Reload with navigation properties
                await _context.Entry(registration)
                    .Reference(r => r.ApprovedbyNavigation)
                    .LoadAsync();

                var response = MapToDto(registration);

                _logger.LogInformation("Registration {Id} marked as {Status} by user {ReviewerId}", 
                    id, dto.Status, reviewerId);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reviewing registration {Id}", id);
                return StatusCode(500, "Lỗi server khi xử lý đăng ký");
            }
        }

        /// <summary>
        /// Employee cancels their pending registration
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> CancelRegistration(int id)
        {
            try
            {
                var registration = await _context.Shiftregistrations.FindAsync(id);

                if (registration == null)
                {
                    return NotFound("Không tìm thấy đăng ký");
                }

                // Check permission
                var userIdClaim = User.FindFirst("UserId")?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (userRole == "Employee" && registration.Userid.ToString() != userIdClaim)
                {
                    return Forbid();
                }

                if (registration.Status != "pending")
                {
                    return BadRequest("Chỉ có thể hủy đăng ký đang chờ duyệt");
                }

                _context.Shiftregistrations.Remove(registration);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Registration {Id} cancelled", id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling registration {Id}", id);
                return StatusCode(500, "Lỗi server khi hủy đăng ký");
            }
        }

        private static ShiftRegistrationDto MapToDto(Shiftregistration registration)
        {
            return new ShiftRegistrationDto
            {
                Id = registration.Id,
                UserId = registration.Userid ?? 0,
                UserName = registration.User?.Username,
                FullName = registration.User?.Fullname,
                ShiftId = registration.Shiftid ?? 0,
                ShiftName = registration.Shift?.Name,
                ShiftStartTime = registration.Shift?.Starttime,
                ShiftEndTime = registration.Shift?.Endtime,
                RequestedDate = registration.Registrationdate.HasValue 
                    ? DateOnly.FromDateTime(registration.Registrationdate.Value) 
                    : DateOnly.MinValue,
                Status = registration.Status ?? "pending",
                ApprovedBy = registration.Approvedby,
                ApprovedByName = registration.ApprovedbyNavigation?.Fullname,
                CreatedAt = registration.Registrationdate,
                ApprovedAt = null // Model không có field này
            };
        }
    }
}
