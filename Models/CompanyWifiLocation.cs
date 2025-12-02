using System;

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
}
