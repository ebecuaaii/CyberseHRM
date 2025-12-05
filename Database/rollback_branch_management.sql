-- =====================================================
-- Rollback Branch Management Schema Changes
-- =====================================================

-- Drop views
DROP VIEW IF EXISTS v_current_month_salary;

DROP VIEW IF EXISTS v_employee_monthly_salary;

-- Drop triggers and functions
DROP TRIGGER IF EXISTS trigger_calculate_daily_salary ON attendance;

DROP FUNCTION IF EXISTS calculate_daily_salary ();

DROP FUNCTION IF EXISTS generate_branch_code ();

DROP FUNCTION IF EXISTS generate_invitation_token ();

-- Remove columns from attendance
ALTER TABLE attendance
DROP COLUMN IF EXISTS daily_salary,
DROP COLUMN IF EXISTS work_hours,
DROP COLUMN IF EXISTS salary_rate_used;

-- Remove columns from payroll
ALTER TABLE payroll
DROP COLUMN IF EXISTS is_finalized,
DROP COLUMN IF EXISTS finalized_at;

-- Remove branch_id from users
ALTER TABLE users DROP COLUMN IF EXISTS branch_id;

-- Remove branch_id from companywifilocations
ALTER TABLE companywifilocations DROP COLUMN IF EXISTS branch_id;

-- Drop tables
DROP TABLE IF EXISTS employee_invitations;

DROP TABLE IF EXISTS branches;

-- Drop indexes (if they still exist)
DROP INDEX IF EXISTS idx_branches_code;

DROP INDEX IF EXISTS idx_branches_active;

DROP INDEX IF EXISTS idx_wifi_branch;

DROP INDEX IF EXISTS idx_invitations_email;

DROP INDEX IF EXISTS idx_invitations_token;

DROP INDEX IF EXISTS idx_invitations_branch;

DROP INDEX IF EXISTS idx_users_branch;

DROP INDEX IF EXISTS idx_attendance_daily_salary;