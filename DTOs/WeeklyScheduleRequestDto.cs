using System.ComponentModel.DataAnnotations;

namespace HRMCyberse.DTOs
{
    /// <summary>
    /// DTO for simple shift registration (1 shift, 1 date)
    /// </summary>
    public class SimpleShiftRegistrationDto
    {
        [Required(ErrorMessage = "ID ca làm việc là bắt buộc")]
        public int ShiftId { get; set; }

        [Required(ErrorMessage = "Ngày đăng ký là bắt buộc")]
        public DateOnly RequestedDate { get; set; }
    }

    /// <summary>
    /// DTO for employee weekly schedule availability request
    /// Employee registers their available time slots for the week
    /// </summary>
    public class CreateWeeklyScheduleRequestDto
    {
        public DateOnly? WeekStartDate { get; set; }

        public List<DayShiftAvailability>? Availability { get; set; }

        public string? Note { get; set; }

        // Simple registration fields (alternative to weekly)
        public int? ShiftId { get; set; }
        public DateOnly? RequestedDate { get; set; }
    }

    /// <summary>
    /// Availability for a specific day
    /// </summary>
    public class DayShiftAvailability
    {
        [Required]
        public DayOfWeek DayOfWeek { get; set; } // 0=Sunday, 1=Monday, etc.

        [Required]
        public List<int> ShiftIds { get; set; } = new(); // List of shift IDs employee can work
    }

    /// <summary>
    /// Response DTO for weekly schedule request
    /// </summary>
    public class WeeklyScheduleRequestDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? FullName { get; set; }
        public DateOnly WeekStartDate { get; set; }
        public DateOnly WeekEndDate { get; set; }
        public string Status { get; set; } = "pending"; // pending, reviewed, scheduled
        public string? AvailabilityData { get; set; } // JSON string of availability
        public string? Note { get; set; }
        public int? ReviewedBy { get; set; }
        public string? ReviewedByName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }

    /// <summary>
    /// DTO for admin to mark request as reviewed/scheduled
    /// </summary>
    public class ReviewWeeklyScheduleRequestDto
    {
        [Required]
        public int RequestId { get; set; }

        [Required]
        [RegularExpression("^(reviewed|scheduled)$", ErrorMessage = "Trạng thái phải là 'reviewed' hoặc 'scheduled'")]
        public string Status { get; set; } = null!;

        public string? Note { get; set; }
    }
}
