using HRMCyberse.Constants;
using HRMCyberse.Data;
using HRMCyberse.DTOs;
using HRMCyberse.Models;
using Microsoft.EntityFrameworkCore;

namespace HRMCyberse.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly CybersehrmContext _context;

        public AttendanceService(CybersehrmContext context)
        {
            _context = context;
        }

        public async Task<AttendanceResponseDto> CheckInAsync(CheckInDto checkInDto)
        {
            // Validate if user can check in
            if (!await CanCheckInAsync(checkInDto.UserId, checkInDto.ShiftId))
            {
                throw new InvalidOperationException("User cannot check in at this time or has already checked in today.");
            }

            // Get shift information to determine status
            var shift = await _context.Shifts.FindAsync(checkInDto.ShiftId);
            if (shift == null)
            {
                throw new ArgumentException("Shift not found.");
            }

            var now = DateTime.UtcNow;
            
            // Determine status based on check-in time
            string status = AttendanceUtilities.CalculateAttendanceStatus(now, shift.Starttime);

            var attendance = new Attendance
            {
                Userid = checkInDto.UserId,
                Shiftid = checkInDto.ShiftId,
                Checkintime = now,
                Checkinlat = checkInDto.Latitude,
                Checkinlng = checkInDto.Longitude,
                Checkinimageurl = checkInDto.ImageUrl,
                Status = status,
                Notes = checkInDto.Notes
            };

            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();

            // Add image if provided
            if (!string.IsNullOrEmpty(checkInDto.ImageUrl))
            {
                var attendanceImage = new Attendanceimage
                {
                    Attendanceid = attendance.Id,
                    Imageurl = checkInDto.ImageUrl,
                    Type = AttendanceConstants.ImageType.CheckIn
                };
                _context.Attendanceimages.Add(attendanceImage);
                await _context.SaveChangesAsync();
            }

            return await GetAttendanceByIdAsync(attendance.Id);
        }

        public async Task<AttendanceResponseDto> CheckOutAsync(CheckOutDto checkOutDto)
        {
            if (!await CanCheckOutAsync(checkOutDto.AttendanceId))
            {
                throw new InvalidOperationException("Cannot check out. Invalid attendance record or already checked out.");
            }

            var attendance = await _context.Attendances.FindAsync(checkOutDto.AttendanceId);
            if (attendance == null)
            {
                throw new ArgumentException("Attendance record not found.");
            }

            var now = DateTime.UtcNow;
            attendance.Checkouttime = now;
            attendance.Checkoutlat = checkOutDto.Latitude;
            attendance.Checkoutlng = checkOutDto.Longitude;
            attendance.Checkoutimageurl = checkOutDto.ImageUrl;
            
            if (!string.IsNullOrEmpty(checkOutDto.Notes))
            {
                attendance.Notes = string.IsNullOrEmpty(attendance.Notes) 
                    ? checkOutDto.Notes 
                    : $"{attendance.Notes}; {checkOutDto.Notes}";
            }

            _context.Attendances.Update(attendance);

            // Add checkout image if provided
            if (!string.IsNullOrEmpty(checkOutDto.ImageUrl))
            {
                var attendanceImage = new Attendanceimage
                {
                    Attendanceid = attendance.Id,
                    Imageurl = checkOutDto.ImageUrl,
                    Type = AttendanceConstants.ImageType.CheckOut
                };
                _context.Attendanceimages.Add(attendanceImage);
            }

            await _context.SaveChangesAsync();

            return await GetAttendanceByIdAsync(attendance.Id);
        }

        public async Task<AttendanceResponseDto> CreateManualAttendanceAsync(ManualAttendanceDto manualAttendanceDto)
        {
            // Verify the manager has permission (this would typically check roles)
            var manager = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == manualAttendanceDto.CreatedByManagerId);
            
            if (manager == null)
            {
                throw new ArgumentException("Manager not found.");
            }
            
            // Optional: Verify manager has correct role
            if (manager.Role?.Rolename != "Admin" && manager.Role?.Rolename != "Manager")
            {
                throw new ArgumentException("User does not have permission to create manual attendance.");
            }

            // Check if attendance already exists for this user and date
            var existingAttendance = await _context.Attendances
                .Where(a => a.Userid == manualAttendanceDto.UserId && 
                           a.Checkintime.HasValue &&
                           a.Checkintime.Value.Date == manualAttendanceDto.CheckInTime.Date)
                .FirstOrDefaultAsync();

            if (existingAttendance != null)
            {
                throw new InvalidOperationException("Attendance record already exists for this date.");
            }

            var attendance = new Attendance
            {
                Userid = manualAttendanceDto.UserId,
                Shiftid = manualAttendanceDto.ShiftId,
                Checkintime = manualAttendanceDto.CheckInTime,
                Checkouttime = manualAttendanceDto.CheckOutTime,
                Status = manualAttendanceDto.Status ?? AttendanceConstants.Status.ManualEntry,
                Notes = $"Manual entry by manager (ID: {manualAttendanceDto.CreatedByManagerId}). {manualAttendanceDto.Notes}"
            };

            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();

            return await GetAttendanceByIdAsync(attendance.Id);
        }

        public async Task<AttendanceResponseDto?> GetTodayAttendanceAsync(int userId)
        {
            var today = DateTime.Today;
            var attendance = await _context.Attendances
                .Include(a => a.User)
                .Include(a => a.Shift)
                .Include(a => a.Attendanceimages)
                .Where(a => a.Userid == userId && 
                           a.Checkintime.HasValue &&
                           a.Checkintime.Value.Date == today)
                .FirstOrDefaultAsync();

            return attendance != null ? MapToResponseDto(attendance) : null;
        }

        public async Task<List<AttendanceResponseDto>> GetUserAttendanceHistoryAsync(int userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Attendances
                .Include(a => a.User)
                .Include(a => a.Shift)
                .Include(a => a.Attendanceimages)
                .Where(a => a.Userid == userId);

            if (startDate.HasValue)
                query = query.Where(a => a.Checkintime >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(a => a.Checkintime <= endDate.Value);

            var attendances = await query
                .OrderByDescending(a => a.Checkintime)
                .ToListAsync();

            return attendances.Select(MapToResponseDto).ToList();
        }   
     public async Task<List<AttendanceReportDto>> GetAttendanceReportAsync(AttendanceReportRequestDto request)
        {
            var query = _context.Attendances
                .Include(a => a.User)
                .ThenInclude(u => u.Department)
                .Include(a => a.Shift)
                .Where(a => a.Checkintime >= request.StartDate && a.Checkintime <= request.EndDate);

            if (request.UserId.HasValue)
                query = query.Where(a => a.Userid == request.UserId.Value);

            if (request.DepartmentId.HasValue)
                query = query.Where(a => a.User.Departmentid == request.DepartmentId.Value);

            if (!string.IsNullOrEmpty(request.Status))
                query = query.Where(a => a.Status == request.Status);

            var attendances = await query
                .OrderBy(a => a.User.Fullname)
                .ThenBy(a => a.Checkintime)
                .ToListAsync();

            return attendances.Select(a => new AttendanceReportDto
            {
                UserId = a.Userid,
                UserName = a.User.Fullname ?? "Unknown",
                Department = a.User.Department?.Name ?? "Unknown",
                Date = a.Checkintime?.Date ?? DateTime.MinValue,
                ShiftName = a.Shift?.Name,
                ShiftStartTime = a.Shift?.Starttime,
                ShiftEndTime = a.Shift?.Endtime,
                CheckInTime = a.Checkintime,
                CheckOutTime = a.Checkouttime,
                Status = a.Status ?? "Unknown",
                WorkedHours = AttendanceUtilities.CalculateWorkedHours(a.Checkintime, a.Checkouttime),
                LateMinutes = a.Checkintime.HasValue && a.Shift != null && a.Status == AttendanceConstants.Status.Late
                    ? AttendanceUtilities.CalculateLateMinutes(a.Checkintime.Value, a.Shift.Starttime)
                    : null,
                Notes = a.Notes
            }).ToList();
        }

        public async Task<AttendanceSummary> GetAttendanceSummaryAsync(int userId, DateTime startDate, DateTime endDate)
        {
            var attendances = await _context.Attendances
                .Include(a => a.Shift)
                .Where(a => a.Userid == userId && 
                           a.Checkintime >= startDate && 
                           a.Checkintime <= endDate)
                .ToListAsync();

            return AttendanceUtilities.CalculateAttendanceSummary(attendances);
        }

        public async Task<bool> CanCheckInAsync(int userId, int shiftId)
        {
            // Check if user already checked in today
            var today = DateTime.Today;
            var existingAttendance = await _context.Attendances
                .Where(a => a.Userid == userId && 
                           a.Checkintime.HasValue &&
                           a.Checkintime.Value.Date == today)
                .FirstOrDefaultAsync();

            if (existingAttendance != null)
                return false;

            // Verify shift exists and user is assigned to it
            var userShift = await _context.Usershifts
                .Where(us => us.Userid == userId && us.Shiftid == shiftId)
                .FirstOrDefaultAsync();

            return userShift != null;
        }

        public async Task<bool> CanCheckOutAsync(int attendanceId)
        {
            var attendance = await _context.Attendances.FindAsync(attendanceId);
            return attendance != null && attendance.Checkintime.HasValue && !attendance.Checkouttime.HasValue;
        }

        private async Task<AttendanceResponseDto> GetAttendanceByIdAsync(int attendanceId)
        {
            var attendance = await _context.Attendances
                .Include(a => a.User)
                .Include(a => a.Shift)
                .Include(a => a.Attendanceimages)
                .FirstOrDefaultAsync(a => a.Id == attendanceId);

            if (attendance == null)
                throw new ArgumentException("Attendance record not found.");

            return MapToResponseDto(attendance);
        }

        private static AttendanceResponseDto MapToResponseDto(Attendance attendance)
        {
            return new AttendanceResponseDto
            {
                Id = attendance.Id,
                UserId = attendance.Userid,
                UserName = attendance.User?.Fullname ?? "Unknown",
                ShiftId = attendance.Shiftid,
                ShiftName = attendance.Shift?.Name,
                CheckInTime = attendance.Checkintime,
                CheckOutTime = attendance.Checkouttime,
                CheckInLat = attendance.Checkinlat,
                CheckInLng = attendance.Checkinlng,
                CheckOutLat = attendance.Checkoutlat,
                CheckOutLng = attendance.Checkoutlng,
                CheckInImageUrl = attendance.Checkinimageurl,
                CheckOutImageUrl = attendance.Checkoutimageurl,
                Status = attendance.Status,
                Notes = attendance.Notes,
                Images = attendance.Attendanceimages?.Select(img => new AttendanceImageDto
                {
                    Id = img.Id,
                    ImageUrl = img.Imageurl,
                    Type = img.Type
                }).ToList() ?? new List<AttendanceImageDto>()
            };
        }
    }
}