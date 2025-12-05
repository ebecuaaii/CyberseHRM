-- =====================================================
-- Branch Management and Payroll Enhancement Schema
-- =====================================================

-- 1. Create Branches table
CREATE TABLE IF NOT EXISTS branches (
    id SERIAL PRIMARY KEY,
    branch_code VARCHAR(20) UNIQUE NOT NULL,
    branch_name VARCHAR(100) NOT NULL,
    location_address TEXT,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

-- 2. Update CompanyWifiLocations to link with branches
ALTER TABLE companywifilocations
ADD COLUMN IF NOT EXISTS branch_id INTEGER REFERENCES branches (id) ON DELETE CASCADE;

-- 3. Create Employee Invitations table
CREATE TABLE IF NOT EXISTS employee_invitations (
    id SERIAL PRIMARY KEY,
    email VARCHAR(100) NOT NULL,
    branch_id INTEGER NOT NULL REFERENCES branches (id) ON DELETE CASCADE,
    roleid INTEGER REFERENCES roles (id),
    departmentid INTEGER REFERENCES departments (id),
    positionid INTEGER REFERENCES positiontitles (id),
    salaryrate DECIMAL(10, 2),
    invitation_token VARCHAR(255) UNIQUE NOT NULL,
    expires_at TIMESTAMP NOT NULL,
    is_used BOOLEAN DEFAULT false,
    used_at TIMESTAMP,
    created_by INTEGER REFERENCES users (id),
    created_at TIMESTAMP DEFAULT NOW()
);

-- 4. Add branch_id to users table
ALTER TABLE users
ADD COLUMN IF NOT EXISTS branch_id INTEGER REFERENCES branches (id) ON DELETE SET NULL;

-- 5. Add salaryrate to payroll table
ALTER TABLE payroll
ADD COLUMN IF NOT EXISTS salaryrate numeric(12, 2);

-- 6. Add accumulated salary tracking to payroll
ALTER TABLE payroll
ADD COLUMN IF NOT EXISTS is_finalized BOOLEAN DEFAULT false,
ADD COLUMN IF NOT EXISTS finalized_at TIMESTAMP;

-- 6.1. Create attendance_payroll table
CREATE TABLE IF NOT EXISTS attendance_payroll (
    id SERIAL PRIMARY KEY,
    attendanceid INTEGER REFERENCES attendance (id),
    userid INTEGER REFERENCES users (id),
    shiftid INTEGER REFERENCES shifts (id),
    salaryrate numeric(12, 2),
    shiftmultiplier numeric(5, 2),
    effectiverate numeric(12, 2),
    hoursworked numeric(10, 2),
    overtimehours numeric(10, 2),
    overtimerate numeric(12, 2),
    regularamount numeric(12, 2),
    overtimeamount numeric(12, 2),
    totalamount numeric(12, 2),
    createdat TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

COMMENT ON TABLE attendance_payroll IS 'Chi tiết tính lương theo từng ca làm việc';

COMMENT ON COLUMN attendance_payroll.salaryrate IS 'Lương cơ bản theo giờ của nhân viên';

COMMENT ON COLUMN attendance_payroll.shiftmultiplier IS 'Hệ số nhân cho ca (1.0 = ca thường, 1.5 = ca đêm, 2.0 = ngày lễ)';

COMMENT ON COLUMN attendance_payroll.effectiverate IS 'Lương thực tế = salaryrate × shiftmultiplier';

COMMENT ON COLUMN attendance_payroll.hoursworked IS 'Số giờ làm thực tế';

COMMENT ON COLUMN attendance_payroll.overtimehours IS 'Giờ làm thêm (nếu có)';

COMMENT ON COLUMN attendance_payroll.overtimerate IS 'Lương OT (thường = effectiverate × 1.5)';

COMMENT ON COLUMN attendance_payroll.regularamount IS 'hoursworked × effectiverate';

COMMENT ON COLUMN attendance_payroll.overtimeamount IS 'overtimehours × overtimerate';

COMMENT ON COLUMN attendance_payroll.totalamount IS 'regularamount + overtimeamount';

-- 7. Create indexes for performance
-- Branch indexes
CREATE INDEX IF NOT EXISTS idx_branches_code ON branches (branch_code);

CREATE INDEX IF NOT EXISTS idx_branches_active ON branches (is_active);

-- WiFi location index
CREATE INDEX IF NOT EXISTS idx_wifi_branch ON companywifilocations (branch_id);

-- Employee invitation indexes
CREATE INDEX IF NOT EXISTS idx_invitations_email ON employee_invitations (email);

CREATE INDEX IF NOT EXISTS idx_invitations_token ON employee_invitations (invitation_token);

CREATE INDEX IF NOT EXISTS idx_invitations_branch ON employee_invitations (branch_id);

-- User index
CREATE INDEX IF NOT EXISTS idx_users_branch ON users (branch_id);

-- Attendance payroll indexes
CREATE INDEX IF NOT EXISTS idx_attendance_payroll_user ON attendance_payroll (userid);

CREATE INDEX IF NOT EXISTS idx_attendance_payroll_attendance ON attendance_payroll (attendanceid);

CREATE INDEX IF NOT EXISTS idx_attendance_payroll_shift ON attendance_payroll (shiftid);

CREATE INDEX IF NOT EXISTS idx_attendance_payroll_created ON attendance_payroll (createdat);

-- Payroll indexes
CREATE INDEX IF NOT EXISTS idx_payroll_user_month ON payroll (userid, month, year);

CREATE INDEX IF NOT EXISTS idx_payroll_finalized ON payroll (is_finalized);

-- 8. Create views for salary reporting
-- View: Lương tháng hiện tại của từng nhân viên
CREATE OR REPLACE VIEW v_current_month_salary AS
SELECT
    ap.userid,
    u.fullname,
    u.email,
    u.branch_id,
    b.branch_name,
    COUNT(*) as days_worked,
    SUM(ap.hoursworked) as total_hours,
    SUM(ap.overtimehours) as total_overtime_hours,
    SUM(ap.regularamount) as regular_salary,
    SUM(ap.overtimeamount) as overtime_salary,
    SUM(ap.totalamount) as total_salary,
    AVG(ap.salaryrate) as avg_salary_rate
FROM
    attendance_payroll ap
    JOIN users u ON ap.userid = u.id
    LEFT JOIN branches b ON u.branch_id = b.id
WHERE
    EXTRACT(
        YEAR
        FROM ap.createdat
    ) = EXTRACT(
        YEAR
        FROM CURRENT_DATE
    )
    AND EXTRACT(
        MONTH
        FROM ap.createdat
    ) = EXTRACT(
        MONTH
        FROM CURRENT_DATE
    )
GROUP BY
    ap.userid,
    u.fullname,
    u.email,
    u.branch_id,
    b.branch_name;

-- View: Lương theo tháng/năm (lịch sử)
CREATE OR REPLACE VIEW v_employee_monthly_salary AS
SELECT
    ap.userid,
    u.fullname,
    u.branch_id,
    b.branch_name,
    EXTRACT(
        YEAR
        FROM ap.createdat
    ) as year,
    EXTRACT(
        MONTH
        FROM ap.createdat
    ) as month,
    COUNT(*) as total_days_worked,
    SUM(ap.hoursworked) as total_hours,
    SUM(ap.overtimehours) as total_overtime_hours,
    SUM(ap.regularamount) as regular_salary,
    SUM(ap.overtimeamount) as overtime_salary,
    SUM(ap.totalamount) as total_salary,
    AVG(ap.salaryrate) as avg_salary_rate
FROM
    attendance_payroll ap
    JOIN users u ON ap.userid = u.id
    LEFT JOIN branches b ON u.branch_id = b.id
GROUP BY
    ap.userid,
    u.fullname,
    u.branch_id,
    b.branch_name,
    EXTRACT(
        YEAR
        FROM ap.createdat
    ),
    EXTRACT(
        MONTH
        FROM ap.createdat
    );

-- View: Chi tiết lương theo ca (để kiểm tra)
CREATE OR REPLACE VIEW v_attendance_payroll_details AS
SELECT
    ap.id,
    ap.attendanceid,
    a.checkintime,
    a.checkouttime,
    ap.userid,
    u.fullname,
    u.branch_id,
    b.branch_name,
    s."name",
    ap.salaryrate,
    ap.shiftmultiplier,
    ap.effectiverate,
    ap.hoursworked,
    ap.overtimehours,
    ap.regularamount,
    ap.overtimeamount,
    ap.totalamount,
    ap.createdat
FROM
    attendance_payroll ap
    JOIN attendance a ON ap.attendanceid = a.id
    JOIN users u ON ap.userid = u.id
    LEFT JOIN branches b ON u.branch_id = b.id
    LEFT JOIN shifts s ON ap.shiftid = s.id;

-- 9. Insert sample branches
INSERT INTO
    branches (
        branch_code,
        branch_name,
        location_address,
        is_active
    )
VALUES (
        'HUTECHAB',
        'SAIGON CAMPUS - BRANCH 1',
        'DIEN BIEN PHU, BINH THANH District, Ho Chi Minh City',
        true
    ),
    (
        'HUTECHKHUE',
        'THU DUC CAMPUS - BRANCH 2',
        'E1, HIGH-TECH PARK, Ho Chi Minh City',
        true
    ),
    (
        'KEYBOX',
        'KEYBOX KAFE - BRANCH 3',
        'KEYBOX KAFE, LUONG DINH CUA, Ho Chi Minh City',
        true
    ),
    (
        'MYPHONE',
        'MYPHONE HOTSPOT - BRANCH 4',
        'MYPHONE HOTSPOT, LUONG DINH CUA, Ho Chi Minh City',
        true
    )
ON CONFLICT (branch_code) DO NOTHING;

-- 10. Create utility functions
CREATE OR REPLACE FUNCTION generate_branch_code()
RETURNS VARCHAR(20) AS $$
DECLARE
    new_code VARCHAR(20);
    code_exists BOOLEAN;
BEGIN
    LOOP
        new_code := 'BR' || LPAD((SELECT COALESCE(MAX(CAST(SUBSTRING(branch_code FROM 3) AS INTEGER)), 0) + 1 FROM branches WHERE branch_code LIKE 'BR%')::TEXT, 3, '0');
        SELECT EXISTS(SELECT 1 FROM branches WHERE branch_code = new_code) INTO code_exists;
        EXIT WHEN NOT code_exists;
    END LOOP;
    RETURN new_code;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION generate_invitation_token()
RETURNS VARCHAR(255) AS $$
BEGIN
    RETURN encode(gen_random_bytes(32), 'hex');
END;
$$ LANGUAGE plpgsql;

-- 11. Add table comments
COMMENT ON TABLE branches IS 'Company branches/locations';

COMMENT ON TABLE employee_invitations IS 'Employee invitation system with pre-configured details';

select * from branches;

select * from companywifilocations;

UPDATE companywifilocations
SET
    branch_id = 3
WHERE
    wifissid = 'KEYBOX_CN1';

SELECT w.id, w.locationname, w.wifissid, w.branch_id, b.branch_name
FROM
    companywifilocations w
    LEFT JOIN branches b ON w.branch_id = b.id;

select * from positiontitles;