using System;
using System.Collections.Generic;

namespace HRMCyberse.Models;

public partial class VCurrentMonthSalary
{
    public int? Userid { get; set; }

    public string? Fullname { get; set; }

    public string? Email { get; set; }

    public int? BranchId { get; set; }

    public string? BranchName { get; set; }

    public long? DaysWorked { get; set; }

    public decimal? TotalHours { get; set; }

    public decimal? TotalOvertimeHours { get; set; }

    public decimal? RegularSalary { get; set; }

    public decimal? OvertimeSalary { get; set; }

    public decimal? TotalSalary { get; set; }

    public decimal? AvgSalaryRate { get; set; }
}
