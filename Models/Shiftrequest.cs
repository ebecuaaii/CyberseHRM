using System;
using System.Collections.Generic;

namespace HRMCyberse.Models;

public partial class Shiftrequest
{
    public int Id { get; set; }

    public int Userid { get; set; }

    public int Shiftid { get; set; }

    public DateOnly Shiftdate { get; set; }

    public string? Reason { get; set; }

    public string? Status { get; set; }

    public int? Reviewedby { get; set; }

    public DateTime? Reviewedat { get; set; }

    public DateTime? Createdat { get; set; }

    public virtual User? ReviewedbyNavigation { get; set; }

    public virtual Shift Shift { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
