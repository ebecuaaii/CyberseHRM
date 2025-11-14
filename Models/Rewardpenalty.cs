using System;
using System.Collections.Generic;

namespace HRMCyberse.Models;

public partial class Rewardpenalty
{
    public int Id { get; set; }

    public int Userid { get; set; }

    public string Type { get; set; } = null!;

    public decimal Amount { get; set; }

    public string? Reason { get; set; }

    public int? Createdby { get; set; }

    public DateTime? Createdat { get; set; }

    public virtual User? CreatedbyNavigation { get; set; }

    public virtual ICollection<Performancereview> Performancereviews { get; set; } = new List<Performancereview>();

    public virtual User User { get; set; } = null!;
}
