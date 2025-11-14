using System;
using System.Collections.Generic;

namespace HRMCyberse.Models;

public partial class Shiftregistration
{
    public int Id { get; set; }

    public int? Userid { get; set; }

    public int? Shiftid { get; set; }

    public DateTime? Registrationdate { get; set; }

    public string? Status { get; set; }

    public int? Approvedby { get; set; }

    public virtual User? ApprovedbyNavigation { get; set; }

    public virtual Shift? Shift { get; set; }

    public virtual User? User { get; set; }
}
