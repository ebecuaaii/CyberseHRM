using System;
using System.Collections.Generic;
using HRMCyberse.Models;
using Microsoft.EntityFrameworkCore;

namespace HRMCyberse.Data;

public partial class CybersehrmContext : DbContext
{
    public CybersehrmContext()
    {
    }

    public CybersehrmContext(DbContextOptions<CybersehrmContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Activitylog> Activitylogs { get; set; }

    public virtual DbSet<Attendance> Attendances { get; set; }

    public virtual DbSet<Attendanceimage> Attendanceimages { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Holidaycalendar> Holidaycalendars { get; set; }

    public virtual DbSet<Laterequest> Laterequests { get; set; }

    public virtual DbSet<Leaverequest> Leaverequests { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Payroll> Payrolls { get; set; }

    public virtual DbSet<Performancereview> Performancereviews { get; set; }

    public virtual DbSet<Positiontitle> Positiontitles { get; set; }

    public virtual DbSet<Rewardpenalty> Rewardpenalties { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Salaryadjustment> Salaryadjustments { get; set; }

    public virtual DbSet<Salarydetail> Salarydetails { get; set; }

    public virtual DbSet<Setting> Settings { get; set; }

    public virtual DbSet<Shift> Shifts { get; set; }

    public virtual DbSet<Shiftregistration> Shiftregistrations { get; set; }

    public virtual DbSet<Shiftrequest> Shiftrequests { get; set; }

    public virtual DbSet<Systemlog> Systemlogs { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Usershift> Usershifts { get; set; }

    public virtual DbSet<WeeklyscheduleRequest> WeeklyscheduleRequests { get; set; }

    public virtual DbSet<CompanyWifiLocation> CompanyWifiLocations { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=dpg-d415mvbe5dus738lqdgg-a.oregon-postgres.render.com;Port=5432;Database=cybersehrm;Username=cybersehrm_user;Password=tL34pBRebfa8gGmYUp1WVAI1nh9ouh2u;SSL Mode=Require;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Activitylog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("activitylogs_pkey");

            entity.ToTable("activitylogs");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Action)
                .HasMaxLength(100)
                .HasColumnName("action");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnName("createdat");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.User).WithMany(p => p.Activitylogs)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("activitylogs_userid_fkey");
        });

        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("attendance_pkey");

            entity.ToTable("attendance");

            entity.HasIndex(e => new { e.Userid, e.Checkintime }, "idx_attendance_user_time");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Checkinimageurl).HasColumnName("checkinimageurl");
            entity.Property(e => e.Checkinlat)
                .HasPrecision(10, 7)
                .HasColumnName("checkinlat");
            entity.Property(e => e.Checkinlng)
                .HasPrecision(10, 7)
                .HasColumnName("checkinlng");
            entity.Property(e => e.Checkintime).HasColumnName("checkintime");
            entity.Property(e => e.Checkoutimageurl).HasColumnName("checkoutimageurl");
            entity.Property(e => e.Checkoutlat)
                .HasPrecision(10, 7)
                .HasColumnName("checkoutlat");
            entity.Property(e => e.Checkoutlng)
                .HasPrecision(10, 7)
                .HasColumnName("checkoutlng");
            entity.Property(e => e.Checkouttime).HasColumnName("checkouttime");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.Shiftid).HasColumnName("shiftid");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasColumnName("status");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.Shift).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.Shiftid)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("attendance_shiftid_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("attendance_userid_fkey");
        });

        modelBuilder.Entity<Attendanceimage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("attendanceimages_pkey");

            entity.ToTable("attendanceimages");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Attendanceid).HasColumnName("attendanceid");
            entity.Property(e => e.Imageurl).HasColumnName("imageurl");
            entity.Property(e => e.Type)
                .HasMaxLength(10)
                .HasColumnName("type");

            entity.HasOne(d => d.Attendance).WithMany(p => p.Attendanceimages)
                .HasForeignKey(d => d.Attendanceid)
                .HasConstraintName("attendanceimages_attendanceid_fkey");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("departments_pkey");

            entity.ToTable("departments");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnName("createdat");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Holidaycalendar>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("holidaycalendar_pkey");

            entity.ToTable("holidaycalendar");

            entity.HasIndex(e => e.Holidaydate, "holidaycalendar_holidaydate_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnName("createdat");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.Holidaydate).HasColumnName("holidaydate");
            entity.Property(e => e.Multiplier)
                .HasPrecision(3, 2)
                .HasDefaultValueSql("1.00")
                .HasColumnName("multiplier");
        });

        modelBuilder.Entity<Laterequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("laterequests_pkey");

            entity.ToTable("laterequests");

            entity.HasIndex(e => e.Userid, "idx_laterequests_userid");
            entity.HasIndex(e => e.Status, "idx_laterequests_status");
            entity.HasIndex(e => e.Requestdate, "idx_laterequests_requestdate");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Userid).HasColumnName("userid");
            entity.Property(e => e.Shiftid).HasColumnName("shiftid");
            entity.Property(e => e.Requestdate).HasColumnName("requestdate");
            entity.Property(e => e.Expectedarrivaltime).HasColumnName("expectedarrivaltime");
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Pending'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.Reviewedby).HasColumnName("reviewedby");
            entity.Property(e => e.Reviewedat).HasColumnName("reviewedat");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnName("createdat");

            entity.HasOne(d => d.ReviewedbyNavigation).WithMany(p => p.LaterequestReviewedbyNavigations)
                .HasForeignKey(d => d.Reviewedby)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("laterequests_reviewedby_fkey");

            entity.HasOne(d => d.Shift).WithMany(p => p.Laterequests)
                .HasForeignKey(d => d.Shiftid)
                .HasConstraintName("laterequests_shiftid_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.LaterequestUsers)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("laterequests_userid_fkey");
        });

        modelBuilder.Entity<Leaverequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("leaverequests_pkey");

            entity.ToTable("leaverequests");

            entity.HasIndex(e => new { e.Userid, e.Startdate, e.Enddate }, "idx_leave_user_date");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnName("createdat");
            entity.Property(e => e.Enddate).HasColumnName("enddate");
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.Reviewedat).HasColumnName("reviewedat");
            entity.Property(e => e.Reviewedby).HasColumnName("reviewedby");
            entity.Property(e => e.Startdate).HasColumnName("startdate");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Pending'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.ReviewedbyNavigation).WithMany(p => p.LeaverequestReviewedbyNavigations)
                .HasForeignKey(d => d.Reviewedby)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("leaverequests_reviewedby_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.LeaverequestUsers)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("leaverequests_userid_fkey");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("notifications_pkey");

            entity.ToTable("notifications");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnName("createdat");
            entity.Property(e => e.Isread)
                .HasDefaultValue(false)
                .HasColumnName("isread");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.Title)
                .HasMaxLength(100)
                .HasColumnName("title");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("notifications_userid_fkey");
        });

        modelBuilder.Entity<Payroll>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("payroll_pkey");

            entity.ToTable("payroll");

            entity.HasIndex(e => new { e.Userid, e.Month, e.Year }, "idx_usersalary_user_month");

            entity.HasIndex(e => new { e.Userid, e.Month, e.Year }, "payroll_userid_month_year_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Basesalary)
                .HasPrecision(12, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("basesalary");
            entity.Property(e => e.Bonuses)
                .HasPrecision(12, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("bonuses");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnName("createdat");
            entity.Property(e => e.Month).HasColumnName("month");
            entity.Property(e => e.Netsalary)
                .HasPrecision(12, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("netsalary");
            entity.Property(e => e.Penalties)
                .HasPrecision(12, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("penalties");
            entity.Property(e => e.Totalhours)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("totalhours");
            entity.Property(e => e.Userid).HasColumnName("userid");
            entity.Property(e => e.Year).HasColumnName("year");

            entity.HasOne(d => d.User).WithMany(p => p.Payrolls)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("payroll_userid_fkey");
        });

        modelBuilder.Entity<Performancereview>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("performancereview_pkey");

            entity.ToTable("performancereview");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Comments).HasColumnName("comments");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnName("createdat");
            entity.Property(e => e.Period)
                .HasMaxLength(20)
                .HasColumnName("period");
            entity.Property(e => e.Reviewdate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("reviewdate");
            entity.Property(e => e.Reviewerid).HasColumnName("reviewerid");
            entity.Property(e => e.Rewardpenaltyid).HasColumnName("rewardpenaltyid");
            entity.Property(e => e.Score)
                .HasPrecision(3, 2)
                .HasColumnName("score");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.Reviewer).WithMany(p => p.PerformancereviewReviewers)
                .HasForeignKey(d => d.Reviewerid)
                .HasConstraintName("performancereview_reviewerid_fkey");

            entity.HasOne(d => d.Rewardpenalty).WithMany(p => p.Performancereviews)
                .HasForeignKey(d => d.Rewardpenaltyid)
                .HasConstraintName("performancereview_rewardpenaltyid_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.PerformancereviewUsers)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("performancereview_userid_fkey");
        });

        modelBuilder.Entity<Positiontitle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("positiontitles_pkey");

            entity.ToTable("positiontitles");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Titlename)
                .HasMaxLength(100)
                .HasColumnName("titlename");
        });

        modelBuilder.Entity<Rewardpenalty>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("rewardpenalty_pkey");

            entity.ToTable("rewardpenalty");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasPrecision(12, 2)
                .HasColumnName("amount");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnName("createdat");
            entity.Property(e => e.Createdby).HasColumnName("createdby");
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.Type)
                .HasMaxLength(20)
                .HasColumnName("type");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.CreatedbyNavigation).WithMany(p => p.RewardpenaltyCreatedbyNavigations)
                .HasForeignKey(d => d.Createdby)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("rewardpenalty_createdby_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.RewardpenaltyUsers)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("rewardpenalty_userid_fkey");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("roles_pkey");

            entity.ToTable("roles");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Rolename)
                .HasMaxLength(50)
                .HasColumnName("rolename");
        });

        modelBuilder.Entity<Salaryadjustment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("salaryadjustments_pkey");

            entity.ToTable("salaryadjustments");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Approvedby).HasColumnName("approvedby");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnName("createdat");
            entity.Property(e => e.Newrate)
                .HasPrecision(10, 2)
                .HasColumnName("newrate");
            entity.Property(e => e.Oldrate)
                .HasPrecision(10, 2)
                .HasColumnName("oldrate");
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.ApprovedbyNavigation).WithMany(p => p.SalaryadjustmentApprovedbyNavigations)
                .HasForeignKey(d => d.Approvedby)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("salaryadjustments_approvedby_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.SalaryadjustmentUsers)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("salaryadjustments_userid_fkey");
        });

        modelBuilder.Entity<Salarydetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("salarydetails_pkey");

            entity.ToTable("salarydetails");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasPrecision(12, 2)
                .HasColumnName("amount");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnName("createdat");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.Payrollid).HasColumnName("payrollid");

            entity.HasOne(d => d.Payroll).WithMany(p => p.Salarydetails)
                .HasForeignKey(d => d.Payrollid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("salarydetails_payrollid_fkey");
        });

        modelBuilder.Entity<Setting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("settings_pkey");

            entity.ToTable("settings");

            entity.HasIndex(e => e.Key, "settings_key_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Category)
                .HasMaxLength(50)
                .HasColumnName("category");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnName("createdat");
            entity.Property(e => e.Key)
                .HasMaxLength(100)
                .HasColumnName("key");
            entity.Property(e => e.Value).HasColumnName("value");
        });

        modelBuilder.Entity<Shift>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("shifts_pkey");

            entity.ToTable("shifts");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnName("createdat");
            entity.Property(e => e.Createdby).HasColumnName("createdby");
            entity.Property(e => e.Durationminutes).HasColumnName("durationminutes");
            entity.Property(e => e.Endtime).HasColumnName("endtime");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.Starttime).HasColumnName("starttime");

            entity.HasOne(d => d.CreatedbyNavigation).WithMany(p => p.Shifts)
                .HasForeignKey(d => d.Createdby)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("shifts_createdby_fkey");
        });

        modelBuilder.Entity<Shiftregistration>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("shiftregistration_pkey");

            entity.ToTable("shiftregistration");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Approvedby).HasColumnName("approvedby");
            entity.Property(e => e.Registrationdate)
                .HasDefaultValueSql("now()")
                .HasColumnName("registrationdate");
            entity.Property(e => e.Shiftid).HasColumnName("shiftid");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Pending'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.ApprovedbyNavigation).WithMany(p => p.ShiftregistrationApprovedbyNavigations)
                .HasForeignKey(d => d.Approvedby)
                .HasConstraintName("shiftregistration_approvedby_fkey");

            entity.HasOne(d => d.Shift).WithMany(p => p.Shiftregistrations)
                .HasForeignKey(d => d.Shiftid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("shiftregistration_shiftid_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.ShiftregistrationUsers)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("shiftregistration_userid_fkey");
        });

        modelBuilder.Entity<Shiftrequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("shiftrequests_pkey");

            entity.ToTable("shiftrequests");

            entity.HasIndex(e => e.Status, "idx_shiftrequests_status");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnName("createdat");
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.Reviewedat).HasColumnName("reviewedat");
            entity.Property(e => e.Reviewedby).HasColumnName("reviewedby");
            entity.Property(e => e.Shiftdate).HasColumnName("shiftdate");
            entity.Property(e => e.Shiftid).HasColumnName("shiftid");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Pending'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.ReviewedbyNavigation).WithMany(p => p.ShiftrequestReviewedbyNavigations)
                .HasForeignKey(d => d.Reviewedby)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("shiftrequests_reviewedby_fkey");

            entity.HasOne(d => d.Shift).WithMany(p => p.Shiftrequests)
                .HasForeignKey(d => d.Shiftid)
                .HasConstraintName("shiftrequests_shiftid_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.ShiftrequestUsers)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("shiftrequests_userid_fkey");
        });

        modelBuilder.Entity<Systemlog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("systemlogs_pkey");

            entity.ToTable("systemlogs");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnName("createdat");
            entity.Property(e => e.Loglevel)
                .HasMaxLength(20)
                .HasColumnName("loglevel");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.Stacktrace).HasColumnName("stacktrace");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Departmentid, "idx_users_department");

            entity.HasIndex(e => e.Roleid, "idx_users_role");

            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.HasIndex(e => e.Username, "users_username_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Avatarurl).HasColumnName("avatarurl");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnName("createdat");
            entity.Property(e => e.Departmentid).HasColumnName("departmentid");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Fullname)
                .HasMaxLength(100)
                .HasColumnName("fullname");
            entity.Property(e => e.Hiredate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("hiredate");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.Passwordhash)
                .HasMaxLength(255)
                .HasColumnName("passwordhash");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.Positionid).HasColumnName("positionid");
            entity.Property(e => e.Roleid).HasColumnName("roleid");
            entity.Property(e => e.Salaryrate)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("salaryrate");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasColumnName("username");

            entity.HasOne(d => d.Department).WithMany(p => p.Users)
                .HasForeignKey(d => d.Departmentid)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("users_departmentid_fkey");

            entity.HasOne(d => d.Position).WithMany(p => p.Users)
                .HasForeignKey(d => d.Positionid)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("users_positionid_fkey");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.Roleid)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("users_roleid_fkey");
        });

        modelBuilder.Entity<Usershift>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("usershifts_pkey");

            entity.ToTable("usershifts");

            entity.HasIndex(e => e.Shiftdate, "idx_usershifts_date");

            entity.HasIndex(e => new { e.Userid, e.Shiftid, e.Shiftdate }, "usershifts_userid_shiftid_shiftdate_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnName("createdat");
            entity.Property(e => e.Shiftdate).HasColumnName("shiftdate");
            entity.Property(e => e.Shiftid).HasColumnName("shiftid");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Assigned'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.Shift).WithMany(p => p.Usershifts)
                .HasForeignKey(d => d.Shiftid)
                .HasConstraintName("usershifts_shiftid_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Usershifts)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("usershifts_userid_fkey");
        });

        // Configure WeeklyscheduleRequest relationships
        modelBuilder.Entity<WeeklyscheduleRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("weeklyschedule_requests");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Userid).HasColumnName("userid");
            entity.Property(e => e.WeekStartDate).HasColumnName("week_start_date");
            entity.Property(e => e.WeekEndDate).HasColumnName("week_end_date");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20);
            entity.Property(e => e.AvailabilityData).HasColumnName("availability_data").HasColumnType("jsonb");
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.ReviewedBy).HasColumnName("reviewed_by");
            entity.Property(e => e.ReviewedAt).HasColumnName("reviewed_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            // Configure User relationship (creator)
            entity.HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("weeklyschedule_requests_userid_fkey");

            // Configure ReviewedBy relationship
            entity.HasOne(d => d.ReviewedByNavigation)
                .WithMany()
                .HasForeignKey(d => d.ReviewedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("weeklyschedule_requests_reviewed_by_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
