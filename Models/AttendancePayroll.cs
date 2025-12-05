using System;
using System.Collections.Generic;

namespace HRMCyberse.Models;

/// <summary>
/// Chi tiết tính lương theo từng ca làm việc
/// </summary>
public partial class AttendancePayroll
{
    public int Id { get; set; }

    public int? Attendanceid { get; set; }

    public int? Userid { get; set; }

    public int? Shiftid { get; set; }

    /// <summary>
    /// Lương cơ bản theo giờ của nhân viên
    /// </summary>
    public decimal? Salaryrate { get; set; }

    /// <summary>
    /// Hệ số nhân cho ca (1.0 = ca thường, 1.5 = ca đêm, 2.0 = ngày lễ)
    /// </summary>
    public decimal? Shiftmultiplier { get; set; }

    /// <summary>
    /// Lương thực tế = basesalaryrate × shiftmultiplier
    /// </summary>
    public decimal? Effectiverate { get; set; }

    /// <summary>
    /// Số giờ làm thực tế
    /// </summary>
    public decimal? Hoursworked { get; set; }

    /// <summary>
    /// Giờ làm thêm (nếu có)
    /// </summary>
    public decimal? Overtimehours { get; set; }

    /// <summary>
    /// Lương OT (thường = effectiverate × 1.5)
    /// </summary>
    public decimal? Overtimerate { get; set; }

    /// <summary>
    /// hoursworked × effectiverate
    /// </summary>
    public decimal? Regularamount { get; set; }

    /// <summary>
    /// overtimehours × overtimerate
    /// </summary>
    public decimal? Overtimeamount { get; set; }

    /// <summary>
    /// regularamount + overtimeamount
    /// </summary>
    public decimal? Totalamount { get; set; }

    public DateTime? Createdat { get; set; }

    public virtual Attendance? Attendance { get; set; }

    public virtual Shift? Shift { get; set; }

    public virtual User? User { get; set; }
}
