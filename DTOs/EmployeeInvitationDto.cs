using System.ComponentModel.DataAnnotations;

namespace HRMCyberse.DTOs;

public class CreateEmployeeInvitationDto
{
    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Branch ID là bắt buộc")]
    public int BranchId { get; set; }

    public int? RoleId { get; set; }
    
    public int? DepartmentId { get; set; }
    
    public int? PositionId { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Lương phải lớn hơn 0")]
    public decimal? SalaryRate { get; set; }
}

public class EmployeeInvitationResponseDto
{
    public int Id { get; set; }
    public string Email { get; set; } = null!;
    public string BranchCode { get; set; } = null!;
    public string BranchName { get; set; } = null!;
    public string? RoleName { get; set; }
    public string? DepartmentName { get; set; }
    public string? PositionName { get; set; }
    public decimal? SalaryRate { get; set; }
    public string InvitationToken { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime? UsedAt { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AcceptInvitationDto
{
    [Required(ErrorMessage = "Token là bắt buộc")]
    public string Token { get; set; } = null!;

    [Required(ErrorMessage = "Username là bắt buộc")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username phải từ 3-50 ký tự")]
    public string Username { get; set; } = null!;

    [Required(ErrorMessage = "Password là bắt buộc")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password phải từ 6-100 ký tự")]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Họ tên là bắt buộc")]
    public string FullName { get; set; } = null!;

    public string? Phone { get; set; }
}

public class InvitationDetailsDto
{
    public string Email { get; set; } = null!;
    public string BranchCode { get; set; } = null!;
    public string BranchName { get; set; } = null!;
    public string? RoleName { get; set; }
    public string? DepartmentName { get; set; }
    public string? PositionName { get; set; }
    public decimal? SalaryRate { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsExpired { get; set; }
    public bool IsUsed { get; set; }
}
