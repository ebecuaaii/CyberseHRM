using Microsoft.AspNetCore.Mvc;
using HRMCyberse.Services;
using HRMCyberse.DTOs;

namespace HRMCyberse.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SimpleShiftController : ControllerBase
    {
        private readonly IShiftService _shiftService;
        private readonly ILogger<SimpleShiftController> _logger;

        public SimpleShiftController(IShiftService shiftService, ILogger<SimpleShiftController> logger)
        {
            _shiftService = shiftService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ShiftDto>>> GetAllShifts()
        {
            try
            {
                var shifts = await _shiftService.GetAllShiftsAsync();
                _logger.LogInformation("Retrieved {Count} shifts", shifts.Count());
                return Ok(shifts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving shifts");
                return StatusCode(500, new { 
                    error = ex.Message, 
                    innerError = ex.InnerException?.Message,
                    stackTrace = ex.StackTrace 
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ShiftDto>> CreateShift(CreateShiftDto createShiftDto)
        {
            try
            {
                _logger.LogInformation("Creating shift: {ShiftName}", createShiftDto.Name);

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

                // Dùng user ID mặc định cho test
                int createdBy = 1;

                var shift = await _shiftService.CreateShiftAsync(createShiftDto, createdBy);
                
                _logger.LogInformation("Created new shift: {ShiftName} with ID {ShiftId}", shift.Name, shift.Id);
                
                return CreatedAtAction(nameof(GetShiftById), new { id = shift.Id }, shift);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating shift: {ShiftName}", createShiftDto?.Name ?? "Unknown");
                return StatusCode(500, new { 
                    error = ex.Message, 
                    innerError = ex.InnerException?.Message,
                    stackTrace = ex.StackTrace 
                });
            }
        }

        [HttpGet("{id}")]
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
                return StatusCode(500, new { 
                    error = ex.Message, 
                    innerError = ex.InnerException?.Message 
                });
            }
        }

        [HttpPut("{id}")]
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

                // Dùng user ID mặc định cho test
                int updatedBy = 1;

                var updatedShift = await _shiftService.UpdateShiftAsync(id, updateShiftDto, updatedBy);
                
                _logger.LogInformation("Updated shift {ShiftId}: {ShiftName}", id, updatedShift.Name);
                
                return Ok(updatedShift);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating shift {ShiftId}", id);
                return StatusCode(500, new { 
                    error = ex.Message, 
                    innerError = ex.InnerException?.Message 
                });
            }
        }

        [HttpDelete("{id}")]
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

                // Dùng user ID mặc định cho test
                int deletedBy = 1;

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
                return StatusCode(500, new { 
                    error = ex.Message, 
                    innerError = ex.InnerException?.Message 
                });
            }
        }
    }
}