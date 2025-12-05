using System;
using System.Collections.Generic;

namespace HRMCyberse.Models;

public partial class VAttendancePayrollDetail
{
    public int? Id { get; set; }

    public int? Attendanceid { get; set; }

    public DateTime? Checkintime { get; set; }

    public DateTime? Checkouttime { get; set; }

    public int? Userid { get; set; }

    public string? Fullname { get; set; }

    public int? BranchId { get; set; }

    public string? BranchName { get; set; }

    public string? Name { get; set; }

    public decimal? Salaryrate { get; set; }

    public decimal? Shiftmultiplier { get; set; }

    public decimal? Effectiverate { get; set; }

    public decimal? Hoursworked { get; set; }

    public decimal? Overtimehours { get; set; }

    public decimal? Regularamount { get; set; }

    public decimal? Overtimeamount { get; set; }

    public decimal? Totalamount { get; set; }

    public DateTime? Createdat { get; set; }
}
