using System;
using System.Collections.Generic;

namespace HRMCyberse.Models;

public partial class Systemlog
{
    public int Id { get; set; }

    public string? Loglevel { get; set; }

    public string? Message { get; set; }

    public string? Stacktrace { get; set; }

    public DateTime? Createdat { get; set; }
}
