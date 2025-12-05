using System;
using System.Collections.Generic;

namespace HRMCyberse.Models;

public partial class Shift
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public TimeOnly Starttime { get; set; }

    public TimeOnly Endtime { get; set; }

    public int? Durationminutes { get; set; }

    public int? Createdby { get; set; }

    public DateTime? Createdat { get; set; }

    public virtual ICollection<AttendancePayroll> AttendancePayrolls { get; set; } = new List<AttendancePayroll>();

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual User? CreatedbyNavigation { get; set; }

    public virtual ICollection<Laterequest> Laterequests { get; set; } = new List<Laterequest>();

    public virtual ICollection<Shiftregistration> Shiftregistrations { get; set; } = new List<Shiftregistration>();

    public virtual ICollection<Shiftrequest> Shiftrequests { get; set; } = new List<Shiftrequest>();

    public virtual ICollection<Usershift> Usershifts { get; set; } = new List<Usershift>();
}
