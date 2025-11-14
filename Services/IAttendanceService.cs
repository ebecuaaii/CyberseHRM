using HRMCyberse.DTOs;

namespace HRMCyberse.Services
{
    public interface IAttendanceService
    {
        Task<AttendanceResponseDto> CheckInAsync(CheckInDto checkInDto);
        Task<AttendanceResponseDto> CheckOutAsync(CheckOutDto checkOutDto);
        Task<AttendanceResponseDto> CreateManualAttendanceAsync(ManualAttendanceDto manualAttendanceDto);
        Task<AttendanceResponseDto?> GetTodayAttendanceAsync(int userId);
        Task<List<AttendanceResponseDto>> GetUserAttendanceHistoryAsync(int userId, DateTime? startDate = null, DateTime? endDate = null);
        Task<List<AttendanceReportDto>> GetAttendanceReportAsync(AttendanceReportRequestDto request);
        Task<AttendanceSummary> GetAttendanceSummaryAsync(int userId, DateTime startDate, DateTime endDate);
        Task<bool> CanCheckInAsync(int userId, int shiftId);
        Task<bool> CanCheckOutAsync(int attendanceId);
    }
}