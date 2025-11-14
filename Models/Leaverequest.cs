using System;
using System.Collections.Generic;

namespace HRMCyberse.Models;

public partial class Leaverequest
{
    public int Id { get; set; }

    public int Userid { get; set; }

    public DateOnly Startdate { get; set; }

    public DateOnly Enddate { get; set; }

    public string? Reason { get; set; }

    public string? Status { get; set; }

    public int? Reviewedby { get; set; }

    public DateTime? Reviewedat { get; set; }

    public DateTime? Createdat { get; set; }

    public virtual User? ReviewedbyNavigation { get; set; }

    public virtual User User { get; set; } = null!;
}
