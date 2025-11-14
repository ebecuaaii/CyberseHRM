using HRMCyberse.DTOs;
using HRMCyberse.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HRMCyberse.Controllers
{
    /// <summary>
    /// Shift Change Request Management - Employee shift change requests
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ShiftRequestsController : ControllerBase
    {
        private readonly IRequestService _requestService;

        public ShiftRequestsController(IRequestService requestService)
        {
            _requestService = requestService;
        }

        [HttpPost]
        public async Task<ActionResult<ShiftRequestResponseDto>> CreateShiftRequest([FromBody] CreateShiftRequestDto dto)
        {
            try
            {
                var result = await _requestService.CreateShiftRequestAsync(dto);
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

        [HttpPost("review")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ShiftRequestResponseDto>> ReviewShiftRequest([FromBody] ReviewShiftRequestDto dto)
        {
            try
            {
                var reviewerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var result = await _requestService.ReviewShiftRequestAsync(dto, reviewerId);
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
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ShiftRequestResponseDto>> GetShiftRequest(int id)
        {
            var result = await _requestService.GetShiftRequestByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<ShiftRequestResponseDto>>> GetUserShiftRequests(int userId, [FromQuery] string? status = null)
        {
            return Ok(await _requestService.GetUserShiftRequestsAsync(userId, status));
        }

        [HttpGet("pending")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<List<ShiftRequestResponseDto>>> GetPendingShiftRequests()
        {
            return Ok(await _requestService.GetPendingShiftRequestsAsync());
        }

        [HttpPost("{id}/cancel")]
        public async Task<ActionResult> CancelShiftRequest(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var result = await _requestService.CancelShiftRequestAsync(id, userId);
                if (!result) return NotFound();
                return Ok(new { message = "Cancelled successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
