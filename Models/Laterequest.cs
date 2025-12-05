using System;
using System.Collections.Generic;

namespace HRMCyberse.Models;

/// <summary>
/// Stores employee late arrival requests
/// </summary>
public partial class Laterequest
{
    public int Id { get; set; }

    public int Userid { get; set; }

    public int Shiftid { get; set; }

    public DateOnly Requestdate { get; set; }

    /// <summary>
    /// Expected time of arrival when requesting to be late
    /// </summary>
    public TimeOnly Expectedarrivaltime { get; set; }

    public string? Reason { get; set; }

    /// <summary>
    /// Request status: Pending, Approved, Rejected, Cancelled
    /// </summary>
    public string? Status { get; set; }

    public int? Reviewedby { get; set; }

    public DateTime? Reviewedat { get; set; }

    public DateTime? Createdat { get; set; }

    public virtual User? ReviewedbyNavigation { get; set; }

    public virtual Shift Shift { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
