using System;
using System.Collections.Generic;

namespace HRMCyberse.Models;

public partial class Salaryadjustment
{
    public int Id { get; set; }

    public int Userid { get; set; }

    public decimal? Oldrate { get; set; }

    public decimal? Newrate { get; set; }

    public string? Reason { get; set; }

    public int? Approvedby { get; set; }

    public DateTime? Createdat { get; set; }

    public virtual User? ApprovedbyNavigation { get; set; }

    public virtual User User { get; set; } = null!;
}
