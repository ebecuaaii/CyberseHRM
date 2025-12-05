# Requirements Document

## Introduction

This document outlines the requirements for a comprehensive Branch Management and Automated Payroll System. The system enables multi-branch operations where each branch has its own WiFi validation, employee invitation workflow, and real-time salary calculation based on attendance.

## Glossary

- **Branch**: A physical location of the company with unique identifier and WiFi configuration
- **Branch Code**: A unique alphanumeric code used by employees to join a specific branch during registration
- **WiFi Validation**: Process of verifying employee location by checking WiFi SSID/BSSID against branch configuration
- **Salary Rate**: Hourly or daily wage rate assigned to an employee
- **Daily Salary**: Calculated salary for a single work shift based on check-in/check-out times
- **Accumulated Salary**: Running total of daily salaries within a pay period
- **Payroll Period**: Monthly cycle for salary calculation and payslip generation
- **Employee Invitation**: Process where admin/manager sends email with pre-configured employee details
- **System**: The HRM Cyberse application

## Requirements

### Requirement 1: Branch Management

**User Story:** As an admin, I want to manage company branches, so that I can organize employees by location and validate their attendance based on WiFi.

#### Acceptance Criteria

1. WHEN an admin creates a branch THEN the System SHALL generate a unique branch code and store branch details
2. WHEN creating a branch THEN the System SHALL require branch name, location address, and at least one WiFi configuration
3. WHEN an admin assigns WiFi to a branch THEN the System SHALL store WiFi SSID and optionally BSSID for location validation
4. WHEN an admin views branches THEN the System SHALL display all branches with their codes, locations, and active status
5. WHEN an admin deactivates a branch THEN the System SHALL prevent new employee registrations but maintain existing employee data

### Requirement 2: Employee Registration with Branch Code

**User Story:** As a new employee, I want to register using a branch code, so that I am automatically assigned to the correct branch and location.

#### Acceptance Criteria

1. WHEN an employee registers THEN the System SHALL require a valid branch code
2. WHEN an employee enters a branch code THEN the System SHALL validate the code exists and the branch is active
3. WHEN registration is successful THEN the System SHALL associate the employee with the branch
4. WHEN an invalid branch code is entered THEN the System SHALL reject registration and display an error message
5. WHEN an employee is assigned to a branch THEN the System SHALL inherit the branch's WiFi validation rules

### Requirement 3: Employee Invitation System

**User Story:** As an admin or manager, I want to invite employees via email with pre-configured details, so that new hires can quickly join with all information set up.

#### Acceptance Criteria

1. WHEN an admin sends an invitation THEN the System SHALL create an invitation record with email, role, department, position, and salary rate
2. WHEN an invitation is created THEN the System SHALL send an email containing the branch code and registration link
3. WHEN an employee registers using an invitation THEN the System SHALL auto-populate role, department, position, and salary rate
4. WHEN an invitation expires THEN the System SHALL prevent registration using that invitation
5. WHEN an invitation is used THEN the System SHALL mark it as consumed and prevent reuse

### Requirement 4: WiFi-Based Attendance Validation

**User Story:** As an employee, I want my check-in to be validated against branch WiFi, so that attendance is only recorded when I am physically at the workplace.

#### Acceptance Criteria

1. WHEN an employee checks in THEN the System SHALL validate the WiFi SSID or BSSID matches the branch configuration
2. WHEN WiFi validation fails THEN the System SHALL reject check-in and display location error message
3. WHEN WiFi validation succeeds THEN the System SHALL record attendance with timestamp
4. WHEN a branch has multiple WiFi networks THEN the System SHALL accept any configured network
5. WHEN WiFi data is unavailable THEN the System SHALL log a warning but allow check-in for backward compatibility

### Requirement 5: Real-Time Daily Salary Calculation

**User Story:** As an employee, I want my daily salary calculated automatically after check-out, so that I can track my earnings in real-time.

#### Acceptance Criteria

1. WHEN an employee checks out THEN the System SHALL calculate work hours from check-in to check-out time
2. WHEN work hours are calculated THEN the System SHALL multiply hours by the employee's salary rate to compute daily salary
3. WHEN daily salary is calculated THEN the System SHALL store it in the attendance record
4. WHEN an employee has shift assignments THEN the System SHALL use shift duration for salary calculation
5. WHEN overtime occurs THEN the System SHALL calculate additional compensation based on overtime rules

### Requirement 6: Accumulated Salary Tracking

**User Story:** As an employee, I want to view my accumulated salary for the current month, so that I know my expected earnings before payday.

#### Acceptance Criteria

1. WHEN an employee views salary dashboard THEN the System SHALL display total accumulated salary for current month
2. WHEN calculating accumulated salary THEN the System SHALL sum all daily salaries within the current pay period
3. WHEN bonuses or penalties exist THEN the System SHALL include them in the accumulated total
4. WHEN an employee checks their salary THEN the System SHALL show breakdown by date with daily amounts
5. WHEN the pay period ends THEN the System SHALL freeze the accumulated amount for payroll processing

### Requirement 7: Automated Monthly Payroll Generation

**User Story:** As an admin, I want the system to automatically generate monthly payroll, so that salary processing is accurate and efficient.

#### Acceptance Criteria

1. WHEN the month ends THEN the System SHALL automatically calculate total hours worked for each employee
2. WHEN calculating payroll THEN the System SHALL sum all daily salaries for the month
3. WHEN generating payroll THEN the System SHALL include base salary, bonuses, and penalties
4. WHEN payroll is generated THEN the System SHALL create a payroll record with net salary
5. WHEN payroll is finalized THEN the System SHALL mark it as processed and prevent modifications

### Requirement 8: Employee Payslip Access

**User Story:** As an employee, I want to view my monthly payslip, so that I can verify my salary calculation and deductions.

#### Acceptance Criteria

1. WHEN an employee requests a payslip THEN the System SHALL display payroll details for the specified month
2. WHEN displaying a payslip THEN the System SHALL show total hours, base salary, bonuses, penalties, and net salary
3. WHEN a payslip is viewed THEN the System SHALL include daily salary breakdown for the month
4. WHEN no payroll exists for a month THEN the System SHALL display a message indicating no data available
5. WHEN an employee downloads a payslip THEN the System SHALL generate a PDF with all salary details

### Requirement 9: Salary Rate Management

**User Story:** As an admin, I want to set and update employee salary rates, so that salary calculations reflect current compensation agreements.

#### Acceptance Criteria

1. WHEN an admin sets a salary rate THEN the System SHALL store it in the employee profile
2. WHEN a salary rate is updated THEN the System SHALL apply the new rate to future attendance records
3. WHEN a salary rate changes THEN the System SHALL maintain historical rates for past calculations
4. WHEN calculating daily salary THEN the System SHALL use the salary rate effective on the work date
5. WHEN a salary rate is zero or negative THEN the System SHALL reject the value and display an error

### Requirement 10: Branch-Specific Reporting

**User Story:** As an admin, I want to view payroll reports by branch, so that I can analyze labor costs per location.

#### Acceptance Criteria

1. WHEN an admin requests a branch report THEN the System SHALL display total payroll costs for that branch
2. WHEN generating branch reports THEN the System SHALL include employee count and average salary
3. WHEN comparing branches THEN the System SHALL show payroll costs side by side
4. WHEN filtering by date range THEN the System SHALL calculate costs for the specified period
5. WHEN exporting branch reports THEN the System SHALL generate CSV or Excel files with detailed data
