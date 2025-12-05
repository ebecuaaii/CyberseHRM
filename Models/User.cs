using System;
using System.Collections.Generic;

namespace HRMCyberse.Models;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Passwordhash { get; set; } = null!;

    public string Fullname { get; set; } = null!;

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public int? Roleid { get; set; }

    public int? Departmentid { get; set; }

    public int? Positionid { get; set; }

    public decimal? Salaryrate { get; set; }

    public DateOnly? Hiredate { get; set; }

    public string? Avatarurl { get; set; }

    public bool? Isactive { get; set; }

    public DateTime? Createdat { get; set; }

    public int? BranchId { get; set; }

    public virtual ICollection<Activitylog> Activitylogs { get; set; } = new List<Activitylog>();

    public virtual ICollection<AttendancePayroll> AttendancePayrolls { get; set; } = new List<AttendancePayroll>();

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual Branch? Branch { get; set; }

    public virtual Department? Department { get; set; }

    public virtual ICollection<EmployeeInvitation> EmployeeInvitations { get; set; } = new List<EmployeeInvitation>();

    public virtual ICollection<Laterequest> LaterequestReviewedbyNavigations { get; set; } = new List<Laterequest>();

    public virtual ICollection<Laterequest> LaterequestUsers { get; set; } = new List<Laterequest>();

    public virtual ICollection<Leaverequest> LeaverequestReviewedbyNavigations { get; set; } = new List<Leaverequest>();

    public virtual ICollection<Leaverequest> LeaverequestUsers { get; set; } = new List<Leaverequest>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Payroll> Payrolls { get; set; } = new List<Payroll>();

    public virtual ICollection<Performancereview> PerformancereviewReviewers { get; set; } = new List<Performancereview>();

    public virtual ICollection<Performancereview> PerformancereviewUsers { get; set; } = new List<Performancereview>();

    public virtual Positiontitle? Position { get; set; }

    public virtual ICollection<Rewardpenalty> RewardpenaltyCreatedbyNavigations { get; set; } = new List<Rewardpenalty>();

    public virtual ICollection<Rewardpenalty> RewardpenaltyUsers { get; set; } = new List<Rewardpenalty>();

    public virtual Role? Role { get; set; }

    public virtual ICollection<Salaryadjustment> SalaryadjustmentApprovedbyNavigations { get; set; } = new List<Salaryadjustment>();

    public virtual ICollection<Salaryadjustment> SalaryadjustmentUsers { get; set; } = new List<Salaryadjustment>();

    public virtual ICollection<Shiftregistration> ShiftregistrationApprovedbyNavigations { get; set; } = new List<Shiftregistration>();

    public virtual ICollection<Shiftregistration> ShiftregistrationUsers { get; set; } = new List<Shiftregistration>();

    public virtual ICollection<Shiftrequest> ShiftrequestReviewedbyNavigations { get; set; } = new List<Shiftrequest>();

    public virtual ICollection<Shiftrequest> ShiftrequestUsers { get; set; } = new List<Shiftrequest>();

    public virtual ICollection<Shift> Shifts { get; set; } = new List<Shift>();

    public virtual ICollection<Usershift> Usershifts { get; set; } = new List<Usershift>();

    public virtual ICollection<WeeklyscheduleRequest> WeeklyscheduleRequestReviewedByNavigations { get; set; } = new List<WeeklyscheduleRequest>();

    public virtual ICollection<WeeklyscheduleRequest> WeeklyscheduleRequestUsers { get; set; } = new List<WeeklyscheduleRequest>();
}
