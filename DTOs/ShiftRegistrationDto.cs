using System.ComponentModel.DataAnnotations;

namespace HRMCyberse.DTOs
{
    /// <summary>
    /// DTO for creating a shift registration (1 shift, 1 date)
    /// </summary>
    public class CreateShiftRegistrationDto
    {
        [Required(ErrorMessage = "ID ca làm việc là bắt buộc")]
        public int ShiftId { get; set; }

        [Required(ErrorMessage = "Ngày đăng ký là bắt buộc")]
        public DateOnly RequestedDate { get; set; }
    }

    /// <summary>
    /// Response DTO for shift registration
    /// </summary>
    public class ShiftRegistrationDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? FullName { get; set; }
        public int ShiftId { get; set; }
        public string? ShiftName { get; set; }
        public TimeOnly? ShiftStartTime { get; set; }
        public TimeOnly? ShiftEndTime { get; set; }
        public DateOnly RequestedDate { get; set; }
        public string Status { get; set; } = "pending"; // pending, approved, rejected
        public int? ApprovedBy { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
    }

    /// <summary>
    /// DTO for admin/manager to review registration
    /// </summary>
    public class ReviewShiftRegistrationDto
    {
        [Required]
        [RegularExpression("^(approved|rejected)$", ErrorMessage = "Trạng thái phải là 'approved' hoặc 'rejected'")]
        public string Status { get; set; } = null!;
    }
}
