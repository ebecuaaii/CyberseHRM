namespace HRMCyberse.DTOs
{
    public class LoginResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public UserResponseDto? User { get; set; }
        public string? Token { get; set; } // Để sau này thêm JWT
    }
}