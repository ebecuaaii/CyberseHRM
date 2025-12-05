using System;
using System.Collections.Generic;

namespace HRMCyberse.Models;

/// <summary>
/// Employee invitation system with pre-configured details
/// </summary>
public partial class EmployeeInvitation
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public int BranchId { get; set; }

    public int? Roleid { get; set; }

    public int? Departmentid { get; set; }

    public int? Positionid { get; set; }

    public decimal? Salaryrate { get; set; }

    public string InvitationToken { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public bool? IsUsed { get; set; }

    public DateTime? UsedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual User? CreatedByNavigation { get; set; }

    public virtual Department? Department { get; set; }

    public virtual Positiontitle? Position { get; set; }

    public virtual Role? Role { get; set; }
}
