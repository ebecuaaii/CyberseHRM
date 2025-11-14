using System;
using System.Collections.Generic;

namespace HRMCyberse.Models;

public partial class Positiontitle
{
    public int Id { get; set; }

    public string Titlename { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
