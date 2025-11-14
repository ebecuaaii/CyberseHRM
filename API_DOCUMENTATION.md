# HRM Cyberse - Shift Management API Documentation

## Overview

The Shift Management API provides comprehensive functionality for managing work shifts and employee shift assignments in the HRM Cyberse system. This API supports role-based access control with three user roles: Admin, Manager, and Employee.

## Base URL

```
Development: https://localhost:7000/api
Production: [Your production URL]/api
```

## Authentication

All API endpoints require JWT Bearer token authentication. Include the token in the Authorization header:

```
Authorization: Bearer <your-jwt-token>
```

### Getting a JWT Token

First, authenticate using the login endpoint:

```http
POST /api/auth/login
Content-Type: application/json

{
    "username": "your_username",
    "password": "your_password"
}
```

**Response:**
```json
{
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "user": {
        "id": 1,
        "username": "admin_user",
        "fullname": "Admin User",
        "role": "admin"
    }
}
```

## Role-Based Access Control

| Role | Permissions |
|------|-------------|
| **Admin** | Full access to all endpoints |
| **Manager** | Can view, create, and assign shifts; cannot update/delete shifts |
| **Employee** | Can only view personal schedule |

## API Endpoints

### 1. Shift Management

#### 1.1 Get All Shifts

Retrieves all work shifts in the system.

```http
GET /api/shifts
Authorization: Bearer <token>
```

**Authorization:** Admin, Manager

**Response (200):**
```json
[
    {
        "id": 1,
        "name": "Ca1",
        "startTime": "06:30:00",
        "endTime": "14:30:00",
        "durationMinutes": 480,
        "createdByName": "Admin User",
        "createdAt": "2024-01-01T00:00:00Z"
    },
    {
        "id": 2,
        "name": "Ca đêm",
        "startTime": "22:30:00",
        "endTime": "06:30:00",
        "durationMinutes": 480,
        "createdByName": "Admin User",
        "createdAt": "2024-01-01T00:00:00Z"
    }
]
```

**Error Responses:**
- `401 Unauthorized`: Missing or invalid token
- `403 Forbidden`: Employee role attempting access
- `500 Internal Server Error`: Server error

#### 1.2 Get Shift by ID

Retrieves a specific work shift by its ID.

```http
GET /api/shifts/{id}
Authorization: Bearer <token>
```

**Authorization:** Admin, Manager

**Parameters:**
- `id` (path): The ID of the shift to retrieve

**Response (200):**
```json
{
    "id": 1,
    "name": "Ca1",
    "startTime": "06:30:00",
    "endTime": "14:30:00",
    "durationMinutes": 480,
    "createdByName": "Admin User",
    "createdAt": "2024-01-01T00:00:00Z"
}
```

**Error Responses:**
- `401 Unauthorized`: Missing or invalid token
- `403 Forbidden`: Employee role attempting access
- `404 Not Found`: Shift not found
- `500 Internal Server Error`: Server error

#### 1.3 Create New Shift

Creates a new work shift.

```http
POST /api/shifts
Authorization: Bearer <token>
Content-Type: application/json

{
    "name": "Morning Shift",
    "startTime": "08:00:00",
    "endTime": "16:00:00"
}
```

**Authorization:** Admin, Manager

**Request Body:**
```json
{
    "name": "string (required, max 100 chars)",
    "startTime": "HH:mm:ss (required)",
    "endTime": "HH:mm:ss (required)"
}
```

**Response (201):**
```json
{
    "id": 5,
    "name": "Morning Shift",
    "startTime": "08:00:00",
    "endTime": "16:00:00",
    "durationMinutes": 480,
    "createdByName": "Current User",
    "createdAt": "2024-01-01T10:00:00Z"
}
```

**Error Responses:**
- `400 Bad Request`: Invalid data, duplicate name, or invalid time range
- `401 Unauthorized`: Missing or invalid token
- `403 Forbidden`: Employee role attempting access
- `500 Internal Server Error`: Server error

**Validation Rules:**
- Shift name must be unique
- Start time must be before end time (for same-day shifts)
- Overnight shifts are supported (end time < start time)

#### 1.4 Update Shift

Updates an existing work shift. Only Admin users can update shifts.

```http
PUT /api/shifts/{id}
Authorization: Bearer <token>
Content-Type: application/json

{
    "name": "Updated Morning Shift",
    "startTime": "07:30:00",
    "endTime": "15:30:00"
}
```

**Authorization:** Admin only

**Parameters:**
- `id` (path): The ID of the shift to update

**Request Body:**
```json
{
    "name": "string (required, max 100 chars)",
    "startTime": "HH:mm:ss (required)",
    "endTime": "HH:mm:ss (required)"
}
```

**Response (200):**
```json
{
    "id": 1,
    "name": "Updated Morning Shift",
    "startTime": "07:30:00",
    "endTime": "15:30:00",
    "durationMinutes": 480,
    "createdByName": "Admin User",
    "createdAt": "2024-01-01T00:00:00Z"
}
```

**Error Responses:**
- `400 Bad Request`: Invalid data, duplicate name, or invalid time range
- `401 Unauthorized`: Missing or invalid token
- `403 Forbidden`: Manager or Employee role attempting access
- `404 Not Found`: Shift not found
- `500 Internal Server Error`: Server error

#### 1.5 Delete Shift

Deletes a work shift. Only Admin users can delete shifts. Cannot delete shifts with existing assignments.

```http
DELETE /api/shifts/{id}
Authorization: Bearer <token>
```

**Authorization:** Admin only

**Parameters:**
- `id` (path): The ID of the shift to delete

**Response (204):** No Content

**Error Responses:**
- `400 Bad Request`: Shift has existing assignments
- `401 Unauthorized`: Missing or invalid token
- `403 Forbidden`: Manager or Employee role attempting access
- `404 Not Found`: Shift not found
- `500 Internal Server Error`: Server error

### 2. Shift Assignments

#### 2.1 Get All Assignments

Retrieves all shift assignments in the system.

```http
GET /api/shifts/assignments
Authorization: Bearer <token>
```

**Authorization:** Admin, Manager

**Response (200):**
```json
[
    {
        "id": 10,
        "userId": 2,
        "userName": "employee_user",
        "fullName": "Employee User",
        "shiftId": 1,
        "shiftName": "Ca1",
        "shiftStartTime": "06:30:00",
        "shiftEndTime": "14:30:00",
        "shiftDate": "2024-12-25",
        "status": "assigned",
        "createdAt": "2024-01-01T10:00:00Z"
    }
]
```

**Error Responses:**
- `401 Unauthorized`: Missing or invalid token
- `403 Forbidden`: Employee role attempting access
- `500 Internal Server Error`: Server error

#### 2.2 Assign Shift to Employee

Assigns a work shift to an employee for a specific date.

```http
POST /api/shifts/assign
Authorization: Bearer <token>
Content-Type: application/json

{
    "userId": 5,
    "shiftId": 1,
    "shiftDate": "2024-12-25",
    "status": "assigned"
}
```

**Authorization:** Admin, Manager

**Request Body:**
```json
{
    "userId": "integer (required)",
    "shiftId": "integer (required)",
    "shiftDate": "YYYY-MM-DD (required)",
    "status": "string (optional, default: 'assigned')"
}
```

**Response (201):**
```json
{
    "id": 15,
    "userId": 5,
    "userName": "employee_user",
    "fullName": "Employee User",
    "shiftId": 1,
    "shiftName": "Ca1",
    "shiftStartTime": "06:30:00",
    "shiftEndTime": "14:30:00",
    "shiftDate": "2024-12-25",
    "status": "assigned",
    "createdAt": "2024-01-01T10:00:00Z"
}
```

**Error Responses:**
- `400 Bad Request`: Invalid data, user/shift doesn't exist, or duplicate assignment
- `401 Unauthorized`: Missing or invalid token
- `403 Forbidden`: Employee role attempting access
- `500 Internal Server Error`: Server error

**Business Rules:**
- User must exist in the system
- Shift must exist in the system
- Cannot assign duplicate shifts (same user, shift, and date)

#### 2.3 Get Assignment by ID

Retrieves a specific shift assignment by its ID.

```http
GET /api/shifts/assignments/{id}
Authorization: Bearer <token>
```

**Authorization:** Admin, Manager

**Parameters:**
- `id` (path): The ID of the assignment to retrieve

**Response (200):**
```json
{
    "id": 10,
    "userId": 2,
    "userName": "employee_user",
    "fullName": "Employee User",
    "shiftId": 1,
    "shiftName": "Ca1",
    "shiftStartTime": "06:30:00",
    "shiftEndTime": "14:30:00",
    "shiftDate": "2024-12-25",
    "status": "assigned",
    "createdAt": "2024-01-01T10:00:00Z"
}
```

**Error Responses:**
- `401 Unauthorized`: Missing or invalid token
- `403 Forbidden`: Employee role attempting access
- `404 Not Found`: Assignment not found
- `500 Internal Server Error`: Server error

#### 2.4 Remove Assignment

Removes a shift assignment from an employee.

```http
DELETE /api/shifts/assignments/{id}
Authorization: Bearer <token>
```

**Authorization:** Admin, Manager

**Parameters:**
- `id` (path): The ID of the assignment to remove

**Response (204):** No Content

**Error Responses:**
- `400 Bad Request`: Assignment cannot be removed
- `401 Unauthorized`: Missing or invalid token
- `403 Forbidden`: Employee role attempting access
- `404 Not Found`: Assignment not found
- `500 Internal Server Error`: Server error

### 3. Employee Schedules

#### 3.1 Get Personal Schedule

Retrieves the current user's personal shift schedule.

```http
GET /api/shifts/my-schedule
Authorization: Bearer <token>
```

**Authorization:** All authenticated users

**Query Parameters:**
- `fromDate` (optional): Start date filter (YYYY-MM-DD format)
- `toDate` (optional): End date filter (YYYY-MM-DD format)
- `sortBy` (optional): Sort field - ShiftDate, ShiftName, ShiftStartTime, Status (default: ShiftDate)
- `ascending` (optional): Sort direction - true/false (default: true)

**Example with filters:**
```http
GET /api/shifts/my-schedule?fromDate=2024-12-01&toDate=2024-12-31&sortBy=ShiftDate&ascending=true
```

**Response (200):**
```json
[
    {
        "id": 10,
        "userId": 2,
        "userName": "current_user",
        "fullName": "Current User",
        "shiftId": 1,
        "shiftName": "Ca1",
        "shiftStartTime": "06:30:00",
        "shiftEndTime": "14:30:00",
        "shiftDate": "2024-12-25",
        "status": "assigned",
        "createdAt": "2024-01-01T10:00:00Z"
    }
]
```

**Error Responses:**
- `400 Bad Request`: Invalid query parameters
- `401 Unauthorized`: Missing or invalid token
- `500 Internal Server Error`: Server error

#### 3.2 Get User Schedule

Retrieves a specific user's shift schedule. Only Admin and Manager users can access this endpoint.

```http
GET /api/shifts/user/{userId}/schedule
Authorization: Bearer <token>
```

**Authorization:** Admin, Manager

**Parameters:**
- `userId` (path): The ID of the user whose schedule to retrieve

**Query Parameters:**
- `fromDate` (optional): Start date filter (YYYY-MM-DD format)
- `toDate` (optional): End date filter (YYYY-MM-DD format)
- `sortBy` (optional): Sort field - ShiftDate, ShiftName, ShiftStartTime, Status (default: ShiftDate)
- `ascending` (optional): Sort direction - true/false (default: true)

**Example:**
```http
GET /api/shifts/user/5/schedule?fromDate=2024-12-01&toDate=2024-12-31
```

**Response (200):**
```json
[
    {
        "id": 15,
        "userId": 5,
        "userName": "employee_user",
        "fullName": "Employee User",
        "shiftId": 1,
        "shiftName": "Ca1",
        "shiftStartTime": "06:30:00",
        "shiftEndTime": "14:30:00",
        "shiftDate": "2024-12-25",
        "status": "assigned",
        "createdAt": "2024-01-01T10:00:00Z"
    }
]
```

**Error Responses:**
- `400 Bad Request`: Invalid query parameters
- `401 Unauthorized`: Missing or invalid token
- `403 Forbidden`: Employee role attempting access
- `404 Not Found`: User not found
- `500 Internal Server Error`: Server error

## Data Models

### ShiftDto

```json
{
    "id": "integer",
    "name": "string",
    "startTime": "HH:mm:ss",
    "endTime": "HH:mm:ss",
    "durationMinutes": "integer",
    "createdByName": "string",
    "createdAt": "ISO 8601 datetime"
}
```

### CreateShiftDto

```json
{
    "name": "string (required, max 100 chars)",
    "startTime": "HH:mm:ss (required)",
    "endTime": "HH:mm:ss (required)"
}
```

### UpdateShiftDto

```json
{
    "name": "string (required, max 100 chars)",
    "startTime": "HH:mm:ss (required)",
    "endTime": "HH:mm:ss (required)"
}
```

### UserShiftDto

```json
{
    "id": "integer",
    "userId": "integer",
    "userName": "string",
    "fullName": "string",
    "shiftId": "integer",
    "shiftName": "string",
    "shiftStartTime": "HH:mm:ss",
    "shiftEndTime": "HH:mm:ss",
    "shiftDate": "YYYY-MM-DD",
    "status": "string",
    "createdAt": "ISO 8601 datetime"
}
```

### AssignShiftDto

```json
{
    "userId": "integer (required)",
    "shiftId": "integer (required)",
    "shiftDate": "YYYY-MM-DD (required)",
    "status": "string (optional, default: 'assigned')"
}
```

## Error Response Format

All error responses follow a consistent format:

```json
{
    "type": "string",
    "title": "string",
    "status": "integer",
    "detail": "string",
    "instance": "string"
}
```

For validation errors:

```json
{
    "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
    "title": "One or more validation errors occurred.",
    "status": 400,
    "errors": {
        "Name": ["Tên ca làm việc là bắt buộc"],
        "StartTime": ["Thời gian bắt đầu là bắt buộc"]
    }
}
```

## Common HTTP Status Codes

| Code | Description |
|------|-------------|
| 200 | OK - Request successful |
| 201 | Created - Resource created successfully |
| 204 | No Content - Request successful, no content returned |
| 400 | Bad Request - Invalid request data or business rule violation |
| 401 | Unauthorized - Missing or invalid authentication token |
| 403 | Forbidden - Insufficient permissions for the requested operation |
| 404 | Not Found - Requested resource not found |
| 500 | Internal Server Error - Unexpected server error |

## Business Rules

### Shift Creation/Update Rules

1. **Unique Names**: Shift names must be unique across the system
2. **Time Validation**: Start time must be before end time for same-day shifts
3. **Overnight Shifts**: Supported when end time is less than start time (e.g., 22:00 to 06:00)
4. **Duration Calculation**: Automatically calculated in minutes, accounting for overnight shifts

### Assignment Rules

1. **User Validation**: User must exist in the system
2. **Shift Validation**: Shift must exist in the system
3. **Duplicate Prevention**: Cannot assign the same shift to the same user on the same date
4. **Status Values**: Common values include "assigned", "completed", "cancelled"

### Authorization Rules

1. **Admin**: Full access to all operations
2. **Manager**: Can view, create, and assign shifts; cannot update/delete shifts
3. **Employee**: Can only view personal schedule

### Deletion Rules

1. **Shift Deletion**: Cannot delete shifts that have existing assignments
2. **Assignment Removal**: Can remove assignments at any time

## Rate Limiting

The API implements rate limiting to prevent abuse:

- **General endpoints**: 100 requests per minute per user
- **Creation endpoints**: 10 requests per minute per user
- **Authentication endpoints**: 5 requests per minute per IP

## Swagger Documentation

Interactive API documentation is available at:

```
Development: https://localhost:7000/swagger
```

The Swagger UI provides:
- Interactive endpoint testing
- Request/response examples
- Authentication setup
- Model schemas
- Error response examples

## Postman Collection

A comprehensive Postman collection is available that includes:
- Pre-configured authentication
- All endpoint examples
- Test scenarios for different user roles
- Error case testing
- Environment variables setup

## Support

For API support or questions:
- Email: support@hrmcyberse.com
- Documentation: [Internal Wiki Link]
- Issue Tracking: [Internal Issue Tracker]