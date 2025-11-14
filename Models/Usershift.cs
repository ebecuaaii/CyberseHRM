using System;
using System.Collections.Generic;

namespace HRMCyberse.Models;

public partial class Usershift
{
    public int Id { get; set; }

    public int Userid { get; set; }

    public int Shiftid { get; set; }

    public DateOnly Shiftdate { get; set; }

    public string? Status { get; set; }

    public DateTime? Createdat { get; set; }

    public virtual Shift Shift { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
