using System;
using System.Collections.Generic;

namespace HRMCyberse.Models;

public partial class CompanyWifiLocation
{
    public int Id { get; set; }

    public string LocationName { get; set; } = null!;

    public string WifiSsid { get; set; } = null!;

    public string? WifiBssid { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? BranchId { get; set; }

    public virtual Branch? Branch { get; set; }
}
