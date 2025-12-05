# Branch Management Database Migration Guide

## Overview
This guide explains how to apply the branch management and automated payroll schema changes to your database.

## Prerequisites
- PostgreSQL database access
- Connection string configured in `appsettings.json`
- EF Core tools installed: `dotnet tool install --global dotnet-ef`

## Step 1: Backup Current Database
```bash
# Create backup before migration
pg_dump -h your-host -U your-user -d cybersehrm > backup_before_branch_$(date +%Y%m%d).sql
```

## Step 2: Apply Database Schema Changes

### Option A: Using psql
```bash
psql -h dpg-d415mvbe5dus738lqdgg-a.oregon-postgres.render.com \
     -U cybersehrm_user \
     -d cybersehrm \
     -f Database/add_branch_management.sql
```

### Option B: Using pgAdmin or Database Client
1. Open your database client
2. Connect to `cybersehrm` database
3. Open and execute `Database/add_branch_management.sql`

## Step 3: Verify Schema Changes
```sql
-- Check if branches table exists
SELECT * FROM branches;

-- Check if new columns added to users
SELECT column_name FROM information_schema.columns 
WHERE table_name = 'users' AND column_name = 'branch_id';

-- Check if new columns added to attendance
SELECT column_name FROM information_schema.columns 
WHERE table_name = 'attendance' AND column_name IN ('daily_salary', 'work_hours', 'salary_rate_used');

-- Check if views created
SELECT * FROM v_current_month_salary LIMIT 5;
```

## Step 4: Scaffold New Models

### Update Connection String (if needed)
Edit `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=dpg-d415mvbe5dus738lqdgg-a.oregon-postgres.render.com;Port=5432;Database=cybersehrm;Username=cybersehrm_user;Password=tL34pBRebfa8gGmYUp1WVAI1nh9ouh2u;SSL Mode=Require;"
  }
}
```

### Run Scaffold Command
```bash
# Navigate to project directory
cd C:\Users\PC\source\repos\HRMCyberse

# Scaffold database (this will regenerate all models)
dotnet ef dbcontext scaffold "Host=dpg-d415mvbe5dus738lqdgg-a.oregon-postgres.render.com;Port=5432;Database=cybersehrm;Username=cybersehrm_user;Password=tL34pBRebfa8gGmYUp1WVAI1nh9ouh2u;SSL Mode=Require;" Npgsql.EntityFrameworkCore.PostgreSQL -o Models -c CybersehrmContext -d Data --force
```

**Note:** The `--force` flag will overwrite existing model files. Make sure you've backed up any custom changes.

## Step 5: Verify Generated Models

Check that these new models were created:
- `Models/Branch.cs`
- `Models/EmployeeInvitation.cs`

Check that these models were updated:
- `Models/User.cs` - should have `BranchId` property
- `Models/Attendance.cs` - should have `DailySalary`, `WorkHours`, `SalaryRateUsed` properties
- `Models/CompanyWifiLocation.cs` - should have `BranchId` property
- `Models/Payroll.cs` - should have `IsFinalized`, `FinalizedAt` properties

## Step 6: Update DbContext

The `Data/CybersehrmContext.cs` should now include:
```csharp
public virtual DbSet<Branch> Branches { get; set; }
public virtual DbSet<EmployeeInvitation> EmployeeInvitations { get; set; }
```

## Step 7: Build and Test
```bash
# Build the project
dotnet build

# Run the application
dotnet run
```

## Rollback (if needed)

If something goes wrong, you can rollback:
```bash
psql -h dpg-d415mvbe5dus738lqdgg-a.oregon-postgres.render.com \
     -U cybersehrm_user \
     -d cybersehrm \
     -f Database/rollback_branch_management.sql
```

Then restore from backup:
```bash
psql -h your-host -U your-user -d cybersehrm < backup_before_branch_YYYYMMDD.sql
```

## What's New

### Tables
1. **branches** - Store company branch information
2. **employee_invitations** - Manage employee invitation workflow

### Columns Added
- `users.branch_id` - Link user to branch
- `companywifilocations.branch_id` - Link WiFi to branch
- `attendance.daily_salary` - Auto-calculated daily salary
- `attendance.work_hours` - Calculated work hours
- `attendance.salary_rate_used` - Historical salary rate
- `payroll.is_finalized` - Payroll finalization status
- `payroll.finalized_at` - Finalization timestamp

### Database Functions
- `calculate_daily_salary()` - Auto-calculates salary on checkout
- `generate_branch_code()` - Generates unique branch codes
- `generate_invitation_token()` - Generates secure invitation tokens

### Views
- `v_employee_monthly_salary` - Monthly salary aggregation
- `v_current_month_salary` - Current month salary tracking

## Testing the Changes

### Test 1: Create a Branch
```sql
INSERT INTO branches (branch_code, branch_name, location_address) 
VALUES ('TEST01', 'Test Branch', '123 Test Street');
```

### Test 2: Assign User to Branch
```sql
UPDATE users SET branch_id = (SELECT id FROM branches WHERE branch_code = 'TEST01') 
WHERE id = 1;
```

### Test 3: Test Auto Salary Calculation
```sql
-- Insert attendance with checkout
INSERT INTO attendance (userid, checkintime, checkouttime, status)
VALUES (1, NOW() - INTERVAL '8 hours', NOW(), 'Present');

-- Check if daily_salary was calculated
SELECT userid, work_hours, salary_rate_used, daily_salary 
FROM attendance 
WHERE userid = 1 
ORDER BY checkintime DESC 
LIMIT 1;
```

### Test 4: View Current Month Salary
```sql
SELECT * FROM v_current_month_salary WHERE userid = 1;
```

## Troubleshooting

### Issue: Scaffold fails with connection error
**Solution:** Check your connection string and ensure database is accessible

### Issue: Models not generated
**Solution:** Make sure EF Core tools are installed: `dotnet tool install --global dotnet-ef`

### Issue: Trigger not working
**Solution:** Check if function exists: `SELECT * FROM pg_proc WHERE proname = 'calculate_daily_salary';`

### Issue: Views return no data
**Solution:** Ensure you have attendance records with both checkin and checkout times

## Next Steps

After successful migration:
1. Review the requirements document: `.kiro/specs/branch-management-payroll/requirements.md`
2. Implement API controllers for branch management
3. Implement employee invitation system
4. Update attendance controller to use auto-calculated salaries
5. Create payroll generation endpoints
