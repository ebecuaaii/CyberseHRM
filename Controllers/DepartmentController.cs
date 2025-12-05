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
public class DepartmentController : ControllerBase
{
    private readonly CybersehrmContext _context;

    public DepartmentController(CybersehrmContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all departments
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetDepartments()
    {
        var departments = await _context.Departments
            .Select(d => new
            {
                d.Id,
                d.Name,
                d.Description,
                d.Createdat,
                EmployeeCount = d.Users.Count()
            })
            .ToListAsync();

        return Ok(departments);
    }

    /// <summary>
    /// Get department by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetDepartment(int id)
    {
        var department = await _context.Departments
            .Where(d => d.Id == id)
            .Select(d => new
            {
                d.Id,
                d.Name,
                d.Description,
                d.Createdat,
                Employees = d.Users.Select(u => new
                {
                    u.Id,
                    u.Fullname,
                    u.Email,
                    Position = u.Position != null ? u.Position.Titlename : null
                }),
                EmployeeCount = d.Users.Count()
            })
            .FirstOrDefaultAsync();

        if (department == null)
            return NotFound(new { message = "Department not found" });

        return Ok(department);
    }

    /// <summary>
    /// Create new department (Admin only)
    /// </summary>
    [HttpPost]
    [RequireRole("Admin")]
    public async Task<ActionResult<object>> CreateDepartment([FromBody] CreateDepartmentDto dto)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { message = "Department name is required" });

        // Check if department name already exists
        if (await _context.Departments.AnyAsync(d => d.Name == dto.Name))
            return BadRequest(new { message = "Department name already exists" });

        var department = new Department
        {
            Name = dto.Name,
            Description = dto.Description,
            Createdat = DateTime.UtcNow
        };

        _context.Departments.Add(department);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetDepartment), new { id = department.Id }, new
        {
            id = department.Id,
            name = department.Name,
            description = department.Description,
            message = "Department created successfully"
        });
    }

    /// <summary>
    /// Update department (Admin only)
    /// </summary>
    [HttpPut("{id}")]
    [RequireRole("Admin")]
    public async Task<ActionResult> UpdateDepartment(int id, [FromBody] UpdateDepartmentDto dto)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { message = "Department name is required" });

        var department = await _context.Departments.FindAsync(id);
        if (department == null)
            return NotFound(new { message = "Department not found" });

        // Check if new name conflicts with existing
        if (dto.Name != department.Name && 
            await _context.Departments.AnyAsync(d => d.Name == dto.Name))
            return BadRequest(new { message = "Department name already exists" });

        department.Name = dto.Name;
        department.Description = dto.Description;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Department updated successfully" });
    }

    /// <summary>
    /// Delete department (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [RequireRole("Admin")]
    public async Task<ActionResult> DeleteDepartment(int id)
    {
        var department = await _context.Departments
            .Include(d => d.Users)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (department == null)
            return NotFound(new { message = "Department not found" });

        // Check if department has employees
        if (department.Users.Any())
            return BadRequest(new { message = "Cannot delete department with existing employees. Please reassign employees first." });

        _context.Departments.Remove(department);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Department deleted successfully" });
    }

    /// <summary>
    /// Get employees in a department
    /// </summary>
    [HttpGet("{id}/employees")]
    public async Task<ActionResult<IEnumerable<object>>> GetDepartmentEmployees(int id)
    {
        var department = await _context.Departments.FindAsync(id);
        if (department == null)
            return NotFound(new { message = "Department not found" });

        var employees = await _context.Users
            .Where(u => u.Departmentid == id)
            .Select(u => new
            {
                u.Id,
                u.Fullname,
                u.Email,
                u.Phone,
                Position = u.Position != null ? u.Position.Titlename : null,
                Branch = u.Branch != null ? u.Branch.BranchName : null,
                u.Hiredate
            })
            .ToListAsync();

        return Ok(employees);
    }
}

public class CreateDepartmentDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}

public class UpdateDepartmentDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}
