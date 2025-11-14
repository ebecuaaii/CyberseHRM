using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRMCyberse.Data;

namespace HRMCyberse.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LookupsController : ControllerBase
    {
        private readonly CybersehrmContext _context;
        private readonly ILogger<LookupsController> _logger;

        public LookupsController(CybersehrmContext context, ILogger<LookupsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("departments")]
        public async Task<ActionResult> GetDepartments()
        {
            try
            {
                var departments = await _context.Departments
                    .Select(d => new { d.Id, d.Name, d.Description })
                    .ToListAsync();
                
                return Ok(departments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách departments");
                return StatusCode(500, "Lỗi server");
            }
        }

        [HttpGet("positions")]
        public async Task<ActionResult> GetPositions()
        {
            try
            {
                var positions = await _context.Positiontitles
                    .Select(p => new { p.Id, Title = p.Titlename, p.Description })
                    .ToListAsync();
                
                return Ok(positions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách positions");
                return StatusCode(500, "Lỗi server");
            }
        }

        [HttpGet("roles")]
        public async Task<ActionResult> GetRoles()
        {
            try
            {
                var roles = await _context.Roles
                    .Select(r => new { r.Id, r.Rolename, r.Description })
                    .ToListAsync();
                
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách roles");
                return StatusCode(500, "Lỗi server");
            }
        }

        [HttpGet("positions/by-department/{departmentId}")]
        public async Task<ActionResult> GetPositionsByDepartment(int departmentId)
        {
            try
            {
                // Vì Positiontitle không có departmentid, trả về tất cả positions
                var positions = await _context.Positiontitles
                    .Select(p => new { p.Id, Title = p.Titlename, p.Description })
                    .ToListAsync();
                
                return Ok(positions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy positions theo department {DepartmentId}", departmentId);
                return StatusCode(500, "Lỗi server");
            }
        }
    }
}