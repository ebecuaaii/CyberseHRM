using System;
using System.Collections.Generic;

namespace HRMCyberse.Models;

public partial class Attendanceimage
{
    public int Id { get; set; }

    public int Attendanceid { get; set; }

    public string Imageurl { get; set; } = null!;

    public string? Type { get; set; }

    public virtual Attendance Attendance { get; set; } = null!;
}
