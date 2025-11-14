using System.ComponentModel.DataAnnotations;

namespace HRMCyberse.DTOs
{
    // Request DTOs
    public class CreateLeaveRequestDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public DateOnly StartDate { get; set; }

        [Required]
        public DateOnly EndDate { get; set; }

        [Required]
        public string Reason { get; set; } = string.Empty;
    }

    public class ReviewLeaveRequestDto
    {
        [Required]
        public int RequestId { get; set; }

        [Required]
        public string Status { get; set; } = string.Empty; // Approved, Rejected

        public string? ReviewNotes { get; set; }
    }

    // Response DTOs
    public class LeaveRequestResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int TotalDays { get; set; }
        public string? Reason { get; set; }
        public string? Status { get; set; }
        public int? ReviewedBy { get; set; }
        public string? ReviewerName { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
