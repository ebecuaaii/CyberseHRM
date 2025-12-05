using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRMCyberse.Data;
using HRMCyberse.Models;
using HRMCyberse.Attributes;

namespace HRMCyberse.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PositionController : ControllerBase
{
    private readonly CybersehrmContext _context;

    public PositionController(CybersehrmContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all positions
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetPositions()
    {
        var positions = await _context.Positiontitles
            .Select(p => new
            {
                p.Id,
                p.Titlename,
                p.Description,
                EmployeeCount = p.Users.Count()
            })
            .ToListAsync();

        return Ok(positions);
    }

    /// <summary>
    /// Get position by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetPosition(int id)
    {
        var position = await _context.Positiontitles
            .Where(p => p.Id == id)
            .Select(p => new
            {
                p.Id,
                p.Titlename,
                p.Description,
                Employees = p.Users.Select(u => new
                {
                    u.Id,
                    u.Fullname,
                    u.Email,
                    Department = u.Department != null ? u.Department.Name : null,
                    Branch = u.Branch != null ? u.Branch.BranchName : null
                }),
                EmployeeCount = p.Users.Count()
            })
            .FirstOrDefaultAsync();

        if (position == null)
            return NotFound(new { message = "Position not found" });

        return Ok(position);
    }

    /// <summary>
    /// Create new position (Admin only)
    /// </summary>
    [HttpPost]
    [RequireRole("Admin")]
    public async Task<ActionResult<object>> CreatePosition([FromBody] CreatePositionDto dto)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(dto.TitleName))
            return BadRequest(new { message = "Position title is required" });

        // Check if position title already exists
        if (await _context.Positiontitles.AnyAsync(p => p.Titlename == dto.TitleName))
            return BadRequest(new { message = "Position title already exists" });

        var position = new Positiontitle
        {
            Titlename = dto.TitleName,
            Description = dto.Description
        };

        _context.Positiontitles.Add(position);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPosition), new { id = position.Id }, new
        {
            id = position.Id,
            titleName = position.Titlename,
            description = position.Description,
            message = "Position created successfully"
        });
    }

    /// <summary>
    /// Update position (Admin only)
    /// </summary>
    [HttpPut("{id}")]
    [RequireRole("Admin")]
    public async Task<ActionResult> UpdatePosition(int id, [FromBody] UpdatePositionDto dto)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(dto.TitleName))
            return BadRequest(new { message = "Position title is required" });

        var position = await _context.Positiontitles.FindAsync(id);
        if (position == null)
            return NotFound(new { message = "Position not found" });

        // Check if new title conflicts with existing
        if (dto.TitleName != position.Titlename && 
            await _context.Positiontitles.AnyAsync(p => p.Titlename == dto.TitleName))
            return BadRequest(new { message = "Position title already exists" });

        position.Titlename = dto.TitleName;
        position.Description = dto.Description;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Position updated successfully" });
    }

    /// <summary>
    /// Delete position (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [RequireRole("Admin")]
    public async Task<ActionResult> DeletePosition(int id)
    {
        var position = await _context.Positiontitles
            .Include(p => p.Users)
            .Include(p => p.EmployeeInvitations)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (position == null)
            return NotFound(new { message = "Position not found" });

        // Check if position has employees
        if (position.Users.Any())
            return BadRequest(new { message = "Cannot delete position with existing employees. Please reassign employees first." });

        // Check if position has pending invitations
        if (position.EmployeeInvitations.Any(i => i.IsUsed == false))
            return BadRequest(new { message = "Cannot delete position with pending invitations." });

        _context.Positiontitles.Remove(position);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Position deleted successfully" });
    }

    /// <summary>
    /// Get employees in a position
    /// </summary>
    [HttpGet("{id}/employees")]
    public async Task<ActionResult<IEnumerable<object>>> GetPositionEmployees(int id)
    {
        var position = await _context.Positiontitles.FindAsync(id);
        if (position == null)
            return NotFound(new { message = "Position not found" });

        var employees = await _context.Users
            .Where(u => u.Positionid == id)
            .Select(u => new
            {
                u.Id,
                u.Fullname,
                u.Email,
                u.Phone,
                Department = u.Department != null ? u.Department.Name : null,
                Branch = u.Branch != null ? u.Branch.BranchName : null,
                u.Salaryrate,
                u.Hiredate
            })
            .ToListAsync();

        return Ok(employees);
    }
}

public class CreatePositionDto
{
    public string TitleName { get; set; } = null!;
    public string? Description { get; set; }
}

public class UpdatePositionDto
{
    public string TitleName { get; set; } = null!;
    public string? Description { get; set; }
}
