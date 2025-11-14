using HRMCyberse.Constants;
using HRMCyberse.Models;

namespace HRMCyberse.Services
{
    public static class AttendanceUtilities
    {
        /// <summary>
        /// Calculate attendance status based on check-in time and shift start time
        /// </summary>
        public static string CalculateAttendanceStatus(DateTime checkInTime, TimeOnly shiftStartTime)
        {
            var shiftStartDateTime = checkInTime.Date.Add(shiftStartTime.ToTimeSpan());
            
            if (checkInTime <= shiftStartDateTime)
            {
                return AttendanceConstants.Status.OnTime;
            }
            else
            {
                return AttendanceConstants.Status.Late;
            }
        }

        /// <summary>
        /// Calculate late minutes if employee checked in late
        /// </summary>
        public static TimeSpan? CalculateLateMinutes(DateTime checkInTime, TimeOnly shiftStartTime)
        {
            var shiftStartDateTime = checkInTime.Date.Add(shiftStartTime.ToTimeSpan());
            
            if (checkInTime > shiftStartDateTime)
            {
                return checkInTime - shiftStartDateTime;
            }
            
            return null;
        }

        /// <summary>
        /// Calculate total worked hours
        /// </summary>
        public static TimeSpan? CalculateWorkedHours(DateTime? checkInTime, DateTime? checkOutTime)
        {
            if (checkInTime.HasValue && checkOutTime.HasValue)
            {
                return checkOutTime.Value - checkInTime.Value;
            }
            
            return null;
        }

        /// <summary>
        /// Check if checkout is early compared to shift end time
        /// </summary>
        public static bool IsEarlyCheckout(DateTime checkOutTime, TimeOnly shiftEndTime)
        {
            var shiftEndDateTime = checkOutTime.Date.Add(shiftEndTime.ToTimeSpan());
            return checkOutTime < shiftEndDateTime;
        }

        /// <summary>
        /// Generate attendance summary for a date range
        /// </summary>
        public static AttendanceSummary CalculateAttendanceSummary(List<Attendance> attendances)
        {
            var summary = new AttendanceSummary();
            
            foreach (var attendance in attendances)
            {
                summary.TotalDays++;
                
                switch (attendance.Status)
                {
                    case AttendanceConstants.Status.OnTime:
                        summary.OnTimeDays++;
                        break;
                    case AttendanceConstants.Status.Late:
                        summary.LateDays++;
                        break;
                    case AttendanceConstants.Status.Absent:
                        summary.AbsentDays++;
                        break;
                }

                if (attendance.Checkintime.HasValue && attendance.Checkouttime.HasValue)
                {
                    var workedHours = attendance.Checkouttime.Value - attendance.Checkintime.Value;
                    summary.TotalWorkedHours += workedHours;
                }
            }
            
            return summary;
        }
    }

    public class AttendanceSummary
    {
        public int TotalDays { get; set; }
        public int OnTimeDays { get; set; }
        public int LateDays { get; set; }
        public int AbsentDays { get; set; }
        public TimeSpan TotalWorkedHours { get; set; }
        
        public double AttendanceRate => TotalDays > 0 ? (double)(OnTimeDays + LateDays) / TotalDays * 100 : 0;
        public double PunctualityRate => TotalDays > 0 ? (double)OnTimeDays / TotalDays * 100 : 0;
    }
}