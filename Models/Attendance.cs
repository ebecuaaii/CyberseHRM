using System;
using System.Collections.Generic;

namespace HRMCyberse.Models;

public partial class Attendance
{
    public int Id { get; set; }

    public int Userid { get; set; }

    public int? Shiftid { get; set; }

    public DateTime? Checkintime { get; set; }

    public DateTime? Checkouttime { get; set; }

    public decimal? Checkinlat { get; set; }

    public decimal? Checkinlng { get; set; }

    public decimal? Checkoutlat { get; set; }

    public decimal? Checkoutlng { get; set; }

    public string? Checkinimageurl { get; set; }

    public string? Checkoutimageurl { get; set; }

    public string? Status { get; set; }

    public string? Notes { get; set; }

    public virtual ICollection<Attendanceimage> Attendanceimages { get; set; } = new List<Attendanceimage>();

    public virtual Shift? Shift { get; set; }

    public virtual User User { get; set; } = null!;
}
