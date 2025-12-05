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
public class SettingsController : ControllerBase
{
    private readonly CybersehrmContext _context;

    public SettingsController(CybersehrmContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get payroll settings (Admin only)
    /// </summary>
    [HttpGet("payroll")]
    [RequireRole("Admin")]
    public async Task<ActionResult<object>> GetPayrollSettings()
    {
        var nightShiftBonus = await GetSettingValue("NightShiftBonus", "50000");
        var overtimeMultiplier = await GetSettingValue("OvertimeMultiplier", "1.5");
        var holidayMultiplier = await GetSettingValue("HolidayMultiplier", "2.0");

        return Ok(new
        {
            nightShiftBonus = decimal.Parse(nightShiftBonus),
            overtimeMultiplier = decimal.Parse(overtimeMultiplier),
            holidayMultiplier = decimal.Parse(holidayMultiplier)
        });
    }

    /// <summary>
    /// Update night shift bonus (Admin only)
    /// </summary>
    [HttpPut("payroll/night-shift-bonus")]
    [RequireRole("Admin")]
    public async Task<ActionResult> UpdateNightShiftBonus([FromBody] UpdateSettingDto dto)
    {
        if (dto.Value < 0)
            return BadRequest(new { message = "Night shift bonus cannot be negative" });

        await SetSettingValue("NightShiftBonus", dto.Value.ToString(), "Payroll");

        return Ok(new { message = "Night shift bonus updated successfully", value = dto.Value });
    }

    /// <summary>
    /// Update overtime multiplier (Admin only)
    /// </summary>
    [HttpPut("payroll/overtime-multiplier")]
    [RequireRole("Admin")]
    public async Task<ActionResult> UpdateOvertimeMultiplier([FromBody] UpdateSettingDto dto)
    {
        if (dto.Value < 1)
            return BadRequest(new { message = "Overtime multiplier must be at least 1.0" });

        await SetSettingValue("OvertimeMultiplier", dto.Value.ToString(), "Payroll");

        return Ok(new { message = "Overtime multiplier updated successfully", value = dto.Value });
    }

    /// <summary>
    /// Update holiday multiplier (Admin only)
    /// </summary>
    [HttpPut("payroll/holiday-multiplier")]
    [RequireRole("Admin")]
    public async Task<ActionResult> UpdateHolidayMultiplier([FromBody] UpdateSettingDto dto)
    {
        if (dto.Value < 1)
            return BadRequest(new { message = "Holiday multiplier must be at least 1.0" });

        await SetSettingValue("HolidayMultiplier", dto.Value.ToString(), "Payroll");

        return Ok(new { message = "Holiday multiplier updated successfully", value = dto.Value });
    }

    /// <summary>
    /// Get all settings (Admin only)
    /// </summary>
    [HttpGet]
    [RequireRole("Admin")]
    public async Task<ActionResult<IEnumerable<Setting>>> GetAllSettings()
    {
        var settings = await _context.Settings
            .OrderBy(s => s.Category)
            .ThenBy(s => s.Key)
            .ToListAsync();

        return Ok(settings);
    }

    // Helper methods
    private async Task<string> GetSettingValue(string key, string defaultValue)
    {
        var setting = await _context.Settings
            .FirstOrDefaultAsync(s => s.Key == key);

        return setting?.Value ?? defaultValue;
    }

    private async Task SetSettingValue(string key, string value, string category)
    {
        var setting = await _context.Settings
            .FirstOrDefaultAsync(s => s.Key == key);

        if (setting == null)
        {
            setting = new Setting
            {
                Key = key,
                Value = value,
                Category = category,
                Createdat = DateTime.UtcNow
            };
            _context.Settings.Add(setting);
        }
        else
        {
            setting.Value = value;
        }

        await _context.SaveChangesAsync();
    }
}

public class UpdateSettingDto
{
    public decimal Value { get; set; }
}
