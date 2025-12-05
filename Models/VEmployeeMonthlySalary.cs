using System;
using System.Collections.Generic;

namespace HRMCyberse.Models;

public partial class VEmployeeMonthlySalary
{
    public int? Userid { get; set; }

    public string? Fullname { get; set; }

    public int? BranchId { get; set; }

    public string? BranchName { get; set; }

    public decimal? Year { get; set; }

    public decimal? Month { get; set; }

    public long? TotalDaysWorked { get; set; }

    public decimal? TotalHours { get; set; }

    public decimal? TotalOvertimeHours { get; set; }

    public decimal? RegularSalary { get; set; }

    public decimal? OvertimeSalary { get; set; }

    public decimal? TotalSalary { get; set; }

    public decimal? AvgSalaryRate { get; set; }
}
