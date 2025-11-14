using System.ComponentModel.DataAnnotations;

namespace HRMCyberse.DTOs
{
    public class UpdateShiftDto
    {
        [Required(ErrorMessage = "Tên ca làm việc là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên ca không được quá 100 ký tự")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Thời gian bắt đầu là bắt buộc")]
        public TimeOnly StartTime { get; set; }

        [Required(ErrorMessage = "Thời gian kết thúc là bắt buộc")]
        public TimeOnly EndTime { get; set; }
    }
}