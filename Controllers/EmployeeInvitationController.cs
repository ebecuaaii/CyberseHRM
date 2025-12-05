using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRMCyberse.Data;
using HRMCyberse.DTOs;
using HRMCyberse.Models;
using HRMCyberse.Services;
using System.Security.Claims;
using System.Security.Cryptography;

namespace HRMCyberse.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeeInvitationController : ControllerBase
{
    private readonly CybersehrmContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<EmployeeInvitationController> _logger;

    public EmployeeInvitationController(
        CybersehrmContext context,
        IEmailService emailService,
        ILogger<EmployeeInvitationController> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    /// <summary>
    /// Tạo lời mời nhân viên mới
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<EmployeeInvitationResponseDto>> CreateInvitation(CreateEmployeeInvitationDto dto)
    {
        try
        {
            // Kiểm tra branch tồn tại
            var branch = await _context.Branches.FindAsync(dto.BranchId);
            if (branch == null)
                return NotFound(new { message = "Chi nhánh không tồn tại" });

            // Kiểm tra email đã được mời chưa (chưa sử dụng và chưa hết hạn)
            var existingInvitation = await _context.EmployeeInvitations
                .Where(i => i.Email == dto.Email && 
                           i.IsUsed == false && 
                           i.ExpiresAt > DateTime.UtcNow)
                .FirstOrDefaultAsync();

            if (existingInvitation != null)
                return BadRequest(new { message = "Email này đã được mời và chưa hết hạn" });

            // Kiểm tra email đã tồn tại trong users chưa
            var existingUser = await _context.Users
                .Where(u => u.Email == dto.Email)
                .FirstOrDefaultAsync();

            if (existingUser != null)
                return BadRequest(new { message = "Email này đã được sử dụng bởi nhân viên khác" });

            // Tạo invitation token
            var token = GenerateSecureToken();
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var invitation = new EmployeeInvitation
            {
                Email = dto.Email,
                BranchId = dto.BranchId,
                Roleid = dto.RoleId,
                Departmentid = dto.DepartmentId,
                Positionid = dto.PositionId,
                Salaryrate = dto.SalaryRate,
                InvitationToken = token,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsUsed = false,
                CreatedBy = currentUserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.EmployeeInvitations.Add(invitation);
            await _context.SaveChangesAsync();

            // Load thông tin để gửi email
            string? departmentName = null;
            string? positionTitle = null;
            string? roleName = null;

            if (dto.DepartmentId.HasValue)
            {
                var dept = await _context.Departments.FindAsync(dto.DepartmentId.Value);
                departmentName = dept?.Name;
            }

            if (dto.PositionId.HasValue)
            {
                var pos = await _context.Positiontitles.FindAsync(dto.PositionId.Value);
                positionTitle = pos?.Titlename;
            }

            if (dto.RoleId.HasValue)
            {
                var role = await _context.Roles.FindAsync(dto.RoleId.Value);
                roleName = role?.Rolename;
            }

            // Gửi email thông báo trúng tuyển
            try
            {
                await _emailService.SendEmployeeInvitationAsync(
                    dto.Email,
                    branch.BranchCode,
                    token,
                    branch.BranchName,
                    departmentName,
                    positionTitle,
                    dto.SalaryRate,
                    roleName
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Không thể gửi email thông báo trúng tuyển");
                // Không throw exception, vẫn trả về invitation đã tạo
            }

            // Load related data
            await _context.Entry(invitation)
                .Reference(i => i.Branch)
                .LoadAsync();
            await _context.Entry(invitation)
                .Reference(i => i.Role)
                .LoadAsync();
            await _context.Entry(invitation)
                .Reference(i => i.Department)
                .LoadAsync();
            await _context.Entry(invitation)
                .Reference(i => i.Position)
                .LoadAsync();
            await _context.Entry(invitation)
                .Reference(i => i.CreatedByNavigation)
                .LoadAsync();

            var response = MapToResponseDto(invitation);
            return CreatedAtAction(nameof(GetInvitation), new { id = invitation.Id }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo lời mời nhân viên");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi tạo lời mời" });
        }
    }

    /// <summary>
    /// Lấy danh sách lời mời
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmployeeInvitationResponseDto>>> GetInvitations(
        [FromQuery] bool? isUsed = null,
        [FromQuery] bool? includeExpired = false)
    {
        try
        {
            var query = _context.EmployeeInvitations
                .Include(i => i.Branch)
                .Include(i => i.Role)
                .Include(i => i.Department)
                .Include(i => i.Position)
                .Include(i => i.CreatedByNavigation)
                .AsQueryable();

            if (isUsed.HasValue)
                query = query.Where(i => i.IsUsed == isUsed.Value);

            if (includeExpired == false)
                query = query.Where(i => i.ExpiresAt > DateTime.UtcNow);

            var invitations = await query
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            var response = invitations.Select(MapToResponseDto);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách lời mời");
            return StatusCode(500, new { message = "Đã xảy ra lỗi" });
        }
    }

    /// <summary>
    /// Lấy chi tiết lời mời
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<EmployeeInvitationResponseDto>> GetInvitation(int id)
    {
        try
        {
            var invitation = await _context.EmployeeInvitations
                .Include(i => i.Branch)
                .Include(i => i.Role)
                .Include(i => i.Department)
                .Include(i => i.Position)
                .Include(i => i.CreatedByNavigation)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invitation == null)
                return NotFound(new { message = "Không tìm thấy lời mời" });

            return Ok(MapToResponseDto(invitation));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy chi tiết lời mời");
            return StatusCode(500, new { message = "Đã xảy ra lỗi" });
        }
    }

    /// <summary>
    /// Xóa/hủy lời mời
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteInvitation(int id)
    {
        try
        {
            var invitation = await _context.EmployeeInvitations.FindAsync(id);
            if (invitation == null)
                return NotFound(new { message = "Không tìm thấy lời mời" });

            if (invitation.IsUsed == true)
                return BadRequest(new { message = "Không thể xóa lời mời đã được sử dụng" });

            _context.EmployeeInvitations.Remove(invitation);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã xóa lời mời thành công" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa lời mời");
            return StatusCode(500, new { message = "Đã xảy ra lỗi" });
        }
    }

    /// <summary>
    /// Test gửi email đơn giản - Dùng query parameter
    /// Example: POST /api/EmployeeInvitation/test-email/ngoctrucnguyen3012@gmail.com
    /// </summary>
    [HttpPost("test-email/{email}")]
    public async Task<IActionResult> TestEmail(string email)
    {
        try
        {
            _logger.LogInformation($"Test email request: {email}");
            
            await _emailService.SendEmployeeInvitationAsync(
                email,
                "TEST001",
                "test-token",
                "Test Branch",
                "IT Department",
                "Developer",
                50000,
                "Employee"
            );
            return Ok(new { message = $"Email test đã được gửi đến {email}. Kiểm tra inbox và spam folder." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi test email");
            return StatusCode(500, new { 
                message = $"Lỗi: {ex.Message}", 
                detail = ex.InnerException?.Message,
                email = email
            });
        }
    }

    /// <summary>
    /// Gửi lại email mời
    /// </summary>
    [HttpPost("{id}/resend")]
    public async Task<IActionResult> ResendInvitation(int id)
    {
        try
        {
            var invitation = await _context.EmployeeInvitations
                .Include(i => i.Branch)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invitation == null)
                return NotFound(new { message = "Không tìm thấy lời mời" });

            if (invitation.IsUsed == true)
                return BadRequest(new { message = "Lời mời đã được sử dụng" });

            if (invitation.ExpiresAt <= DateTime.UtcNow)
            {
                // Gia hạn thêm 7 ngày
                invitation.ExpiresAt = DateTime.UtcNow.AddDays(7);
                await _context.SaveChangesAsync();
            }

            await _emailService.SendEmployeeInvitationAsync(
                invitation.Email,
                invitation.Branch.BranchCode,
                invitation.InvitationToken,
                invitation.Branch.BranchName,
                invitation.Department?.Name,
                invitation.Position?.Titlename,
                invitation.Salaryrate,
                invitation.Role?.Rolename
            );

            return Ok(new { message = "Đã gửi lại email mời thành công" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gửi lại email mời");
            return StatusCode(500, new { message = "Không thể gửi email" });
        }
    }

    private string GenerateSecureToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }

    private EmployeeInvitationResponseDto MapToResponseDto(EmployeeInvitation invitation)
    {
        return new EmployeeInvitationResponseDto
        {
            Id = invitation.Id,
            Email = invitation.Email,
            BranchCode = invitation.Branch?.BranchCode ?? "",
            BranchName = invitation.Branch?.BranchName ?? "",
            RoleName = invitation.Role?.Rolename,
            DepartmentName = invitation.Department?.Name,
            PositionName = invitation.Position?.Titlename,
            SalaryRate = invitation.Salaryrate,
            InvitationToken = invitation.InvitationToken,
            ExpiresAt = invitation.ExpiresAt,
            IsUsed = invitation.IsUsed ?? false,
            UsedAt = invitation.UsedAt,
            CreatedByName = invitation.CreatedByNavigation?.Fullname,
            CreatedAt = invitation.CreatedAt ?? DateTime.UtcNow
        };
    }
}
