namespace HRMCyberse.DTOs
{
    public class AttendanceReportDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string? ShiftName { get; set; }
        public TimeOnly? ShiftStartTime { get; set; }
        public TimeOnly? ShiftEndTime { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public TimeSpan? WorkedHours { get; set; }
        public TimeSpan? LateMinutes { get; set; }
        public string? Notes { get; set; }
    }

    public class AttendanceReportRequestDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? UserId { get; set; }
        public int? DepartmentId { get; set; }
        public string? Status { get; set; }
    }
}