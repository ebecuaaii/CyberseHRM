namespace HRMCyberse.DTOs
{
    /// <summary>
    /// Data transfer object representing a work shift with all its details.
    /// </summary>
    public class ShiftDto
    {
        /// <summary>
        /// Unique identifier for the shift.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the shift (e.g., "Ca1", "Morning Shift").
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Start time of the shift in 24-hour format.
        /// </summary>
        public TimeOnly StartTime { get; set; }

        /// <summary>
        /// End time of the shift in 24-hour format.
        /// </summary>
        public TimeOnly EndTime { get; set; }

        /// <summary>
        /// Duration of the shift in minutes. Automatically calculated.
        /// </summary>
        public int? DurationMinutes { get; set; }

        /// <summary>
        /// Full name of the user who created this shift.
        /// </summary>
        public string? CreatedByName { get; set; }

        /// <summary>
        /// Timestamp when the shift was created.
        /// </summary>
        public DateTime? CreatedAt { get; set; }
    }
}