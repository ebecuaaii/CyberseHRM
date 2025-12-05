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
public class BranchController : ControllerBase
{
    private readonly CybersehrmContext _context;

    public BranchController(CybersehrmContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all branches (Admin only)
    /// </summary>
    [HttpGet]
    [RequireRole("Admin")]
    public async Task<ActionResult<IEnumerable<object>>> GetBranches()
    {
        var branches = await _context.Branches
            .Include(b => b.CompanyWifiLocations)
            .Where(b => b.IsActive == true)
            .Select(b => new
            {
                b.Id,
                b.BranchCode,
                b.BranchName,
                b.LocationAddress,
                b.IsActive,
                b.CreatedAt,
                b.UpdatedAt,
                WifiLocations = b.CompanyWifiLocations
                    .Where(w => w.IsActive == true)
                    .Select(w => new
                    {
                        w.Id,
                        w.LocationName,
                        w.WifiSsid,
                        w.WifiBssid,
                        w.IsActive
                    })
            })
            .ToListAsync();

        return Ok(branches);
    }

    /// <summary>
    /// Get branch by ID (Admin only)
    /// </summary>
    [HttpGet("{id}")]
    [RequireRole("Admin")]
    public async Task<ActionResult<object>> GetBranch(int id)
    {
        var branch = await _context.Branches
            .Include(b => b.CompanyWifiLocations)
            .Where(b => b.Id == id)
            .Select(b => new
            {
                b.Id,
                b.BranchCode,
                b.BranchName,
                b.LocationAddress,
                b.IsActive,
                b.CreatedAt,
                b.UpdatedAt,
                WifiLocations = b.CompanyWifiLocations
                    .Where(w => w.IsActive == true)
                    .Select(w => new
                    {
                        w.Id,
                        w.LocationName,
                        w.WifiSsid,
                        w.WifiBssid,
                        w.IsActive,
                        w.CreatedAt,
                        w.UpdatedAt
                    })
            })
            .FirstOrDefaultAsync();

        if (branch == null)
            return NotFound(new { message = "Branch not found" });

        return Ok(branch);
    }

    /// <summary>
    /// Create new branch with WiFi locations (Admin only)
    /// </summary>
    [HttpPost]
    [RequireRole("Admin")]
    public async Task<ActionResult<object>> CreateBranch([FromBody] CreateBranchDto dto)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(dto.BranchCode))
            return BadRequest(new { message = "Branch code is required" });
        
        if (string.IsNullOrWhiteSpace(dto.BranchName))
            return BadRequest(new { message = "Branch name is required" });

        // Check if branch code already exists
        if (await _context.Branches.AnyAsync(b => b.BranchCode == dto.BranchCode))
            return BadRequest(new { message = "Branch code already exists" });

        var branch = new Branch
        {
            BranchCode = dto.BranchCode,
            BranchName = dto.BranchName,
            LocationAddress = dto.LocationAddress,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Branches.Add(branch);
        await _context.SaveChangesAsync();

        // Add WiFi locations if provided
        if (dto.WifiLocations != null && dto.WifiLocations.Any())
        {
            foreach (var wifiDto in dto.WifiLocations)
            {
                var wifiLocation = new CompanyWifiLocation
                {
                    BranchId = branch.Id,
                    LocationName = wifiDto.LocationName,
                    WifiSsid = wifiDto.WifiSsid,
                    WifiBssid = wifiDto.WifiBssid,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.CompanyWifiLocations.Add(wifiLocation);
            }
            await _context.SaveChangesAsync();
        }

        return CreatedAtAction(nameof(GetBranch), new { id = branch.Id }, new
        {
            branch.Id,
            branch.BranchCode,
            branch.BranchName,
            branch.LocationAddress,
            message = "Branch created successfully"
        });
    }

    /// <summary>
    /// Update branch (Admin only)
    /// </summary>
    [HttpPut("{id}")]
    [RequireRole("Admin")]
    public async Task<ActionResult> UpdateBranch(int id, [FromBody] UpdateBranchDto dto)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(dto.BranchCode))
            return BadRequest(new { message = "Branch code is required" });
        
        if (string.IsNullOrWhiteSpace(dto.BranchName))
            return BadRequest(new { message = "Branch name is required" });

        var branch = await _context.Branches.FindAsync(id);
        if (branch == null)
            return NotFound(new { message = "Branch not found" });

        // Check if new branch code conflicts with existing
        if (dto.BranchCode != branch.BranchCode && 
            await _context.Branches.AnyAsync(b => b.BranchCode == dto.BranchCode))
            return BadRequest(new { message = "Branch code already exists" });

        branch.BranchCode = dto.BranchCode;
        branch.BranchName = dto.BranchName;
        branch.LocationAddress = dto.LocationAddress;
        branch.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Branch updated successfully" });
    }

    /// <summary>
    /// Delete branch (soft delete - Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [RequireRole("Admin")]
    public async Task<ActionResult> DeleteBranch(int id)
    {
        var branch = await _context.Branches.FindAsync(id);
        if (branch == null)
            return NotFound(new { message = "Branch not found" });

        branch.IsActive = false;
        branch.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Branch deleted successfully" });
    }

    /// <summary>
    /// Get all active WiFi locations for employee attendance (All authenticated users)
    /// </summary>
    [HttpGet("wifi-locations")]
    public async Task<ActionResult<IEnumerable<object>>> GetAllWifiLocations()
    {
        var wifiLocations = await _context.CompanyWifiLocations
            .Include(w => w.Branch)
            .Where(w => w.IsActive == true && w.Branch!.IsActive == true)
            .Select(w => new
            {
                w.Id,
                w.LocationName,
                w.WifiSsid,
                w.WifiBssid,
                BranchName = w.Branch!.BranchName,
                BranchCode = w.Branch.BranchCode
            })
            .ToListAsync();

        return Ok(wifiLocations);
    }
}

public class CreateBranchDto
{
    public string BranchCode { get; set; } = null!;
    public string BranchName { get; set; } = null!;
    public string? LocationAddress { get; set; }
    public List<WifiLocationDto>? WifiLocations { get; set; }
}

public class UpdateBranchDto
{
    public string BranchCode { get; set; } = null!;
    public string BranchName { get; set; } = null!;
    public string? LocationAddress { get; set; }
}

public class WifiLocationDto
{
    public string LocationName { get; set; } = null!;
    public string WifiSsid { get; set; } = null!;
    public string? WifiBssid { get; set; }
}
