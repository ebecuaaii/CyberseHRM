using System;
using System.Collections.Generic;

namespace HRMCyberse.Models;

public partial class Holidaycalendar
{
    public int Id { get; set; }

    public DateOnly Holidaydate { get; set; }

    public string? Description { get; set; }

    public DateTime? Createdat { get; set; }

    public decimal? Multiplier { get; set; }
}
