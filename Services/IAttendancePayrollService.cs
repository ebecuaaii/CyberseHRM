namespace HRMCyberse.Services;

public interface IAttendancePayrollService
{
    /// <summary>
    /// Tính lương cho một attendance (sau khi checkout)
    /// </summary>
    Task<AttendancePayrollDto?> CalculateAttendancePayrollAsync(int attendanceId);
    
    /// <summary>
    /// Lấy lương theo ngày của user
    /// </summary>
    Task<List<AttendancePayrollDto>> GetDailyPayrollAsync(int userId, DateTime date);
    
    /// <summary>
    /// Lấy tổng lương theo tháng của user
    /// </summary>
    Task<MonthlyPayrollSummaryDto> GetMonthlyPayrollSummaryAsync(int userId, int month, int year);
    
    /// <summary>
    /// Lấy chi tiết attendance payroll
    /// </summary>
    Task<AttendancePayrollDto?> GetAttendancePayrollAsync(int id);
}

public class AttendancePayrollDto
{
    public int Id { get; set; }
    public int AttendanceId { get; set; }
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public int? ShiftId { get; set; }
    public string? ShiftName { get; set; }
    public decimal SalaryRate { get; set; }
    public decimal ShiftMultiplier { get; set; }
    public decimal EffectiveRate { get; set; }
    public decimal HoursWorked { get; set; }
    public decimal OvertimeHours { get; set; }
    public decimal OvertimeRate { get; set; }
    public decimal RegularAmount { get; set; }
    public decimal OvertimeAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class MonthlyPayrollSummaryDto
{
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public int TotalDays { get; set; }
    public decimal TotalHours { get; set; }
    public decimal TotalOvertimeHours { get; set; }
    public decimal TotalRegularAmount { get; set; }
    public decimal TotalOvertimeAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public List<AttendancePayrollDto> Details { get; set; } = new();
}
