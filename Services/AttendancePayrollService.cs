using HRMCyberse.Data;
using HRMCyberse.Models;
using Microsoft.EntityFrameworkCore;

namespace HRMCyberse.Services;

public class AttendancePayrollService : IAttendancePayrollService
{
    private readonly CybersehrmContext _context;
    private readonly ILogger<AttendancePayrollService> _logger;
    private readonly IConfiguration _configuration;

    public AttendancePayrollService(
        CybersehrmContext context,
        ILogger<AttendancePayrollService> logger,
        IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<AttendancePayrollDto?> CalculateAttendancePayrollAsync(int attendanceId)
    {
        var attendance = await _context.Attendances
            .Include(a => a.User)
            .Include(a => a.Shift)
            .FirstOrDefaultAsync(a => a.Id == attendanceId);

        if (attendance == null)
        {
            _logger.LogWarning($"Attendance {attendanceId} not found");
            return null;
        }

        if (!attendance.Checkintime.HasValue || !attendance.Checkouttime.HasValue)
        {
            _logger.LogWarning($"Attendance {attendanceId} chưa checkout");
            return null;
        }

        // Check if already calculated
        var existing = await _context.AttendancePayrolls
            .FirstOrDefaultAsync(ap => ap.Attendanceid == attendanceId);

        if (existing != null)
        {
            _logger.LogInformation($"Attendance {attendanceId} đã được tính lương");
            return await GetAttendancePayrollAsync(existing.Id);
        }

        // Get user salary rate
        var salaryRate = attendance.User?.Salaryrate ?? 0;
        if (salaryRate == 0)
        {
            _logger.LogWarning($"User {attendance.Userid} chưa có salary rate");
            return null;
        }

        // Calculate hours worked (checkout - checkin)
        var totalMinutes = (attendance.Checkouttime.Value - attendance.Checkintime.Value).TotalMinutes;
        var hoursWorked = (decimal)(totalMinutes / 60);

        // Simple calculation: Lương = salaryRate × số giờ làm thực tế
        // Không có multiplier, không có overtime
        var shiftMultiplier = 1.0m;
        var effectiveRate = salaryRate;
        var overtimeHours = 0m;
        var overtimeRate = 0m;
        var regularAmount = hoursWorked * salaryRate;
        var overtimeAmount = 0m;
        var totalAmount = regularAmount;

        // Create attendance payroll record
        var attendancePayroll = new AttendancePayroll
        {
            Attendanceid = attendanceId,
            Userid = attendance.Userid,
            Shiftid = attendance.Shiftid,
            Salaryrate = salaryRate,
            Shiftmultiplier = shiftMultiplier,
            Effectiverate = effectiveRate,
            Hoursworked = regularHours,
            Overtimehours = overtimeHours,
            Overtimerate = overtimeRate,
            Regularamount = regularAmount,
            Overtimeamount = overtimeAmount,
            Totalamount = totalAmount,
            Createdat = DateTime.UtcNow
        };

        _context.AttendancePayrolls.Add(attendancePayroll);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Đã tính lương cho attendance {attendanceId}: {totalAmount:N0} VND");

        return await GetAttendancePayrollAsync(attendancePayroll.Id);
    }

    public async Task<AttendancePayrollDto?> GetAttendancePayrollAsync(int id)
    {
        var payroll = await _context.AttendancePayrolls
            .Include(ap => ap.User)
            .Include(ap => ap.Shift)
            .FirstOrDefaultAsync(ap => ap.Id == id);

        if (payroll == null) return null;

        return new AttendancePayrollDto
        {
            Id = payroll.Id,
            AttendanceId = payroll.Attendanceid ?? 0,
            UserId = payroll.Userid ?? 0,
            UserName = payroll.User?.Fullname,
            ShiftId = payroll.Shiftid,
            ShiftName = payroll.Shift?.Name,
            SalaryRate = payroll.Salaryrate ?? 0,
            ShiftMultiplier = payroll.Shiftmultiplier ?? 1,
            EffectiveRate = payroll.Effectiverate ?? 0,
            HoursWorked = payroll.Hoursworked ?? 0,
            OvertimeHours = payroll.Overtimehours ?? 0,
            OvertimeRate = payroll.Overtimerate ?? 0,
            RegularAmount = payroll.Regularamount ?? 0,
            OvertimeAmount = payroll.Overtimeamount ?? 0,
            TotalAmount = payroll.Totalamount ?? 0,
            CreatedAt = payroll.Createdat
        };
    }

    public async Task<List<AttendancePayrollDto>> GetDailyPayrollAsync(int userId, DateTime date)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1);

        var payrolls = await _context.AttendancePayrolls
            .Include(ap => ap.User)
            .Include(ap => ap.Shift)
            .Include(ap => ap.Attendance)
            .Where(ap => ap.Userid == userId &&
                        ap.Attendance!.Checkintime >= startOfDay &&
                        ap.Attendance.Checkintime < endOfDay)
            .OrderBy(ap => ap.Attendance!.Checkintime)
            .ToListAsync();

        return payrolls.Select(p => new AttendancePayrollDto
        {
            Id = p.Id,
            AttendanceId = p.Attendanceid ?? 0,
            UserId = p.Userid ?? 0,
            UserName = p.User?.Fullname,
            ShiftId = p.Shiftid,
            ShiftName = p.Shift?.Name,
            SalaryRate = p.Salaryrate ?? 0,
            ShiftMultiplier = p.Shiftmultiplier ?? 1,
            EffectiveRate = p.Effectiverate ?? 0,
            HoursWorked = p.Hoursworked ?? 0,
            OvertimeHours = p.Overtimehours ?? 0,
            OvertimeRate = p.Overtimerate ?? 0,
            RegularAmount = p.Regularamount ?? 0,
            OvertimeAmount = p.Overtimeamount ?? 0,
            TotalAmount = p.Totalamount ?? 0,
            CreatedAt = p.Createdat
        }).ToList();
    }

    public async Task<MonthlyPayrollSummaryDto> GetMonthlyPayrollSummaryAsync(int userId, int month, int year)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1);

        var payrolls = await _context.AttendancePayrolls
            .Include(ap => ap.User)
            .Include(ap => ap.Shift)
            .Include(ap => ap.Attendance)
            .Where(ap => ap.Userid == userId &&
                        ap.Attendance!.Checkintime >= startDate &&
                        ap.Attendance.Checkintime < endDate)
            .OrderBy(ap => ap.Attendance!.Checkintime)
            .ToListAsync();

        var user = await _context.Users.FindAsync(userId);

        var details = payrolls.Select(p => new AttendancePayrollDto
        {
            Id = p.Id,
            AttendanceId = p.Attendanceid ?? 0,
            UserId = p.Userid ?? 0,
            UserName = p.User?.Fullname,
            ShiftId = p.Shiftid,
            ShiftName = p.Shift?.Name,
            SalaryRate = p.Salaryrate ?? 0,
            ShiftMultiplier = p.Shiftmultiplier ?? 1,
            EffectiveRate = p.Effectiverate ?? 0,
            HoursWorked = p.Hoursworked ?? 0,
            OvertimeHours = p.Overtimehours ?? 0,
            OvertimeRate = p.Overtimerate ?? 0,
            RegularAmount = p.Regularamount ?? 0,
            OvertimeAmount = p.Overtimeamount ?? 0,
            TotalAmount = p.Totalamount ?? 0,
            CreatedAt = p.Createdat
        }).ToList();

        return new MonthlyPayrollSummaryDto
        {
            UserId = userId,
            UserName = user?.Fullname,
            Month = month,
            Year = year,
            TotalDays = payrolls.Count,
            TotalHours = payrolls.Sum(p => p.Hoursworked ?? 0),
            TotalOvertimeHours = payrolls.Sum(p => p.Overtimehours ?? 0),
            TotalRegularAmount = payrolls.Sum(p => p.Regularamount ?? 0),
            TotalOvertimeAmount = payrolls.Sum(p => p.Overtimeamount ?? 0),
            TotalAmount = payrolls.Sum(p => p.Totalamount ?? 0),
            Details = details
        };
    }

    private decimal GetShiftMultiplier(Shift? shift)
    {
        if (shift == null) return 1.0m;

        var shiftName = shift.Name?.ToLower() ?? "";

        // Night shift: 1.5x
        if (shiftName.Contains("đêm") || shiftName.Contains("night"))
        {
            return 1.5m;
        }

        // Holiday shift: 2.0x
        if (shiftName.Contains("lễ") || shiftName.Contains("holiday"))
        {
            var holidayMultiplier = decimal.Parse(_configuration["PayrollSettings:HolidayMultiplier"] ?? "2.0");
            return holidayMultiplier;
        }

        // Normal shift: 1.0x
        return 1.0m;
    }

    private decimal GetShiftDuration(Shift? shift)
    {
        if (shift == null) return 8.0m; // Default 8 hours

        if (shift.Starttime != default && shift.Endtime != default)
        {
            var duration = (shift.Endtime - shift.Starttime).TotalHours;
            return (decimal)duration;
        }

        return 8.0m;
    }
}
