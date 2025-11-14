using System;
using System.Collections.Generic;

namespace HRMCyberse.Models;

public partial class Performancereview
{
    public int Id { get; set; }

    public int? Userid { get; set; }

    public int? Reviewerid { get; set; }

    public DateOnly? Reviewdate { get; set; }

    public decimal? Score { get; set; }

    public string? Comments { get; set; }

    public string? Period { get; set; }

    public int? Rewardpenaltyid { get; set; }

    public DateTime? Createdat { get; set; }

    public virtual User? Reviewer { get; set; }

    public virtual Rewardpenalty? Rewardpenalty { get; set; }

    public virtual User? User { get; set; }
}
