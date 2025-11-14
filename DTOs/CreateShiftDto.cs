using System.ComponentModel.DataAnnotations;

namespace HRMCyberse.DTOs
{
    /// <summary>
    /// Data transfer object for creating a new work shift.
    /// </summary>
    public class CreateShiftDto
    {
        /// <summary>
        /// Name of the shift. Must be unique and not exceed 100 characters.
        /// </summary>
        /// <example>Morning Shift</example>
        [Required(ErrorMessage = "Tên ca làm việc là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên ca không được quá 100 ký tự")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Start time of the shift in 24-hour format (HH:mm:ss).
        /// </summary>
        /// <example>08:00:00</example>
        [Required(ErrorMessage = "Thời gian bắt đầu là bắt buộc")]
        public TimeOnly StartTime { get; set; }

        /// <summary>
        /// End time of the shift in 24-hour format (HH:mm:ss).
        /// For overnight shifts, end time can be less than start time.
        /// </summary>
        /// <example>16:00:00</example>
        [Required(ErrorMessage = "Thời gian kết thúc là bắt buộc")]
        public TimeOnly EndTime { get; set; }
    }
}