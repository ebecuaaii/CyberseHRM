using HRMCyberse.DTOs;
using HRMCyberse.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HRMCyberse.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RewardPenaltyController : ControllerBase
    {
        private readonly IPayrollService _payrollService;

        public RewardPenaltyController(IPayrollService payrollService)
        {
            _payrollService = payrollService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<RewardPenaltyResponseDto>> CreateRewardPenalty([FromBody] CreateRewardPenaltyDto dto)
        {
            try
            {
                var createdBy = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var result = await _payrollService.CreateRewardPenaltyAsync(dto, createdBy);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", details = ex.Message });
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<RewardPenaltyResponseDto>>> GetUserRewardPenalties(
            int userId,
            [FromQuery] int? month = null,
            [FromQuery] int? year = null)
        {
            var result = await _payrollService.GetUserRewardPenaltiesAsync(userId, month, year);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult> DeleteRewardPenalty(int id)
        {
            var result = await _payrollService.DeleteRewardPenaltyAsync(id);
            if (!result) return NotFound();
            return Ok(new { message = "Deleted successfully." });
        }
    }
}
