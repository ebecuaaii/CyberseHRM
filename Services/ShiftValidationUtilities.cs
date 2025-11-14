using HRMCyberse.Models;

namespace HRMCyberse.Services
{
    /// <summary>
    /// Utility class for shift validation and business rule enforcement
    /// </summary>
    public static class ShiftValidationUtilities
    {
        /// <summary>
        /// Validates if a shift time configuration is valid for business rules
        /// </summary>
        /// <param name="startTime">Shift start time</param>
        /// <param name="endTime">Shift end time</param>
        /// <returns>Validation result with error message if invalid</returns>
        public static (bool IsValid, string? ErrorMessage) ValidateShiftTimeConfiguration(TimeOnly startTime, TimeOnly endTime)
        {
            // Check if start and end times are the same
            if (startTime == endTime)
            {
                return (false, "Thời gian bắt đầu và kết thúc không thể giống nhau");
            }

            var duration = CalculateShiftDuration(startTime, endTime);

            // Minimum shift duration: 30 minutes
            if (duration < 30)
            {
                return (false, "Ca làm việc phải có thời lượng tối thiểu 30 phút");
            }

            // Maximum shift duration: 24 hours
            if (duration > 1440)
            {
                return (false, "Ca làm việc không thể vượt quá 24 giờ");
            }

            // Business rule: Overnight shifts should have reasonable duration
            if (IsOvernightShift(startTime, endTime) && duration < 120)
            {
                return (false, "Ca đêm phải có thời lượng tối thiểu 2 giờ");
            }

            return (true, null);
        }

        /// <summary>
        /// Determines if a shift is an overnight shift (crosses midnight)
        /// </summary>
        /// <param name="startTime">Shift start time</param>
        /// <param name="endTime">Shift end time</param>
        /// <returns>True if overnight shift, false otherwise</returns>
        public static bool IsOvernightShift(TimeOnly startTime, TimeOnly endTime)
        {
            return endTime < startTime;
        }

        /// <summary>
        /// Calculates the duration of a shift in minutes
        /// </summary>
        /// <param name="startTime">Shift start time</param>
        /// <param name="endTime">Shift end time</param>
        /// <returns>Duration in minutes</returns>
        public static int CalculateShiftDuration(TimeOnly startTime, TimeOnly endTime)
        {
            if (endTime > startTime)
            {
                // Same day shift
                return (int)(endTime - startTime).TotalMinutes;
            }
            else
            {
                // Overnight shift (crosses midnight)
                var timeToMidnight = TimeOnly.MaxValue - startTime;
                var timeFromMidnight = endTime - TimeOnly.MinValue;
                return (int)(timeToMidnight.Add(timeFromMidnight).TotalMinutes);
            }
        }

        /// <summary>
        /// Validates shift assignment business rules
        /// </summary>
        /// <param name="shiftDate">Date of the shift assignment</param>
        /// <param name="currentDate">Current date for validation</param>
        /// <returns>Validation result</returns>
        public static (bool IsValid, string? ErrorMessage) ValidateShiftAssignmentDate(DateOnly shiftDate, DateOnly currentDate)
        {
            // Cannot assign shifts to past dates (except today)
            if (shiftDate < currentDate)
            {
                return (false, "Không thể gán ca cho ngày trong quá khứ");
            }

            // Cannot assign shifts too far in the future (e.g., more than 3 months)
            var maxFutureDate = currentDate.AddMonths(3);
            if (shiftDate > maxFutureDate)
            {
                return (false, "Không thể gán ca quá 3 tháng trong tương lai");
            }

            return (true, null);
        }

        /// <summary>
        /// Gets a human-readable description of shift timing
        /// </summary>
        /// <param name="startTime">Shift start time</param>
        /// <param name="endTime">Shift end time</param>
        /// <returns>Description string</returns>
        public static string GetShiftDescription(TimeOnly startTime, TimeOnly endTime)
        {
            var duration = CalculateShiftDuration(startTime, endTime);
            var hours = duration / 60;
            var minutes = duration % 60;

            var durationText = minutes > 0 ? $"{hours}h{minutes}m" : $"{hours}h";
            var shiftType = IsOvernightShift(startTime, endTime) ? "Ca đêm" : "Ca ngày";

            return $"{shiftType} ({startTime:HH:mm} - {endTime:HH:mm}, {durationText})";
        }

        /// <summary>
        /// Validates if a user can be assigned multiple shifts on the same day
        /// </summary>
        /// <param name="existingShifts">List of existing shifts for the user on the same date</param>
        /// <param name="newShiftStart">New shift start time</param>
        /// <param name="newShiftEnd">New shift end time</param>
        /// <returns>Validation result</returns>
        public static (bool IsValid, string? ErrorMessage) ValidateMultipleShiftsPerDay(
            IEnumerable<(TimeOnly Start, TimeOnly End)> existingShifts, 
            TimeOnly newShiftStart, 
            TimeOnly newShiftEnd)
        {
            foreach (var existingShift in existingShifts)
            {
                if (HasTimeOverlap(newShiftStart, newShiftEnd, existingShift.Start, existingShift.End))
                {
                    return (false, $"Ca mới bị trùng thời gian với ca hiện có ({existingShift.Start:HH:mm} - {existingShift.End:HH:mm})");
                }
            }

            return (true, null);
        }

        /// <summary>
        /// Checks if two time periods overlap
        /// </summary>
        /// <param name="start1">First period start time</param>
        /// <param name="end1">First period end time</param>
        /// <param name="start2">Second period start time</param>
        /// <param name="end2">Second period end time</param>
        /// <returns>True if periods overlap, false otherwise</returns>
        private static bool HasTimeOverlap(TimeOnly start1, TimeOnly end1, TimeOnly start2, TimeOnly end2)
        {
            // Convert to minutes from midnight for easier comparison
            var start1Minutes = GetMinutesFromMidnight(start1);
            var end1Minutes = GetMinutesFromMidnight(end1);
            var start2Minutes = GetMinutesFromMidnight(start2);
            var end2Minutes = GetMinutesFromMidnight(end2);

            // Handle overnight shifts
            if (IsOvernightShift(start1, end1))
            {
                end1Minutes += 1440; // Add 24 hours
            }
            if (IsOvernightShift(start2, end2))
            {
                end2Minutes += 1440; // Add 24 hours
            }

            // Check for overlap
            return start1Minutes < end2Minutes && start2Minutes < end1Minutes;
        }

        /// <summary>
        /// Converts TimeOnly to minutes from midnight
        /// </summary>
        /// <param name="time">Time to convert</param>
        /// <returns>Minutes from midnight</returns>
        private static int GetMinutesFromMidnight(TimeOnly time)
        {
            return time.Hour * 60 + time.Minute;
        }
    }
}