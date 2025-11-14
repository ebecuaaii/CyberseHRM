# Attendance Management API Testing Guide

## Overview
This document provides testing instructions for the Attendance Management API endpoints.

## Prerequisites
- User must be authenticated (JWT token required)
- User must have appropriate shifts assigned
- Database must contain valid users, shifts, and user-shift assignments

## API Endpoints

### 1. Check In
**POST** `/api/attendance/check-in`

**Request Body:**
```json
{
  "userId": 1,
  "shiftId": 1,
  "latitude": 10.7769,
  "longitude": 106.7009,
  "imageUrl": "https://example.com/checkin-image.jpg",
  "notes": "Checked in from office"
}
```

**Expected Response:**
```json
{
  "id": 1,
  "userId": 1,
  "userName": "John Doe",
  "shiftId": 1,
  "shiftName": "Morning Shift",
  "checkInTime": "2024-01-15T08:00:00Z",
  "checkOutTime": null,
  "checkInLat": 10.7769,
  "checkInLng": 106.7009,
  "checkOutLat": null,
  "checkOutLng": null,
  "checkInImageUrl": "https://example.com/checkin-image.jpg",
  "checkOutImageUrl": null,
  "status": "On Time",
  "notes": "Checked in from office",
  "createdAt": "2024-01-15T08:00:00Z",
  "images": [
    {
      "id": 1,
      "imageUrl": "https://example.com/checkin-image.jpg",
      "type": "CheckIn",
      "createdAt": "2024-01-15T08:00:00Z"
    }
  ]
}
```

### 2. Check Out
**POST** `/api/attendance/check-out`

**Request Body:**
```json
{
  "attendanceId": 1,
  "latitude": 10.7769,
  "longitude": 106.7009,
  "imageUrl": "https://example.com/checkout-image.jpg",
  "notes": "Checked out from office"
}
```

### 3. Manual Attendance (Manager/Admin Only)
**POST** `/api/attendance/manual`

**Request Body:**
```json
{
  "userId": 2,
  "shiftId": 1,
  "checkInTime": "2024-01-15T08:30:00Z",
  "checkOutTime": "2024-01-15T17:00:00Z",
  "status": "Late",
  "notes": "Employee forgot to check in",
  "createdByManagerId": 1
}
```

### 4. Get Today's Attendance
**GET** `/api/attendance/today/{userId}`

### 5. Get Attendance History
**GET** `/api/attendance/history/{userId}?startDate=2024-01-01&endDate=2024-01-31`

### 6. Get Attendance Report (Manager/Admin Only)
**POST** `/api/attendance/report`

**Request Body:**
```json
{
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": "2024-01-31T23:59:59Z",
  "userId": null,
  "departmentId": 1,
  "status": "Late"
}
```

### 7. Check Permissions
**GET** `/api/attendance/can-check-in?userId=1&shiftId=1`
**GET** `/api/attendance/can-check-out/1`

## Test Scenarios

### Scenario 1: Normal Check-in/Check-out Flow
1. User checks in at shift start time → Status: "On Time"
2. User checks out at shift end time → Complete attendance record

### Scenario 2: Late Check-in
1. User checks in after shift start time → Status: "Late"
2. Verify late minutes calculation in report

### Scenario 3: Duplicate Check-in Prevention
1. User checks in successfully
2. Attempt second check-in same day → Should fail with error

### Scenario 4: Manager Manual Entry
1. Manager creates manual attendance for employee
2. Verify proper notes and status assignment

### Scenario 5: Attendance Reports
1. Generate report for date range
2. Filter by department, user, or status
3. Verify calculations (worked hours, late minutes)

## Error Cases to Test

### 1. Invalid Check-in Attempts
- User not assigned to shift
- User already checked in today
- Invalid shift ID

### 2. Invalid Check-out Attempts
- Attendance record not found
- Already checked out
- Invalid attendance ID

### 3. Permission Errors
- Non-manager trying to create manual attendance
- Accessing other user's data without permission

## Status Values
- "On Time": Check-in at or before shift start time
- "Late": Check-in after shift start time
- "Manual Entry": Created by manager
- "Absent": No check-in record for assigned shift

## GPS and Image Testing
- Test with valid GPS coordinates
- Test with image URLs
- Test without optional GPS/image data
- Verify image records are created properly

## Performance Considerations
- Test with large date ranges for reports
- Test concurrent check-ins
- Verify database indexes are working (check query performance)