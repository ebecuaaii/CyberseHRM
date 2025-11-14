using HRMCyberse.DTOs;
using HRMCyberse.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HRMCyberse.Controllers
{
    /// <summary>
    /// Late Arrival Request Management - Employee late arrival requests
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LateRequestsController : ControllerBase
    {
        private readonly IRequestService _requestService;

        public LateRequestsController(IRequestService requestService)
        {
            _requestService = requestService;
        }

        [HttpPost]
        public async Task<ActionResult<LateRequestResponseDto>> CreateLateRequest([FromBody] CreateLateRequestDto dto)
        {
            try
            {
                var result = await _requestService.CreateLateRequestAsync(dto);
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
        public async Task<ActionResult<LateRequestResponseDto>> ReviewLateRequest([FromBody] ReviewLateRequestDto dto)
        {
            try
            {
                var reviewerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var result = await _requestService.ReviewLateRequestAsync(dto, reviewerId);
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
        public async Task<ActionResult<LateRequestResponseDto>> GetLateRequest(int id)
        {
            var result = await _requestService.GetLateRequestByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<LateRequestResponseDto>>> GetUserLateRequests(int userId, [FromQuery] string? status = null)
        {
            return Ok(await _requestService.GetUserLateRequestsAsync(userId, status));
        }

        [HttpGet("pending")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<List<LateRequestResponseDto>>> GetPendingLateRequests()
        {
            return Ok(await _requestService.GetPendingLateRequestsAsync());
        }

        [HttpPost("{id}/cancel")]
        public async Task<ActionResult> CancelLateRequest(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var result = await _requestService.CancelLateRequestAsync(id, userId);
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
