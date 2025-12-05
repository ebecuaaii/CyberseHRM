using System;
using System.Collections.Generic;

namespace HRMCyberse.Models;

public partial class Payroll
{
    public int Id { get; set; }

    public int Userid { get; set; }

    public int Month { get; set; }

    public int Year { get; set; }

    public decimal? Totalhours { get; set; }

    public decimal? Basesalary { get; set; }

    public decimal? Bonuses { get; set; }

    public decimal? Penalties { get; set; }

    public decimal? Netsalary { get; set; }

    public DateTime? Createdat { get; set; }

    public decimal? Salaryrate { get; set; }

    public bool? IsFinalized { get; set; }

    public DateTime? FinalizedAt { get; set; }

    public virtual ICollection<Salarydetail> Salarydetails { get; set; } = new List<Salarydetail>();

    public virtual User User { get; set; } = null!;
}
