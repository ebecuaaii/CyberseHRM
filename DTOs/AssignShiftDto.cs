using System.ComponentModel.DataAnnotations;

namespace HRMCyberse.DTOs
{
    /// <summary>
    /// Data transfer object for assigning a shift to an employee.
    /// </summary>
    public class AssignShiftDto
    {
        /// <summary>
        /// ID of the user to assign the shift to. User must exist and be active.
        /// </summary>
        /// <example>5</example>
        [Required(ErrorMessage = "User ID là bắt buộc")]
        public int UserId { get; set; }

        /// <summary>
        /// ID of the shift to assign. Shift must exist in the system.
        /// </summary>
        /// <example>1</example>
        [Required(ErrorMessage = "Shift ID là bắt buộc")]
        public int ShiftId { get; set; }

        /// <summary>
        /// Date for which the shift is assigned in YYYY-MM-DD format.
        /// </summary>
        /// <example>2024-12-25</example>
        [Required(ErrorMessage = "Ngày làm việc là bắt buộc")]
        public DateOnly ShiftDate { get; set; }

        /// <summary>
        /// Status of the assignment. Common values: assigned, completed, cancelled.
        /// </summary>
        /// <example>assigned</example>
        public string? Status { get; set; } = "assigned";
    }
}