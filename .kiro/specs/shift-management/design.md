# Tài liệu Thiết kế - Hệ thống Quản lý Ca làm việc

## Tổng quan

Hệ thống Quản lý Ca làm việc cung cấp chức năng toàn diện để quản lý ca làm việc và phân công ca cho nhân viên trong ứng dụng HRM Cyberse. Hệ thống tận dụng các models database hiện có (Shift và Usershift) và tích hợp liền mạch với framework xác thực JWT và phân quyền dựa trên vai trò hiện tại.

## Kiến trúc

### Các thành phần hệ thống

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   Controllers   │    │     Services     │    │     Models      │
│                 │    │                  │    │                 │
│ ShiftsController│───▶│ IShiftService    │───▶│ Shift           │
│                 │    │ ShiftService     │    │ Usershift       │
│                 │    │                  │    │ User            │
└─────────────────┘    └──────────────────┘    └─────────────────┘
         │                       │                       │
         ▼                       ▼                       ▼
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│      DTOs       │    │  Authorization   │    │    Database     │
│                 │    │                  │    │                 │
│ ShiftDto        │    │ RequireRole      │    │ CybersehrmContext│
│ UserShiftDto    │    │ AdminOrManager   │    │                 │
│ AssignShiftDto  │    │ JWT Middleware   │    │                 │
└─────────────────┘    └──────────────────┘    └─────────────────┘
```

### Điểm tích hợp

- **Xác thực**: Sử dụng hệ thống xác thực JWT hiện có
- **Phân quyền**: Tận dụng kiểm soát truy cập dựa trên vai trò hiện tại (Admin, Manager, Employee)
- **Cơ sở dữ liệu**: Sử dụng các models Shift và Usershift hiện có
- **Ghi log**: Tích hợp với framework logging của ASP.NET Core

## Components and Interfaces

### 1. Controllers

#### ShiftsController
- **Purpose**: Handle HTTP requests for shift management operations
- **Authorization**: Role-based access control using custom attributes
- **Endpoints**:
  - `GET /api/shifts` - List all shifts (Admin/Manager only)
  - `POST /api/shifts` - Create new shift (Admin/Manager only)
  - `PUT /api/shifts/{id}` - Update shift (Admin only)
  - `DELETE /api/shifts/{id}` - Delete shift (Admin only)
  - `GET /api/shifts/assignments` - View all assignments (Admin/Manager)
  - `POST /api/shifts/assign` - Assign shift to employee (Admin/Manager)
  - `DELETE /api/shifts/assignments/{id}` - Remove assignment (Admin/Manager)
  - `GET /api/shifts/my-schedule` - View personal schedule (All authenticated users)

### 2. Services

#### IShiftService Interface
```csharp
public interface IShiftService
{
    Task<IEnumerable<ShiftDto>> GetAllShiftsAsync();
    Task<ShiftDto?> GetShiftByIdAsync(int id);
    Task<ShiftDto> CreateShiftAsync(CreateShiftDto createShiftDto, int createdBy);
    Task<ShiftDto> UpdateShiftAsync(int id, UpdateShiftDto updateShiftDto);
    Task<bool> DeleteShiftAsync(int id);
    Task<IEnumerable<UserShiftDto>> GetAllAssignmentsAsync();
    Task<IEnumerable<UserShiftDto>> GetUserAssignmentsAsync(int userId);
    Task<UserShiftDto> AssignShiftAsync(AssignShiftDto assignShiftDto, int assignedBy);
    Task<bool> RemoveAssignmentAsync(int assignmentId);
    Task<bool> ValidateShiftTimesAsync(TimeOnly startTime, TimeOnly endTime);
}
```

#### ShiftService Implementation
- **Responsibilities**:
  - Business logic for shift operations
  - Data validation and transformation
  - Integration with database context
  - Audit logging for shift changes

### 3. Data Transfer Objects (DTOs)

#### ShiftDto
```csharp
public class ShiftDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int? DurationMinutes { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime? CreatedAt { get; set; }
}
```

#### CreateShiftDto
```csharp
public class CreateShiftDto
{
    [Required(ErrorMessage = "Tên ca làm việc là bắt buộc")]
    [StringLength(100, ErrorMessage = "Tên ca không được quá 100 ký tự")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Thời gian bắt đầu là bắt buộc")]
    public TimeOnly StartTime { get; set; }

    [Required(ErrorMessage = "Thời gian kết thúc là bắt buộc")]
    public TimeOnly EndTime { get; set; }
}
```

#### UserShiftDto
```csharp
public class UserShiftDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; }
    public string FullName { get; set; }
    public int ShiftId { get; set; }
    public string ShiftName { get; set; }
    public TimeOnly ShiftStartTime { get; set; }
    public TimeOnly ShiftEndTime { get; set; }
    public DateOnly ShiftDate { get; set; }
    public string? Status { get; set; }
    public DateTime? CreatedAt { get; set; }
}
```

#### AssignShiftDto
```csharp
public class AssignShiftDto
{
    [Required(ErrorMessage = "User ID là bắt buộc")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "Shift ID là bắt buộc")]
    public int ShiftId { get; set; }

    [Required(ErrorMessage = "Ngày làm việc là bắt buộc")]
    public DateOnly ShiftDate { get; set; }

    public string? Status { get; set; } = "assigned";
}
```

## Data Models

### Existing Database Models (Already Scaffolded)

#### Shift Model
- **Table**: shifts
- **Key Properties**:
  - `Id`: Primary key
  - `Name`: Shift name (e.g., "Ca1", "Ca đêm")
  - `Starttime`: Shift start time (TimeOnly)
  - `Endtime`: Shift end time (TimeOnly)
  - `Durationminutes`: Calculated duration
  - `Createdby`: Foreign key to User
  - `Createdat`: Creation timestamp

#### Usershift Model
- **Table**: usershifts
- **Key Properties**:
  - `Id`: Primary key
  - `Userid`: Foreign key to User
  - `Shiftid`: Foreign key to Shift
  - `Shiftdate`: Date for the shift assignment
  - `Status`: Assignment status (assigned, completed, cancelled)
  - `Createdat`: Assignment timestamp

### Relationships
- `Shift` 1:N `Usershift` (One shift can have many assignments)
- `User` 1:N `Usershift` (One user can have many shift assignments)
- `User` 1:N `Shift` (One user can create many shifts via Createdby)

## Error Handling

### Validation Errors
- **Input Validation**: Use Data Annotations for DTO validation
- **Business Rules**: Custom validation in service layer
- **Response Format**: Consistent error response structure

### Exception Handling
```csharp
public class ShiftNotFoundException : Exception
{
    public ShiftNotFoundException(int shiftId) 
        : base($"Shift with ID {shiftId} not found") { }
}

public class UserShiftConflictException : Exception
{
    public UserShiftConflictException(string message) : base(message) { }
}
```

### HTTP Status Codes
- `200 OK`: Successful operations
- `201 Created`: Successful creation
- `400 Bad Request`: Validation errors
- `401 Unauthorized`: Authentication required
- `403 Forbidden`: Insufficient permissions
- `404 Not Found`: Resource not found
- `409 Conflict`: Business rule violations
- `500 Internal Server Error`: Unexpected errors

## Testing Strategy

### Unit Tests
- **Service Layer**: Test business logic and validation
- **Controller Layer**: Test HTTP request/response handling
- **Authorization**: Test role-based access control

### Integration Tests
- **Database Operations**: Test CRUD operations with test database
- **Authentication Flow**: Test JWT integration
- **End-to-End Scenarios**: Test complete user workflows

### Test Data Setup
```csharp
// Predefined shifts for testing
var testShifts = new[]
{
    new Shift { Name = "Ca1", Starttime = new TimeOnly(6, 30), Endtime = new TimeOnly(14, 30) },
    new Shift { Name = "Ca2", Starttime = new TimeOnly(9, 30), Endtime = new TimeOnly(14, 30) },
    new Shift { Name = "Ca3", Starttime = new TimeOnly(14, 30), Endtime = new TimeOnly(22, 30) },
    new Shift { Name = "Ca4", Starttime = new TimeOnly(14, 30), Endtime = new TimeOnly(18, 30) },
    new Shift { Name = "Ca5", Starttime = new TimeOnly(18, 30), Endtime = new TimeOnly(22, 30) },
    new Shift { Name = "Ca đêm", Starttime = new TimeOnly(22, 30), Endtime = new TimeOnly(6, 30) }
};
```

## Security Considerations

### Authentication
- All endpoints require valid JWT token
- Token validation handled by existing middleware

### Authorization
- Role-based access control using custom attributes
- Admin: Full access to all shift operations
- Manager: Can manage shifts and assignments for their department
- Employee: Read-only access to personal schedule

### Data Protection
- Sensitive operations logged for audit trail
- Input sanitization to prevent injection attacks
- Rate limiting on shift creation/modification endpoints

## Performance Considerations

### Database Optimization
- Proper indexing on foreign keys (userid, shiftid)
- Efficient queries using Entity Framework Include() for related data
- Pagination for large result sets

### Caching Strategy
- Cache frequently accessed shift data
- Invalidate cache on shift modifications
- Use memory cache for shift lookup operations

### API Response Optimization
- Use DTOs to control data exposure
- Implement compression for large responses
- Consider async/await for all database operations