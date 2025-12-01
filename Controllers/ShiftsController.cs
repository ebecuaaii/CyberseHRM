using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using HRMCyberse.DTOs;
using HRMCyberse.Services;
using HRMCyberse.Attributes;
using HRMCyberse.Constants;

namespace HRMCyberse.Controllers
{
    /// <summary>
    /// Controller for managing work shifts and shift assignments in the HRM system.
    /// Provides endpoints for CRUD operations on shifts, shift assignments, and employee schedules.
    /// All endpoints require JWT authentication and role-based authorization.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Require authentication for all endpoints
    [Produces("application/json")]
    public class ShiftsController : ControllerBase
    {
        private readonly IShiftService _shiftService;
        private readonly ILogger<ShiftsController> _logger;

        public ShiftsController(IShiftService shiftService, ILogger<ShiftsController> logger)
        {
            _shiftService = shiftService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all work shifts in the system (Public endpoint for testing).
        /// </summary>
        /// <returns>A list of all work shifts with their details</returns>
        /// <response code="200">Returns the list of shifts</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("public")]
        [AllowAnonymous] // Allow access without authentication for testing
        [ProducesResponseType(typeof(IEnumerable<ShiftDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<IEnumerable<ShiftDto>>> GetAllShiftsPublic()
        {
            using var activity = new System.Diagnostics.Activity("GetAllShiftsPublic").Start();
            activity?.SetTag("operation", "get_all_shifts_public");
            
            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var shifts = await _shiftService.GetAllShiftsAsync();
                stopwatch.Stop();
                
                activity?.SetTag("shift_count", shifts.Count().ToString());
                activity?.SetTag("duration_ms", stopwatch.ElapsedMilliseconds.ToString());
                
                _logger.LogInformation("Retrieved {Count} shifts (public) in {ElapsedMs}ms", 
                    shifts.Count(), stopwatch.ElapsedMilliseconds);
                
                Response.Headers["Cache-Control"] = "public, max-age=300";
                
                return Ok(shifts);
            }
            catch (Exception ex)
            {
                activity?.SetTag("error", "true");
                _logger.LogError(ex, "Error retrieving shifts (public)");
                return StatusCode(500, "Lỗi server khi lấy danh sách ca làm việc");
            }
        }

        /// <summary>
        /// Retrieves all work shifts in the system.
        /// </summary>
        /// <returns>A list of all work shifts with their details</returns>
        /// <response code="200">Returns the list of shifts</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user doesn't have Admin or Manager role</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet]
        [Authorize] // Chỉ cần đăng nhập, không phân biệt role
        [ProducesResponseType(typeof(IEnumerable<ShiftDto>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<IEnumerable<ShiftDto>>> GetAllShifts()
        {
            using var activity = new System.Diagnostics.Activity("GetAllShifts").Start();
            activity?.SetTag("operation", "get_all_shifts");
            
            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var shifts = await _shiftService.GetAllShiftsAsync();
                stopwatch.Stop();
                
                // Add performance metrics to activity
                activity?.SetTag("shift_count", shifts.Count().ToString());
                activity?.SetTag("duration_ms", stopwatch.ElapsedMilliseconds.ToString());
                
                _logger.LogInformation("Retrieved {Count} shifts in {ElapsedMs}ms", 
                    shifts.Count(), stopwatch.ElapsedMilliseconds);
                
                // Add cache headers for client-side caching
                Response.Headers["Cache-Control"] = "public, max-age=60"; // 1 minute
                Response.Headers["ETag"] = $"\"{shifts.GetHashCode()}\"";
                
                return Ok(shifts);
            }
            catch (Exception ex)
            {
                activity?.SetTag("error", "true");
                _logger.LogError(ex, "Error retrieving shifts");
                return StatusCode(500, "Lỗi server khi lấy danh sách ca làm việc");
            }
        }

        /// <summary>
        /// Creates a new work shift.
        /// </summary>
        /// <param name="createShiftDto">The shift data to create</param>
        /// <returns>The created shift</returns>
        /// <response code="201">Returns the newly created shift</response>
        /// <response code="400">If the shift data is invalid or shift name already exists</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user doesn't have Admin or Manager role</response>
        /// <response code="500">If there was an internal server error</response>
        /// <example>
        /// POST /api/shifts
        /// {
        ///     "name": "Morning Shift",
        ///     "startTime": "08:00:00",
        ///     "endTime": "16:00:00"
        /// }
        /// </example>
        [HttpPost]
        [ShiftCreateAuthorize]
        [ProducesResponseType(typeof(ShiftDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ShiftDto>> CreateShift(CreateShiftDto createShiftDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validate shift times
                if (!await _shiftService.ValidateShiftTimesAsync(createShiftDto.StartTime, createShiftDto.EndTime))
                {
                    return BadRequest("Thời gian ca làm việc không hợp lệ");
                }

                // Check for duplicate shift name
                if (!await _shiftService.IsShiftNameUniqueAsync(createShiftDto.Name))
                {
                    return BadRequest("Tên ca làm việc đã tồn tại");
                }

                // Get current user ID from JWT token
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (!int.TryParse(userIdClaim, out int createdBy))
                {
                    return BadRequest("Không thể xác định người tạo ca làm việc");
                }

                var shift = await _shiftService.CreateShiftAsync(createShiftDto, createdBy);
                
                _logger.LogInformation("Created new shift: {ShiftName} by user {UserId}", shift.Name, createdBy);
                
                return CreatedAtAction(nameof(GetShiftById), new { id = shift.Id }, shift);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating shift: {ShiftName}", createShiftDto.Name);
                return StatusCode(500, "Lỗi server khi tạo ca làm việc");
            }
        }

        /// <summary>
        /// Retrieves a specific work shift by its ID.
        /// </summary>
        /// <param name="id">The ID of the shift to retrieve</param>
        /// <returns>The shift details</returns>
        /// <response code="200">Returns the shift details</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user doesn't have Admin or Manager role</response>
        /// <response code="404">If the shift is not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("{id}")]
        [ShiftViewAssignmentsAuthorize]
        [ProducesResponseType(typeof(ShiftDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ShiftDto>> GetShiftById(int id)
        {
            try
            {
                var shift = await _shiftService.GetShiftByIdAsync(id);
                
                if (shift == null)
                {
                    return NotFound("Không tìm thấy ca làm việc");
                }

                return Ok(shift);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving shift {ShiftId}", id);
                return StatusCode(500, "Lỗi server khi lấy thông tin ca làm việc");
            }
        }

        /// <summary>
        /// Updates an existing work shift. Only Admin users can update shifts.
        /// </summary>
        /// <param name="id">The ID of the shift to update</param>
        /// <param name="updateShiftDto">The updated shift data</param>
        /// <returns>The updated shift</returns>
        /// <response code="200">Returns the updated shift</response>
        /// <response code="400">If the shift data is invalid or shift name already exists</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user doesn't have Admin role</response>
        /// <response code="404">If the shift is not found</response>
        /// <response code="500">If there was an internal server error</response>
        /// <example>
        /// PUT /api/shifts/1
        /// {
        ///     "name": "Updated Morning Shift",
        ///     "startTime": "07:30:00",
        ///     "endTime": "15:30:00"
        /// }
        /// </example>
        [HttpPut("{id}")]
        [ShiftUpdateAuthorize]
        [ProducesResponseType(typeof(ShiftDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ShiftDto>> UpdateShift(int id, UpdateShiftDto updateShiftDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if shift exists
                var existingShift = await _shiftService.GetShiftByIdAsync(id);
                if (existingShift == null)
                {
                    return NotFound("Không tìm thấy ca làm việc");
                }

                // Validate shift times
                if (!await _shiftService.ValidateShiftTimesAsync(updateShiftDto.StartTime, updateShiftDto.EndTime))
                {
                    return BadRequest("Thời gian ca làm việc không hợp lệ");
                }

                // Check for duplicate shift name (excluding current shift)
                if (!await _shiftService.IsShiftNameUniqueAsync(updateShiftDto.Name, id))
                {
                    return BadRequest("Tên ca làm việc đã tồn tại");
                }

                // Note: When updating a shift, existing assignments will automatically use the new times
                // We could add conflict checking here if needed, but it might be too restrictive

                // Get current user ID from JWT token
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (!int.TryParse(userIdClaim, out int updatedBy))
                {
                    return BadRequest("Không thể xác định người cập nhật ca làm việc");
                }

                var updatedShift = await _shiftService.UpdateShiftAsync(id, updateShiftDto, updatedBy);
                
                _logger.LogInformation("Updated shift {ShiftId}: {ShiftName}", id, updatedShift.Name);
                
                return Ok(updatedShift);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating shift {ShiftId}", id);
                return StatusCode(500, "Lỗi server khi cập nhật ca làm việc");
            }
        }

        /// <summary>
        /// Deletes a work shift. Only Admin users can delete shifts.
        /// Cannot delete shifts that have existing assignments.
        /// </summary>
        /// <param name="id">The ID of the shift to delete</param>
        /// <returns>No content if successful</returns>
        /// <response code="204">If the shift was successfully deleted</response>
        /// <response code="400">If the shift cannot be deleted (has existing assignments)</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user doesn't have Admin role</response>
        /// <response code="404">If the shift is not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpDelete("{id}")]
        [ShiftDeleteAuthorize]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult> DeleteShift(int id)
        {
            try
            {
                // Check if shift exists
                var existingShift = await _shiftService.GetShiftByIdAsync(id);
                if (existingShift == null)
                {
                    return NotFound("Không tìm thấy ca làm việc");
                }

                // Get current user ID from JWT token
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (!int.TryParse(userIdClaim, out int deletedBy))
                {
                    return BadRequest("Không thể xác định người xóa ca làm việc");
                }

                var result = await _shiftService.DeleteShiftAsync(id, deletedBy);
                
                if (!result)
                {
                    return BadRequest("Không thể xóa ca làm việc. Ca này có thể đang được sử dụng.");
                }

                _logger.LogInformation("Deleted shift {ShiftId}: {ShiftName}", id, existingShift.Name);
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting shift {ShiftId}", id);
                return StatusCode(500, "Lỗi server khi xóa ca làm việc");
            }
        }
   
        /// <summary>
        /// Retrieves all shift assignments in the system with pagination support.
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 50, max: 200)</param>
        /// <param name="fromDate">Optional start date filter</param>
        /// <param name="toDate">Optional end date filter</param>
        /// <returns>A paginated list of all shift assignments</returns>
        /// <response code="200">Returns the list of shift assignments</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user doesn't have Admin or Manager role</response>
        /// <response code="500">If there was an internal server error</response>
        /// <summary>
        /// Debug endpoint to check raw assignment data
        /// </summary>
        [HttpGet("assignments/debug")]
        [Authorize]
        public async Task<ActionResult> GetAssignmentsDebug()
        {
            try
            {
                var assignments = await _shiftService.GetAllAssignmentsAsync();
                var totalCount = assignments.Count();
                
                var debugInfo = new
                {
                    totalAssignments = totalCount,
                    assignments = assignments.Select(a => new
                    {
                        a.Id,
                        a.UserId,
                        a.UserName,
                        a.FullName,
                        a.ShiftId,
                        a.ShiftName,
                        ShiftDate = a.ShiftDate.ToString("yyyy-MM-dd"),
                        a.Status,
                        CreatedAt = a.CreatedAt?.ToString("yyyy-MM-dd HH:mm:ss")
                    }).ToList()
                };
                
                return Ok(debugInfo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Retrieves all shift assignments as a simple list with optional date filtering
        /// </summary>
        /// <param name="fromDate">Optional start date filter (YYYY-MM-DD)</param>
        /// <param name="toDate">Optional end date filter (YYYY-MM-DD)</param>
        /// <param name="date">Optional specific date filter (YYYY-MM-DD)</param>
        /// <returns>Complete list of shift assignments</returns>
        /// <response code="200">Returns the list of shift assignments</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="500">If there was an internal server error</response>
        /// <example>
        /// GET /api/shifts/assignments/list?date=2024-12-25
        /// GET /api/shifts/assignments/list?fromDate=2024-12-01&toDate=2024-12-31
        /// </example>
        [HttpGet("assignments/list")]
        [Authorize] // Tất cả user có thể xem
        [ProducesResponseType(typeof(IEnumerable<UserShiftDto>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<IEnumerable<UserShiftDto>>> GetAssignmentsList(
            [FromQuery] DateOnly? fromDate = null,
            [FromQuery] DateOnly? toDate = null,
            [FromQuery] DateOnly? date = null)
        {
            try
            {
                var assignments = await _shiftService.GetAllAssignmentsAsync();
                var totalCount = assignments.Count();
                
                _logger.LogInformation("Total assignments before filtering: {Count}", totalCount);
                
                // Apply date filtering
                if (date.HasValue)
                {
                    // Nếu có date cụ thể, chỉ lấy assignments của ngày đó
                    assignments = assignments.Where(a => a.ShiftDate == date.Value);
                    _logger.LogInformation("Filtering by date: {Date}", date.Value.ToString("yyyy-MM-dd"));
                }
                else
                {
                    // Nếu không có date cụ thể, áp dụng fromDate và toDate
                    if (fromDate.HasValue)
                    {
                        assignments = assignments.Where(a => a.ShiftDate >= fromDate.Value);
                        _logger.LogInformation("Filtering from date: {FromDate}", fromDate.Value.ToString("yyyy-MM-dd"));
                    }
                    
                    if (toDate.HasValue)
                    {
                        assignments = assignments.Where(a => a.ShiftDate <= toDate.Value);
                        _logger.LogInformation("Filtering to date: {ToDate}", toDate.Value.ToString("yyyy-MM-dd"));
                    }
                }
                
                var result = assignments
                    .OrderBy(a => a.ShiftDate)
                    .ThenBy(a => a.ShiftStartTime)
                    .ThenBy(a => a.FullName)
                    .ToList();
                
                _logger.LogInformation("Retrieved {Count} assignments after filtering", result.Count);
                
                // Add cache headers
                Response.Headers["Cache-Control"] = "public, max-age=30"; // 30 seconds
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving assignments list");
                return StatusCode(500, "Lỗi server khi lấy danh sách phân công");
            }
        }

        [HttpGet("assignments")]
        [Authorize] // Tất cả user có thể xem
        [ProducesResponseType(typeof(IEnumerable<UserShiftDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<IEnumerable<UserShiftDto>>> GetAllAssignments(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] DateOnly? fromDate = null,
            [FromQuery] DateOnly? toDate = null)
        {
            using var activity = new System.Diagnostics.Activity("GetAllAssignments").Start();
            
            try
            {
                // Validate pagination parameters
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 200) pageSize = 50;
                
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var assignments = await _shiftService.GetAllAssignmentsAsync();
                
                // Apply date filtering if provided
                if (fromDate.HasValue)
                {
                    assignments = assignments.Where(a => a.ShiftDate >= fromDate.Value);
                }
                
                if (toDate.HasValue)
                {
                    assignments = assignments.Where(a => a.ShiftDate <= toDate.Value);
                }
                
                // Apply pagination
                var totalCount = assignments.Count();
                var pagedAssignments = assignments
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
                
                stopwatch.Stop();
                
                // Add pagination headers
                Response.Headers["X-Total-Count"] = totalCount.ToString();
                Response.Headers["X-Page"] = page.ToString();
                Response.Headers["X-Page-Size"] = pageSize.ToString();
                Response.Headers["X-Total-Pages"] = ((int)Math.Ceiling((double)totalCount / pageSize)).ToString();
                
                activity?.SetTag("total_count", totalCount.ToString());
                activity?.SetTag("page", page.ToString());
                activity?.SetTag("page_size", pageSize.ToString());
                
                _logger.LogInformation("Retrieved {Count} shift assignments (page {Page}/{TotalPages}) in {ElapsedMs}ms", 
                    pagedAssignments.Count, page, Math.Ceiling((double)totalCount / pageSize), stopwatch.ElapsedMilliseconds);
                
                return Ok(pagedAssignments);
            }
            catch (Exception ex)
            {
                activity?.SetTag("error", "true");
                _logger.LogError(ex, "Error retrieving shift assignments");
                return StatusCode(500, "Lỗi server khi lấy danh sách phân công ca làm việc");
            }
        }

        /// <summary>
        /// Assigns a work shift to an employee for a specific date.
        /// </summary>
        /// <param name="assignShiftDto">The assignment data</param>
        /// <param name="returnList">If true, returns all assignments after creating. If false, returns only the created assignment.</param>
        /// <returns>The created shift assignment or all assignments</returns>
        /// <response code="201">Returns the newly created assignment or all assignments</response>
        /// <response code="400">If the assignment data is invalid, user/shift doesn't exist, or assignment already exists</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user doesn't have Admin or Manager role</response>
        /// <response code="500">If there was an internal server error</response>
        /// <example>
        /// POST /api/shifts/assign?returnList=true
        /// {
        ///     "userId": 5,
        ///     "shiftId": 1,
        ///     "shiftDate": "2024-12-25",
        ///     "status": "assigned"
        /// }
        /// </example>
        [HttpPost("assign")]
        [ShiftAssignAuthorize]
        [ProducesResponseType(typeof(UserShiftDto), 201)]
        [ProducesResponseType(typeof(object), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<ActionResult> AssignShift(AssignShiftDto assignShiftDto, [FromQuery] bool returnList = false)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validate that user exists
                if (!await _shiftService.UserExistsAsync(assignShiftDto.UserId))
                {
                    return BadRequest("Người dùng không tồn tại");
                }

                // Validate that shift exists
                if (!await _shiftService.ShiftExistsAsync(assignShiftDto.ShiftId))
                {
                    return BadRequest("Ca làm việc không tồn tại");
                }

                // Get current user ID from JWT token
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (!int.TryParse(userIdClaim, out int assignedBy))
                {
                    return BadRequest("Không thể xác định người phân công");
                }

                var assignment = await _shiftService.AssignShiftAsync(assignShiftDto, assignedBy);
                
                _logger.LogInformation("Assigned shift {ShiftId} to user {UserId} by {AssignedBy}", 
                    assignShiftDto.ShiftId, assignShiftDto.UserId, assignedBy);

                // Nếu yêu cầu trả về list, lấy tất cả assignments
                if (returnList)
                {
                    var allAssignments = await _shiftService.GetAllAssignmentsAsync();
                    return Ok(new
                    {
                        success = true,
                        message = "Phân công ca thành công",
                        newAssignment = assignment,
                        assignments = allAssignments.ToList()
                    });
                }
                
                // Mặc định chỉ trả về assignment vừa tạo
                return CreatedAtAction(nameof(GetAssignmentById), new { id = assignment.Id }, assignment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning shift {ShiftId} to user {UserId}", 
                    assignShiftDto.ShiftId, assignShiftDto.UserId);
                return StatusCode(500, "Lỗi server khi phân công ca làm việc");
            }
        }



        /// <summary>
        /// Retrieves a specific shift assignment by its ID.
        /// </summary>
        /// <param name="id">The ID of the assignment to retrieve</param>
        /// <returns>The assignment details</returns>
        /// <response code="200">Returns the assignment details</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user doesn't have Admin or Manager role</response>
        /// <response code="404">If the assignment is not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("assignments/{id}")]
        [ShiftViewAssignmentsAuthorize]
        [ProducesResponseType(typeof(UserShiftDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<UserShiftDto>> GetAssignmentById(int id)
        {
            try
            {
                var assignments = await _shiftService.GetAllAssignmentsAsync();
                var assignment = assignments.FirstOrDefault(a => a.Id == id);
                
                if (assignment == null)
                {
                    return NotFound("Không tìm thấy phân công ca làm việc");
                }

                return Ok(assignment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving assignment {AssignmentId}", id);
                return StatusCode(500, "Lỗi server khi lấy thông tin phân công");
            }
        }

        /// <summary>
        /// Removes a shift assignment from an employee.
        /// </summary>
        /// <param name="id">The ID of the assignment to remove</param>
        /// <returns>No content if successful</returns>
        /// <response code="204">If the assignment was successfully removed</response>
        /// <response code="400">If the assignment cannot be removed</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user doesn't have Admin or Manager role</response>
        /// <response code="404">If the assignment is not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpDelete("assignments/{id}")]
        [ShiftAssignAuthorize]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult> RemoveAssignment(int id)
        {
            try
            {
                // Check if assignment exists
                var assignments = await _shiftService.GetAllAssignmentsAsync();
                var assignment = assignments.FirstOrDefault(a => a.Id == id);
                
                if (assignment == null)
                {
                    return NotFound("Không tìm thấy phân công ca làm việc");
                }

                // Get current user ID from JWT token
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (!int.TryParse(userIdClaim, out int removedBy))
                {
                    return BadRequest("Không thể xác định người xóa phân công");
                }

                var result = await _shiftService.RemoveAssignmentAsync(id, removedBy);
                
                if (!result)
                {
                    return BadRequest("Không thể xóa phân công ca làm việc");
                }

                _logger.LogInformation("Removed assignment {AssignmentId} for user {UserId}", id, assignment.UserId);
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing assignment {AssignmentId}", id);
                return StatusCode(500, "Lỗi server khi xóa phân công ca làm việc");
            }
        }      
        /// <summary>
        /// Retrieves the current user's personal shift schedule.
        /// </summary>
        /// <param name="fromDate">Optional start date filter (YYYY-MM-DD format)</param>
        /// <param name="toDate">Optional end date filter (YYYY-MM-DD format)</param>
        /// <param name="sortBy">Sort field: ShiftDate, ShiftName, ShiftStartTime, Status (default: ShiftDate)</param>
        /// <param name="ascending">Sort direction: true for ascending, false for descending (default: true)</param>
        /// <returns>The user's shift schedule</returns>
        /// <response code="200">Returns the user's shift schedule</response>
        /// <response code="400">If the query parameters are invalid</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="500">If there was an internal server error</response>
        /// <example>
        /// GET /api/shifts/my-schedule?fromDate=2024-12-01&amp;toDate=2024-12-31&amp;sortBy=ShiftDate&amp;ascending=true
        /// </example>
        [HttpGet("my-schedule")]
        [Authorize] // Chỉ cần đăng nhập
        [ProducesResponseType(typeof(IEnumerable<UserShiftDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<IEnumerable<UserShiftDto>>> GetMySchedule(
            [FromQuery] DateOnly? fromDate = null,
            [FromQuery] DateOnly? toDate = null,
            [FromQuery] string? sortBy = "ShiftDate",
            [FromQuery] bool ascending = true)
        {
            try
            {
                // Get current user ID from JWT token
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest("Không thể xác định người dùng");
                }

                var assignments = await _shiftService.GetUserAssignmentsAsync(userId);
                
                // Apply date filtering if provided
                if (fromDate.HasValue)
                {
                    assignments = assignments.Where(a => a.ShiftDate >= fromDate.Value);
                }
                
                if (toDate.HasValue)
                {
                    assignments = assignments.Where(a => a.ShiftDate <= toDate.Value);
                }

                // Apply sorting
                assignments = sortBy?.ToLower() switch
                {
                    "shiftdate" => ascending ? assignments.OrderBy(a => a.ShiftDate) : assignments.OrderByDescending(a => a.ShiftDate),
                    "shiftname" => ascending ? assignments.OrderBy(a => a.ShiftName) : assignments.OrderByDescending(a => a.ShiftName),
                    "shiftstarttime" => ascending ? assignments.OrderBy(a => a.ShiftStartTime) : assignments.OrderByDescending(a => a.ShiftStartTime),
                    "status" => ascending ? assignments.OrderBy(a => a.Status) : assignments.OrderByDescending(a => a.Status),
                    _ => ascending ? assignments.OrderBy(a => a.ShiftDate) : assignments.OrderByDescending(a => a.ShiftDate)
                };

                var result = assignments.ToList();
                
                _logger.LogInformation("Retrieved {Count} schedule entries for user {UserId}", result.Count, userId);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving schedule for current user");
                return StatusCode(500, "Lỗi server khi lấy lịch làm việc");
            }
        }

        /// <summary>
        /// Retrieves a specific user's shift schedule. Only Admin and Manager users can access this endpoint.
        /// </summary>
        /// <param name="userId">The ID of the user whose schedule to retrieve</param>
        /// <param name="fromDate">Optional start date filter (YYYY-MM-DD format)</param>
        /// <param name="toDate">Optional end date filter (YYYY-MM-DD format)</param>
        /// <param name="sortBy">Sort field: ShiftDate, ShiftName, ShiftStartTime, Status (default: ShiftDate)</param>
        /// <param name="ascending">Sort direction: true for ascending, false for descending (default: true)</param>
        /// <returns>The specified user's shift schedule</returns>
        /// <response code="200">Returns the user's shift schedule</response>
        /// <response code="400">If the query parameters are invalid</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user doesn't have Admin or Manager role</response>
        /// <response code="404">If the specified user is not found</response>
        /// <response code="500">If there was an internal server error</response>
        /// <example>
        /// GET /api/shifts/user/5/schedule?fromDate=2024-12-01&amp;toDate=2024-12-31
        /// </example>
        [HttpGet("user/{userId}/schedule")]
        [Authorize(Roles = "Admin,Manager")] // Chỉ Admin/Manager
        [ProducesResponseType(typeof(IEnumerable<UserShiftDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<IEnumerable<UserShiftDto>>> GetUserSchedule(
            int userId,
            [FromQuery] DateOnly? fromDate = null,
            [FromQuery] DateOnly? toDate = null,
            [FromQuery] string? sortBy = "ShiftDate",
            [FromQuery] bool ascending = true)
        {
            try
            {
                // Validate that user exists
                if (!await _shiftService.UserExistsAsync(userId))
                {
                    return NotFound("Người dùng không tồn tại");
                }

                var assignments = await _shiftService.GetUserAssignmentsAsync(userId);
                
                // Apply date filtering if provided
                if (fromDate.HasValue)
                {
                    assignments = assignments.Where(a => a.ShiftDate >= fromDate.Value);
                }
                
                if (toDate.HasValue)
                {
                    assignments = assignments.Where(a => a.ShiftDate <= toDate.Value);
                }

                // Apply sorting
                assignments = sortBy?.ToLower() switch
                {
                    "shiftdate" => ascending ? assignments.OrderBy(a => a.ShiftDate) : assignments.OrderByDescending(a => a.ShiftDate),
                    "shiftname" => ascending ? assignments.OrderBy(a => a.ShiftName) : assignments.OrderByDescending(a => a.ShiftName),
                    "shiftstarttime" => ascending ? assignments.OrderBy(a => a.ShiftStartTime) : assignments.OrderByDescending(a => a.ShiftStartTime),
                    "status" => ascending ? assignments.OrderBy(a => a.Status) : assignments.OrderByDescending(a => a.Status),
                    _ => ascending ? assignments.OrderBy(a => a.ShiftDate) : assignments.OrderByDescending(a => a.ShiftDate)
                };

                var result = assignments.ToList();
                
                _logger.LogInformation("Retrieved {Count} schedule entries for user {UserId}", result.Count, userId);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving schedule for user {UserId}", userId);
                return StatusCode(500, "Lỗi server khi lấy lịch làm việc");
            }
        }
    }
}