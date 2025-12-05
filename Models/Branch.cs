using System;
using System.Collections.Generic;

namespace HRMCyberse.Models;

/// <summary>
/// Company branches/locations
/// </summary>
public partial class Branch
{
    public int Id { get; set; }

    public string BranchCode { get; set; } = null!;

    public string BranchName { get; set; } = null!;

    public string? LocationAddress { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<CompanyWifiLocation> CompanyWifiLocations { get; set; } = new List<CompanyWifiLocation>();

    public virtual ICollection<EmployeeInvitation> EmployeeInvitations { get; set; } = new List<EmployeeInvitation>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
