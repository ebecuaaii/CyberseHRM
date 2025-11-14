using HRMCyberse.DTOs;
using HRMCyberse.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMCyberse.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PayrollController : ControllerBase
    {
        private readonly IPayrollService _payrollService;

        public PayrollController(IPayrollService payrollService)
        {
            _payrollService = payrollService;
        }

        [HttpPost("generate")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<List<PayrollResponseDto>>> GeneratePayroll([FromBody] GeneratePayrollDto dto)
        {
            try
            {
                var result = await _payrollService.GeneratePayrollAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", details = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PayrollResponseDto>> GetPayroll(int id)
        {
            var result = await _payrollService.GetPayrollByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<PayrollResponseDto>> GetUserPayroll(
            int userId,
            [FromQuery] int month,
            [FromQuery] int year)
        {
            var result = await _payrollService.GetUserPayrollAsync(userId, month, year);
            if (result == null) return NotFound(new { message = "Payroll not found for this period." });
            return Ok(result);
        }

        [HttpGet("user/{userId}/history")]
        public async Task<ActionResult<List<PayrollResponseDto>>> GetUserPayrollHistory(int userId)
        {
            var result = await _payrollService.GetUserPayrollHistoryAsync(userId);
            return Ok(result);
        }

        [HttpGet("summary")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<PayrollSummaryDto>> GetPayrollSummary(
            [FromQuery] int month,
            [FromQuery] int year)
        {
            var result = await _payrollService.GetPayrollSummaryAsync(month, year);
            return Ok(result);
        }

        [HttpPut("update")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<PayrollResponseDto>> UpdatePayroll([FromBody] UpdatePayrollDto dto)
        {
            try
            {
                var result = await _payrollService.UpdatePayrollAsync(dto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
