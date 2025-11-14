using System;
using System.Collections.Generic;

namespace HRMCyberse.Models;

public partial class Activitylog
{
    public int Id { get; set; }

    public int? Userid { get; set; }

    public string? Action { get; set; }

    public string? Description { get; set; }

    public DateTime? Createdat { get; set; }

    public virtual User? User { get; set; }
}
