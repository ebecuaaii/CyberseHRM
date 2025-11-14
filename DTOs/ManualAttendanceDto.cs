using System.ComponentModel.DataAnnotations;

namespace HRMCyberse.DTOs
{
    public class ManualAttendanceDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int ShiftId { get; set; }

        [Required]
        public DateTime CheckInTime { get; set; }

        public DateTime? CheckOutTime { get; set; }

        public string? Status { get; set; }

        public string? Notes { get; set; }

        [Required]
        public int CreatedByManagerId { get; set; }
    }
}