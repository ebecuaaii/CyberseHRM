using System;
using System.Collections.Generic;

namespace HRMCyberse.Models;

/// <summary>
/// Yêu cầu đăng ký lịch làm việc theo tuần của nhân viên
/// </summary>
public partial class WeeklyscheduleRequest
{
    public int Id { get; set; }

    public int Userid { get; set; }

    public DateOnly WeekStartDate { get; set; }

    public DateOnly WeekEndDate { get; set; }

    public string? Status { get; set; }

    /// <summary>
    /// JSON chứa thông tin ca có thể làm theo từng ngày trong tuần
    /// </summary>
    public string AvailabilityData { get; set; } = null!;

    public string? Note { get; set; }

    public int? ReviewedBy { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User? ReviewedByNavigation { get; set; }

    public virtual User User { get; set; } = null!;
}
