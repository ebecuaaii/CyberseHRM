using System;

namespace HRMCyberse.Models
{
    public partial class WeeklyscheduleRequest
    {
        public int Id { get; set; }
        public int Userid { get; set; }
        public DateOnly WeekStartDate { get; set; }
        public DateOnly WeekEndDate { get; set; }
        public string Status { get; set; } = "pending";
        public string AvailabilityData { get; set; } = null!; // JSON string
        public string? Note { get; set; }
        public int? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual User? ReviewedByNavigation { get; set; }
    }
}
