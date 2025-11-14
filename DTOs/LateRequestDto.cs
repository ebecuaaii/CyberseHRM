using System.ComponentModel.DataAnnotations;

namespace HRMCyberse.DTOs
{
    // Request DTOs
    public class CreateLateRequestDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int ShiftId { get; set; }

        [Required]
        public DateOnly RequestDate { get; set; }

        [Required]
        public TimeOnly ExpectedArrivalTime { get; set; }

        [Required]
        public string Reason { get; set; } = string.Empty;
    }

    public class ReviewLateRequestDto
    {
        [Required]
        public int RequestId { get; set; }

        [Required]
        public string Status { get; set; } = string.Empty; // Approved, Rejected

        public string? ReviewNotes { get; set; }
    }

    // Response DTOs
    public class LateRequestResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int ShiftId { get; set; }
        public string ShiftName { get; set; } = string.Empty;
        public DateOnly RequestDate { get; set; }
        public TimeOnly ExpectedArrivalTime { get; set; }
        public TimeOnly? ShiftStartTime { get; set; }
        public TimeSpan? LateMinutes { get; set; }
        public string? Reason { get; set; }
        public string? Status { get; set; }
        public int? ReviewedBy { get; set; }
        public string? ReviewerName { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
