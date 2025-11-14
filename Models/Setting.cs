using System;
using System.Collections.Generic;

namespace HRMCyberse.Models;

public partial class Setting
{
    public int Id { get; set; }

    public string? Key { get; set; }

    public string? Value { get; set; }

    public string? Category { get; set; }

    public DateTime? Createdat { get; set; }
}
