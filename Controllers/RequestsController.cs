using HRMCyberse.DTOs;
using HRMCyberse.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HRMCyberse.Controllers
{
    /// <summary>
    /// Leave Request Management - Employee leave requests and approvals
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RequestsController : ControllerBase
    {
        private readonly IRequestService _requestService;

        public RequestsController(IRequestService requestService)
        {
            _requestService = requestService;
        }

        #region Leave Requests

        /// <summary>
        /// Create a new leave request
        /// </summary>
        [HttpPost("leave")]
        public async Task<ActionResult<LeaveRequestResponseDto>> CreateLeaveRequest([FromBody] CreateLeaveRequestDto dto)
        {
            try
            {
                var result = await _requestService.CreateLeaveRequestAsync(dto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", details = ex.Message });
            }
        }

        /// <summary>
        /// Review a leave request (Admin/Manager only)
        /// </summary>
        [HttpPost("leave/review")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<LeaveRequestResponseDto>> ReviewLeaveRequest([FromBody] ReviewLeaveRequestDto dto)
        {
            try
            {
                var reviewerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var result = await _requestService.ReviewLeaveRequestAsync(dto, reviewerId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", details = ex.Message });
            }
        }

        /// <summary>
        /// Get leave request by ID
        /// </summary>
        [HttpGet("leave/{id}")]
        public async Task<ActionResult<LeaveRequestResponseDto>> GetLeaveRequest(int id)
        {
            var result = await _requestService.GetLeaveRequestByIdAsync(id);
            if (result == null)
            {
                return NotFound(new { message = "Leave request not found." });
            }
            return Ok(result);
        }

        /// <summary>
        /// Get user's leave requests
        /// </summary>
        [HttpGet("leave/user/{userId}")]
        public async Task<ActionResult<List<LeaveRequestResponseDto>>> GetUserLeaveRequests(
            int userId,
            [FromQuery] string? status = null)
        {
            var result = await _requestService.GetUserLeaveRequestsAsync(userId, status);
            return Ok(result);
        }

        /// <summary>
        /// Get all pending leave requests (Admin/Manager only)
        /// </summary>
        [HttpGet("leave/pending")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<List<LeaveRequestResponseDto>>> GetPendingLeaveRequests()
        {
            var result = await _requestService.GetPendingLeaveRequestsAsync();
            return Ok(result);
        }

        /// <summary>
        /// Cancel a leave request
        /// </summary>
        [HttpPost("leave/{id}/cancel")]
        public async Task<ActionResult> CancelLeaveRequest(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var result = await _requestService.CancelLeaveRequestAsync(id, userId);
                if (!result)
                {
                    return NotFound(new { message = "Leave request not found or you don't have permission." });
                }
                return Ok(new { message = "Leave request cancelled successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        #endregion
    }
}
