# Requirements Document - Shift Management System

## Introduction

This document outlines the requirements for implementing a comprehensive shift management system that allows administrators and managers to create, manage, and assign work shifts to employees. The system serves as the foundation for attendance tracking and ensures proper workforce scheduling.

## Glossary

- **Shift_System**: The HRM Cyberse shift management module
- **Work_Shift**: A defined time period with start and end times for employee work
- **User_Shift_Assignment**: The relationship between an employee and their assigned work shifts
- **Admin_User**: User with administrative privileges (role: admin)
- **Manager_User**: User with management privileges (role: manager)  
- **Employee_User**: User with basic employee privileges (role: employee)
- **Shift_Schedule**: The collection of shifts assigned to an employee over a time period

## Requirements

### Requirement 1

**User Story:** As an Admin or Manager, I want to view all available work shifts in the system, so that I can understand the current shift structure and make informed scheduling decisions.

#### Acceptance Criteria

1. WHEN an Admin_User or Manager_User requests the shift list, THE Shift_System SHALL return all work shifts with their details
2. THE Shift_System SHALL display shift name, start time, end time, and description for each shift
3. THE Shift_System SHALL show the following predefined shifts: Ca1 (6:30-14:30), Ca2 (9:30-14:30), Ca3 (14:30-22:30), Ca4 (14:30-18:30), Ca5 (18:30-22:30), Ca đêm (22:30-6:30)
4. WHEN an Employee_User requests the shift list, THE Shift_System SHALL deny access with appropriate authorization error
5. THE Shift_System SHALL return shifts in a consistent time format (24-hour format)

### Requirement 2

**User Story:** As an Admin or Manager, I want to create new work shifts, so that I can accommodate different business needs and scheduling requirements.

#### Acceptance Criteria

1. WHEN an Admin_User or Manager_User submits a new shift with valid data, THE Shift_System SHALL create the shift successfully
2. THE Shift_System SHALL require shift name, start time, and end time as mandatory fields
3. THE Shift_System SHALL validate that start time is before end time for same-day shifts
4. THE Shift_System SHALL support overnight shifts where end time is next day
5. WHEN an Employee_User attempts to create a shift, THE Shift_System SHALL deny access with authorization error
6. THE Shift_System SHALL prevent duplicate shift names within the system

### Requirement 3

**User Story:** As an Admin or Manager, I want to assign work shifts to employees, so that employees know their scheduled work hours and can check in/out accordingly.

#### Acceptance Criteria

1. WHEN an Admin_User or Manager_User assigns a shift to an employee, THE Shift_System SHALL create a user-shift assignment
2. THE Shift_System SHALL validate that the target employee exists in the system
3. THE Shift_System SHALL validate that the assigned shift exists in the system
4. THE Shift_System SHALL allow multiple shift assignments for the same employee
5. WHEN an Employee_User attempts to assign shifts, THE Shift_System SHALL deny access with authorization error
6. THE Shift_System SHALL record the assignment date and the user who made the assignment

### Requirement 4

**User Story:** As an Employee, I want to view my assigned work shifts, so that I know when I am scheduled to work and can plan accordingly.

#### Acceptance Criteria

1. WHEN an Employee_User requests their shift schedule, THE Shift_System SHALL return only their assigned shifts
2. THE Shift_System SHALL display shift details including name, start time, end time, and assignment date
3. THE Shift_System SHALL show shifts in chronological order by assignment date
4. WHEN an Employee_User attempts to view other employees' shifts, THE Shift_System SHALL deny access
5. THE Shift_System SHALL return an empty list if no shifts are assigned to the employee

### Requirement 5

**User Story:** As an Admin or Manager, I want to view all shift assignments across employees, so that I can monitor workforce distribution and identify scheduling gaps.

#### Acceptance Criteria

1. WHEN an Admin_User requests all shift assignments, THE Shift_System SHALL return assignments for all employees
2. WHEN a Manager_User requests shift assignments, THE Shift_System SHALL return assignments for employees in their department
3. THE Shift_System SHALL display employee name, shift details, and assignment information
4. THE Shift_System SHALL group assignments by employee for better readability
5. WHEN an Employee_User attempts to view all assignments, THE Shift_System SHALL deny access

### Requirement 6

**User Story:** As an Admin or Manager, I want to remove shift assignments from employees, so that I can adjust schedules when business needs change.

#### Acceptance Criteria

1. WHEN an Admin_User or Manager_User removes a shift assignment, THE Shift_System SHALL delete the user-shift relationship
2. THE Shift_System SHALL validate that the assignment exists before attempting removal
3. THE Shift_System SHALL confirm successful removal with appropriate response
4. WHEN an Employee_User attempts to remove assignments, THE Shift_System SHALL deny access
5. THE Shift_System SHALL maintain audit trail of assignment removals

### Requirement 7

**User Story:** As an Admin, I want to update existing work shifts, so that I can modify shift times when operational requirements change.

#### Acceptance Criteria

1. WHEN an Admin_User updates a shift with valid data, THE Shift_System SHALL modify the shift successfully
2. THE Shift_System SHALL validate new start and end times follow the same rules as creation
3. THE Shift_System SHALL update all existing assignments to reflect the new shift times
4. WHEN a Manager_User or Employee_User attempts to update shifts, THE Shift_System SHALL deny access
5. THE Shift_System SHALL prevent updates that would create duplicate shift names

### Requirement 8

**User Story:** As a system user, I want the shift management system to integrate with the existing authentication system, so that access control is consistent across the application.

#### Acceptance Criteria

1. THE Shift_System SHALL use existing JWT authentication for all endpoints
2. THE Shift_System SHALL respect existing role-based authorization (admin, manager, employee)
3. THE Shift_System SHALL return appropriate HTTP status codes for authentication failures
4. THE Shift_System SHALL include user context in all shift-related operations
5. THE Shift_System SHALL log all shift management activities for audit purposes