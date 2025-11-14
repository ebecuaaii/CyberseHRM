namespace HRMCyberse.DTOs
{
    public class AttendanceResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int? ShiftId { get; set; }
        public string? ShiftName { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public decimal? CheckInLat { get; set; }
        public decimal? CheckInLng { get; set; }
        public decimal? CheckOutLat { get; set; }
        public decimal? CheckOutLng { get; set; }
        public string? CheckInImageUrl { get; set; }
        public string? CheckOutImageUrl { get; set; }
        public string? Status { get; set; }
        public string? Notes { get; set; }
        public List<AttendanceImageDto> Images { get; set; } = new List<AttendanceImageDto>();
    }

    public class AttendanceImageDto
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? Type { get; set; }
    }
}