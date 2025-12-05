using System;
using System.Collections.Generic;

namespace HRMCyberse.Models;

public partial class Role
{
    public int Id { get; set; }

    public string Rolename { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<EmployeeInvitation> EmployeeInvitations { get; set; } = new List<EmployeeInvitation>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
