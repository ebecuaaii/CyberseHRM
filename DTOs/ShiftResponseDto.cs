namespace HRMCyberse.DTOs
{
    public class ShiftResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public ShiftDto? Data { get; set; }
    }

    public class ShiftListResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public IEnumerable<ShiftDto>? Data { get; set; }
    }

    public class UserShiftResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public UserShiftDto? Data { get; set; }
    }

    public class UserShiftListResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public IEnumerable<UserShiftDto>? Data { get; set; }
    }
}