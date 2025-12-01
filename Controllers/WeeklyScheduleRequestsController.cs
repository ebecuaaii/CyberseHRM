using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using HRMCyberse.DTOs;
using HRMCyberse.Data;
using HRMCyberse.Models;

namespace HRMCyberse.Controllers
{
    /// <summary>
    /// Controller for managing weekly schedule requests
    /// Employees register their availability for the week, Admin/Manager reviews and schedules
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WeeklyScheduleRequestsController : ControllerBase
    {
        private readonly CybersehrmContext _context;
        private readonly ILogger<WeeklyScheduleRequestsController> _logger;

        public WeeklyScheduleRequestsController(CybersehrmContext context, ILogger<WeeklyScheduleRequestsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get current user's shift registrations in date range
        /// Returns simple registrations (shiftId + requestedDate)
        /// </summary>
        [HttpGet("my-registrations")]
        public async Task<ActionResult> GetMyRegistrations(
            [FromQuery] DateOnly? fromDate = null,
            [FromQuery] DateOnly? toDate = null)
        {
            try
            {
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest("Không thể xác định người dùng");
                }

                var query = _context.Shiftregistrations
                    .Include(r => r.User)
                    .Include(r => r.Shift)
                    .Where(r => r.Userid == userId && r.Registrationdate.HasValue);

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
                    .OrderBy(r => r.Registrationdate)
                    .ThenBy(r => r.Shift.Starttime)
                    .ToListAsync();

                var result = registrations.Select(r => new
                {
                    id = r.Id,
                    userId = r.Userid,
                    userName = r.User?.Username,
                    fullName = r.User?.Fullname,
                    shiftId = r.Shiftid,
                    shiftName = r.Shift?.Name,
                    shiftStartTime = r.Shift?.Starttime,
                    shiftEndTime = r.Shift?.Endtime,
                    requestedDate = DateOnly.FromDateTime(r.Registrationdate.Value),
                    status = r.Status?.ToLower(),
                    createdAt = r.Registrationdate
                }).ToList();

                _logger.LogInformation("Retrieved {Count} registrations for user {UserId}", result.Count, userId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user registrations");
                return StatusCode(500, "Lỗi server khi lấy danh sách đăng ký");
            }
        }

        /// <summary>
        /// Get all registrations for a specific shift and date
        /// </summary>
        [HttpGet("by-shift")]
        public async Task<ActionResult> GetRegistrationsByShift(
            [FromQuery] int shiftId,
            [FromQuery] DateOnly date)
        {
            try
            {
                var registrations = await _context.Shiftregistrations
                    .Include(r => r.User)
                    .Include(r => r.Shift)
                    .Where(r => r.Shiftid == shiftId 
                        && r.Registrationdate.HasValue
                        && DateOnly.FromDateTime(r.Registrationdate.Value) == date)
                    .OrderBy(r => r.User.Fullname)
                    .Select(r => new
                    {
                        id = r.Id,
                        userId = r.Userid,
                        userName = r.User.Username,
                        fullName = r.User.Fullname,
                        shiftId = r.Shiftid,
                        shiftName = r.Shift.Name,
                        requestedDate = DateOnly.FromDateTime(r.Registrationdate.Value),
                        status = r.Status,
                        createdAt = r.Registrationdate
                    })
                    .ToListAsync();

                return Ok(registrations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting registrations by shift");
                return StatusCode(500, "Lỗi server");
            }
        }

        /// <summary>
        /// Employee creates a weekly schedule availability request
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> CreateWeeklyRequest(CreateWeeklyScheduleRequestDto dto)
        {
            try
            {
                // Get current user ID
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest("Không thể xác định người dùng");
                }

                // Check if this is a simple registration (shiftId + requestedDate)
                if (dto.ShiftId.HasValue && dto.RequestedDate.HasValue)
                {
                    return await HandleSimpleRegistration(userId, dto.ShiftId.Value, dto.RequestedDate.Value);
                }

                // Otherwise, handle as weekly schedule request
                if (!ModelState.IsValid || !dto.WeekStartDate.HasValue || dto.Availability == null || !dto.Availability.Any())
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    _logger.LogWarning("Invalid model state: {Errors}", string.Join(", ", errors));
                    return BadRequest(new { 
                        message = "Dữ liệu không hợp lệ", 
                        errors = errors
                    });
                }

                // Calculate week end date (6 days after start)
                var weekEndDate = dto.WeekStartDate.Value.AddDays(6);

                // Check for duplicate shift registrations in the same week
                var existingRequest = await _context.WeeklyscheduleRequests
                    .FirstOrDefaultAsync(r => r.Userid == userId 
                        && r.WeekStartDate == dto.WeekStartDate 
                        && r.Status == "pending");

                // Convert availability to JSON
                var availabilityDict = new Dictionary<string, List<int>>();
                foreach (var day in dto.Availability)
                {
                    var dayName = day.DayOfWeek.ToString().ToLower();
                    availabilityDict[dayName] = day.ShiftIds;
                }
                var availabilityJson = JsonSerializer.Serialize(availabilityDict);

                if (existingRequest != null)
                {
                    // Update existing pending request instead of creating new one
                    existingRequest.AvailabilityData = availabilityJson;
                    existingRequest.Note = dto.Note;
                    existingRequest.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();

                    // Load user info
                    await _context.Entry(existingRequest)
                        .Reference(r => r.User)
                        .LoadAsync();

                    var updatedResponse = MapToDto(existingRequest);

                    _logger.LogInformation("User {UserId} updated weekly schedule request for week {WeekStart}", 
                        userId, dto.WeekStartDate);

                    return Ok(updatedResponse);
                }

                // Create new request

                // Create request
                var request = new WeeklyscheduleRequest
                {
                    Userid = userId,
                    WeekStartDate = dto.WeekStartDate.Value,
                    WeekEndDate = weekEndDate,
                    Status = "pending",
                    AvailabilityData = availabilityJson,
                    Note = dto.Note,
                    CreatedAt = DateTime.UtcNow
                };

                _context.WeeklyscheduleRequests.Add(request);
                await _context.SaveChangesAsync();

                // Load user info
                await _context.Entry(request)
                    .Reference(r => r.User)
                    .LoadAsync();

                var response = MapToDto(request);

                _logger.LogInformation("User {UserId} created weekly schedule request for week {WeekStart}", 
                    userId, dto.WeekStartDate);

                return CreatedAtAction(nameof(GetRequestById), new { id = request.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating weekly schedule request");
                return StatusCode(500, "Lỗi server khi tạo yêu cầu đăng ký lịch");
            }
        }

        /// <summary>
        /// Get all pending weekly requests (Admin/Manager only)
        /// </summary>
        [HttpGet("pending")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<List<WeeklyScheduleRequestDto>>> GetPendingRequests()
        {
            try
            {
                var requests = await _context.WeeklyscheduleRequests
                    .Include(r => r.User)
                    .Where(r => r.Status == "pending")
                    .OrderBy(r => r.WeekStartDate)
                    .ThenBy(r => r.User.Fullname)
                    .ToListAsync();

                var response = requests.Select(MapToDto).ToList();

                _logger.LogInformation("Retrieved {Count} pending weekly requests", response.Count);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending weekly requests");
                return StatusCode(500, "Lỗi server khi lấy danh sách yêu cầu");
            }
        }

        /// <summary>
        /// Get all shift registrations (Admin/Manager see all, Employee see own)
        /// Returns simple registrations with shiftId and requestedDate
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> GetAllRequests(
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
                    .Where(r => r.Registrationdate.HasValue)
                    .AsQueryable();

                // Employee only sees their own registrations
                if (userRole == "Employee")
                {
                    query = query.Where(r => r.Userid == userId);
                }

                // Filter by status if provided
                if (!string.IsNullOrEmpty(status))
                {
                    var capitalizedStatus = char.ToUpper(status[0]) + status.Substring(1).ToLower();
                    query = query.Where(r => r.Status == capitalizedStatus);
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

                var response = registrations.Select(r => new
                {
                    id = r.Id,
                    userId = r.Userid,
                    userName = r.User?.Username,
                    fullName = r.User?.Fullname,
                    shiftId = r.Shiftid,
                    shiftName = r.Shift?.Name,
                    shiftStartTime = r.Shift?.Starttime,
                    shiftEndTime = r.Shift?.Endtime,
                    requestedDate = DateOnly.FromDateTime(r.Registrationdate.Value),
                    status = r.Status?.ToLower(),
                    approvedBy = r.Approvedby,
                    approvedByName = r.ApprovedbyNavigation?.Fullname,
                    createdAt = r.Registrationdate
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving registrations");
                return StatusCode(500, "Lỗi server khi lấy danh sách đăng ký");
            }
        }

        /// <summary>
        /// Get request by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<WeeklyScheduleRequestDto>> GetRequestById(int id)
        {
            try
            {
                var request = await _context.WeeklyscheduleRequests
                    .Include(r => r.User)
                    .Include(r => r.ReviewedByNavigation)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (request == null)
                {
                    return NotFound("Không tìm thấy yêu cầu");
                }

                // Check permission
                var userIdClaim = User.FindFirst("UserId")?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (userRole == "Employee" && request.Userid.ToString() != userIdClaim)
                {
                    return Forbid();
                }

                return Ok(MapToDto(request));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving request {Id}", id);
                return StatusCode(500, "Lỗi server khi lấy thông tin yêu cầu");
            }
        }

        /// <summary>
        /// Admin/Manager marks request as reviewed or scheduled
        /// </summary>
        [HttpPost("review")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<WeeklyScheduleRequestDto>> ReviewRequest(ReviewWeeklyScheduleRequestDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var request = await _context.WeeklyscheduleRequests
                    .Include(r => r.User)
                    .FirstOrDefaultAsync(r => r.Id == dto.RequestId);

                if (request == null)
                {
                    return NotFound("Không tìm thấy yêu cầu");
                }

                // Get reviewer ID
                var reviewerIdClaim = User.FindFirst("UserId")?.Value;
                if (!int.TryParse(reviewerIdClaim, out int reviewerId))
                {
                    return BadRequest("Không thể xác định người duyệt");
                }

                // Update request status
                request.Status = dto.Status;
                request.ReviewedBy = reviewerId;
                request.ReviewedAt = DateTime.UtcNow;
                request.UpdatedAt = DateTime.UtcNow;

                if (!string.IsNullOrEmpty(dto.Note))
                {
                    request.Note = dto.Note;
                }

                await _context.SaveChangesAsync();

                // Reload with navigation properties
                await _context.Entry(request)
                    .Reference(r => r.ReviewedByNavigation)
                    .LoadAsync();

                var response = MapToDto(request);

                _logger.LogInformation("Weekly request {RequestId} marked as {Status} by user {ReviewerId}", 
                    request.Id, dto.Status, reviewerId);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reviewing weekly request");
                return StatusCode(500, "Lỗi server khi xử lý yêu cầu");
            }
        }

        /// <summary>
        /// Employee updates their pending request
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<WeeklyScheduleRequestDto>> UpdateRequest(int id, CreateWeeklyScheduleRequestDto dto)
        {
            try
            {
                var request = await _context.WeeklyscheduleRequests.FindAsync(id);

                if (request == null)
                {
                    return NotFound("Không tìm thấy yêu cầu");
                }

                // Check permission
                var userIdClaim = User.FindFirst("UserId")?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (userRole == "Employee" && request.Userid.ToString() != userIdClaim)
                {
                    return Forbid();
                }

                if (request.Status != "pending")
                {
                    return BadRequest("Chỉ có thể sửa yêu cầu đang chờ duyệt");
                }

                // Update availability
                var availabilityDict = new Dictionary<string, List<int>>();
                foreach (var day in dto.Availability)
                {
                    var dayName = day.DayOfWeek.ToString().ToLower();
                    availabilityDict[dayName] = day.ShiftIds;
                }
                request.AvailabilityData = JsonSerializer.Serialize(availabilityDict);
                request.Note = dto.Note;
                request.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Load user info
                await _context.Entry(request)
                    .Reference(r => r.User)
                    .LoadAsync();

                var response = MapToDto(request);

                _logger.LogInformation("Weekly request {RequestId} updated", id);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating weekly request {Id}", id);
                return StatusCode(500, "Lỗi server khi cập nhật yêu cầu");
            }
        }

        /// <summary>
        /// Employee deletes their pending request
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteRequest(int id)
        {
            try
            {
                var request = await _context.WeeklyscheduleRequests.FindAsync(id);

                if (request == null)
                {
                    return NotFound("Không tìm thấy yêu cầu");
                }

                // Check permission
                var userIdClaim = User.FindFirst("UserId")?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (userRole == "Employee" && request.Userid.ToString() != userIdClaim)
                {
                    return Forbid();
                }

                if (request.Status != "pending")
                {
                    return BadRequest("Chỉ có thể xóa yêu cầu đang chờ duyệt");
                }

                _context.WeeklyscheduleRequests.Remove(request);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Weekly request {RequestId} deleted", id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting weekly request {Id}", id);
                return StatusCode(500, "Lỗi server khi xóa yêu cầu");
            }
        }

        private async Task<ActionResult> HandleSimpleRegistration(int userId, int shiftId, DateOnly requestedDate)
        {
            try
            {
                // Check if shift exists
                var shift = await _context.Shifts.FindAsync(shiftId);
                if (shift == null)
                {
                    return BadRequest("Ca làm việc không tồn tại");
                }

                // Check if already registered (any status)
                var existingReg = await _context.Shiftregistrations
                    .Include(r => r.User)
                    .Include(r => r.Shift)
                    .FirstOrDefaultAsync(r => r.Userid == userId 
                        && r.Shiftid == shiftId 
                        && r.Registrationdate.HasValue
                        && DateOnly.FromDateTime(r.Registrationdate.Value) == requestedDate
                        && r.Status == "Pending"); // Only check pending registrations

                if (existingReg != null)
                {
                    // Return existing registration
                    return Ok(new
                    {
                        id = existingReg.Id,
                        userId = existingReg.Userid,
                        userName = existingReg.User?.Username,
                        fullName = existingReg.User?.Fullname,
                        shiftId = existingReg.Shiftid,
                        shiftName = existingReg.Shift?.Name,
                        requestedDate = DateOnly.FromDateTime(existingReg.Registrationdate.Value),
                        status = existingReg.Status?.ToLower(), // Convert to lowercase for frontend
                        createdAt = existingReg.Registrationdate
                    });
                }

                // Create new registration
                var registration = new Shiftregistration
                {
                    Userid = userId,
                    Shiftid = shiftId,
                    Registrationdate = new DateTime(requestedDate.Year, requestedDate.Month, requestedDate.Day, 0, 0, 0, DateTimeKind.Utc),
                    Status = "Pending" // Must be: Pending, Approved, or Rejected (capital first letter)
                };

                _context.Shiftregistrations.Add(registration);
                await _context.SaveChangesAsync();

                // Load related data
                await _context.Entry(registration).Reference(r => r.User).LoadAsync();
                await _context.Entry(registration).Reference(r => r.Shift).LoadAsync();

                _logger.LogInformation("User {UserId} registered for shift {ShiftId} on {Date}", 
                    userId, shiftId, requestedDate);

                return Ok(new
                {
                    id = registration.Id,
                    userId = registration.Userid,
                    userName = registration.User?.Username,
                    fullName = registration.User?.Fullname,
                    shiftId = registration.Shiftid,
                    shiftName = registration.Shift?.Name,
                    requestedDate = requestedDate,
                    status = registration.Status?.ToLower(), // Convert to lowercase for frontend
                    createdAt = registration.Registrationdate
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in simple registration");
                return StatusCode(500, "Lỗi server khi đăng ký ca làm việc");
            }
        }

        private static WeeklyScheduleRequestDto MapToDto(WeeklyscheduleRequest request)
        {
            return new WeeklyScheduleRequestDto
            {
                Id = request.Id,
                UserId = request.Userid,
                UserName = request.User?.Username,
                FullName = request.User?.Fullname,
                WeekStartDate = request.WeekStartDate,
                WeekEndDate = request.WeekEndDate,
                Status = request.Status,
                AvailabilityData = request.AvailabilityData,
                Note = request.Note,
                ReviewedBy = request.ReviewedBy,
                ReviewedByName = request.ReviewedByNavigation?.Fullname,
                CreatedAt = request.CreatedAt,
                ReviewedAt = request.ReviewedAt
            };
        }
    }
}
