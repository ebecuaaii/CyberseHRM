namespace HRMCyberse.DTOs
{
    public class UserShiftDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int ShiftId { get; set; }
        public string ShiftName { get; set; } = string.Empty;
        public TimeOnly ShiftStartTime { get; set; }
        public TimeOnly ShiftEndTime { get; set; }
        public DateOnly ShiftDate { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}