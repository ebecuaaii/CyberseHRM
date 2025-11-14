using System;
using System.Collections.Generic;

namespace HRMCyberse.Models;

public partial class Salarydetail
{
    public int Id { get; set; }

    public int? Payrollid { get; set; }

    public string Description { get; set; } = null!;

    public decimal Amount { get; set; }

    public DateTime? Createdat { get; set; }

    public virtual Payroll? Payroll { get; set; }
}
