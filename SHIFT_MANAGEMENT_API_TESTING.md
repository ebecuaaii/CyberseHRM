# Hướng Dẫn Test API Shift Management với Postman

## Tổng quan
Tài liệu này hướng dẫn cách test toàn bộ API Shift Management System bằng Postman, bao gồm authentication, authorization và tất cả các endpoints.

## Chuẩn bị

### 1. Database Setup
Đảm bảo database PostgreSQL đã được setup với:
- Connection string trong `appsettings.json`
- Các bảng: `users`, `shifts`, `usershifts`, `activitylogs`
- Dữ liệu test users với các roles khác nhau

### 2. Chạy ứng dụng
```bash
dotnet run
```
Ứng dụng sẽ chạy tại: `https://localhost:7000` hoặc `http://localhost:5000`

### 3. Postman Environment
Tạo environment trong Postman với các biến:
- `baseUrl`: `https://localhost:7000` (hoặc port của bạn)
- `token`: (sẽ được set sau khi login)
- `userId`: (ID của user đã login)

## Authentication & Authorization

### 1. Đăng ký User (nếu cần)
```http
POST {{baseUrl}}/api/auth/register
Content-Type: application/json

{
    "username": "admin_test",
    "password": "Admin123!",
    "fullname": "Admin Test User",
    "email": "admin@test.com",
    "role": "admin"
}
```

### 2. Đăng nhập để lấy JWT Token
```http
POST {{baseUrl}}/api/auth/login
Content-Type: application/json

{
    "username": "admin_test",
    "password": "Admin123!"
}
```

**Response:**
```json
{
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "user": {
        "id": 1,
        "username": "admin_test",
        "fullname": "Admin Test User",
        "role": "admin"
    }
}
```

**Postman Script (Tests tab):**
```javascript
if (pm.response.code === 200) {
    const response = pm.response.json();
    pm.environment.set("token", response.token);
    pm.environment.set("userId", response.user.id);
}
```

### 3. Setup Authorization Header
Trong tất cả requests tiếp theo, thêm header:
```
Authorization: Bearer {{token}}
```

## Shift Management API Tests

### 1. Lấy danh sách tất cả ca làm việc

#### Test với Admin/Manager (Should PASS)
```http
GET {{baseUrl}}/api/shifts
Authorization: Bearer {{token}}
```

**Expected Response (200):**
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
    }
]
```

#### Test với Employee (Should FAIL)
Đăng nhập với user có role "employee" và thử request trên.
**Expected Response (403):** Forbidden

### 2. Tạo ca làm việc mới

#### Test tạo ca bình thường (Admin/Manager)
```http
POST {{baseUrl}}/api/shifts
Authorization: Bearer {{token}}
Content-Type: application/json

{
    "name": "Ca Test",
    "startTime": "09:00:00",
    "endTime": "17:00:00"
}
```

**Expected Response (201):**
```json
{
    "id": 5,
    "name": "Ca Test",
    "startTime": "09:00:00",
    "endTime": "17:00:00",
    "durationMinutes": 480,
    "createdByName": "Admin Test User",
    "createdAt": "2024-01-01T10:00:00Z"
}
```

#### Test tạo ca đêm
```http
POST {{baseUrl}}/api/shifts
Authorization: Bearer {{token}}
Content-Type: application/json

{
    "name": "Ca đêm test",
    "startTime": "22:00:00",
    "endTime": "06:00:00"
}
```

#### Test tạo ca với dữ liệu không hợp lệ
```http
POST {{baseUrl}}/api/shifts
Authorization: Bearer {{token}}
Content-Type: application/json

{
    "name": "",
    "startTime": "17:00:00",
    "endTime": "09:00:00"
}
```

**Expected Response (400):** Bad Request với validation errors

#### Test tạo ca trùng tên
```http
POST {{baseUrl}}/api/shifts
Authorization: Bearer {{token}}
Content-Type: application/json

{
    "name": "Ca Test",
    "startTime": "10:00:00",
    "endTime": "18:00:00"
}
```

**Expected Response (400):** "Tên ca làm việc đã tồn tại"

### 3. Lấy thông tin ca làm việc theo ID

#### Test với ID hợp lệ
```http
GET {{baseUrl}}/api/shifts/1
Authorization: Bearer {{token}}
```

#### Test với ID không tồn tại
```http
GET {{baseUrl}}/api/shifts/999
Authorization: Bearer {{token}}
```

**Expected Response (404):** Not Found

### 4. Cập nhật ca làm việc (Admin only)

#### Test update thành công
```http
PUT {{baseUrl}}/api/shifts/5
Authorization: Bearer {{token}}
Content-Type: application/json

{
    "name": "Ca Test Updated",
    "startTime": "08:30:00",
    "endTime": "16:30:00"
}
```

#### Test update với Manager (Should FAIL)
Đăng nhập với user có role "manager" và thử request trên.
**Expected Response (403):** Forbidden

### 5. Xóa ca làm việc (Admin only)

#### Test xóa ca không có assignment
```http
DELETE {{baseUrl}}/api/shifts/5
Authorization: Bearer {{token}}
```

**Expected Response (204):** No Content

#### Test xóa ca đã có assignment
```http
DELETE {{baseUrl}}/api/shifts/1
Authorization: Bearer {{token}}
```

**Expected Response (400):** "Không thể xóa ca đã được gán cho nhân viên"

### 6. Phân công ca làm việc

#### Test assign ca cho nhân viên
```http
POST {{baseUrl}}/api/shifts/assign
Authorization: Bearer {{token}}
Content-Type: application/json

{
    "userId": 2,
    "shiftId": 1,
    "shiftDate": "2024-12-25",
    "status": "assigned"
}
```

**Expected Response (201):**
```json
{
    "id": 10,
    "userId": 2,
    "userName": "employee_test",
    "fullName": "Employee Test User",
    "shiftId": 1,
    "shiftName": "Ca1",
    "shiftStartTime": "06:30:00",
    "shiftEndTime": "14:30:00",
    "shiftDate": "2024-12-25",
    "status": "assigned",
    "createdAt": "2024-01-01T10:00:00Z"
}
```

#### Test assign với user không tồn tại
```http
POST {{baseUrl}}/api/shifts/assign
Authorization: Bearer {{token}}
Content-Type: application/json

{
    "userId": 999,
    "shiftId": 1,
    "shiftDate": "2024-12-25"
}
```

**Expected Response (400):** "Người dùng không tồn tại"

#### Test assign với shift không tồn tại
```http
POST {{baseUrl}}/api/shifts/assign
Authorization: Bearer {{token}}
Content-Type: application/json

{
    "userId": 2,
    "shiftId": 999,
    "shiftDate": "2024-12-25"
}
```

**Expected Response (400):** "Ca làm việc không tồn tại"

#### Test assign với ngày trong quá khứ
```http
POST {{baseUrl}}/api/shifts/assign
Authorization: Bearer {{token}}
Content-Type: application/json

{
    "userId": 2,
    "shiftId": 1,
    "shiftDate": "2024-01-01"
}
```

**Expected Response (400):** Validation error về ngày

#### Test assign trùng lặp
Thử assign cùng user, cùng shift, cùng ngày lần thứ 2.
**Expected Response (400):** "Nhân viên đã được gán ca này cho ngày..."

### 7. Xem tất cả phân công ca làm việc

```http
GET {{baseUrl}}/api/shifts/assignments
Authorization: Bearer {{token}}
```

**Expected Response (200):**
```json
[
    {
        "id": 10,
        "userId": 2,
        "userName": "employee_test",
        "fullName": "Employee Test User",
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

### 8. Xem lịch làm việc cá nhân

#### Test với user đã đăng nhập
```http
GET {{baseUrl}}/api/shifts/my-schedule
Authorization: Bearer {{token}}
```

#### Test với query parameters
```http
GET {{baseUrl}}/api/shifts/my-schedule?fromDate=2024-12-01&toDate=2024-12-31&sortBy=ShiftDate&ascending=true
Authorization: Bearer {{token}}
```

### 9. Xem lịch làm việc của user khác (Admin/Manager only)

```http
GET {{baseUrl}}/api/shifts/user/2/schedule
Authorization: Bearer {{token}}
```

#### Test với Employee (Should FAIL)
**Expected Response (403):** Forbidden

### 10. Hủy phân công ca làm việc

#### Test hủy assignment hợp lệ
```http
DELETE {{baseUrl}}/api/shifts/assignments/10
Authorization: Bearer {{token}}
```

**Expected Response (204):** No Content

#### Test hủy assignment không tồn tại
```http
DELETE {{baseUrl}}/api/shifts/assignments/999
Authorization: Bearer {{token}}
```

**Expected Response (404):** Not Found

## Test Cases cho Conflict Detection

### 1. Tạo conflict scenario
```http
# Bước 1: Assign ca 1 (6:30-14:30) cho user 2 ngày 25/12
POST {{baseUrl}}/api/shifts/assign
{
    "userId": 2,
    "shiftId": 1,
    "shiftDate": "2024-12-25"
}

# Bước 2: Thử assign ca 2 (9:30-14:30) cho cùng user cùng ngày
POST {{baseUrl}}/api/shifts/assign
{
    "userId": 2,
    "shiftId": 2,
    "shiftDate": "2024-12-25"
}
```

**Expected:** Bước 2 should fail với conflict error

## Test Cases cho Business Rules

### 1. Test duration calculation
Tạo các ca với thời gian khác nhau và verify `durationMinutes`:
- Ca 8 tiếng: 9:00-17:00 → 480 phút
- Ca 4 tiếng: 14:30-18:30 → 240 phút  
- Ca đêm 8 tiếng: 22:00-06:00 → 480 phút

### 2. Test overnight shifts
```http
POST {{baseUrl}}/api/shifts
{
    "name": "Ca đêm test",
    "startTime": "23:00:00",
    "endTime": "07:00:00"
}
```

Verify duration = 480 phút (8 tiếng)

## Error Scenarios Testing

### 1. Authentication Errors
- Request không có Authorization header → 401
- Request với token không hợp lệ → 401
- Request với token hết hạn → 401

### 2. Authorization Errors
- Employee thử tạo shift → 403
- Manager thử update shift → 403
- Employee thử xem assignments của người khác → 403

### 3. Validation Errors
- Tên ca rỗng → 400
- Thời gian không hợp lệ → 400
- User ID không tồn tại → 400
- Shift ID không tồn tại → 400

### 4. Business Rule Violations
- Tạo ca trùng tên → 400
- Assign trùng lặp → 400
- Xóa ca đã có assignment → 400
- Assign ca cho ngày quá khứ → 400

## Postman Collection Structure

Tạo collection với các folders:
```
Shift Management API
├── 01. Authentication
│   ├── Register Admin
│   ├── Register Manager  
│   ├── Register Employee
│   ├── Login Admin
│   ├── Login Manager
│   └── Login Employee
├── 02. Shifts CRUD
│   ├── Get All Shifts (Admin)
│   ├── Get All Shifts (Manager)
│   ├── Get All Shifts (Employee - Should Fail)
│   ├── Get Shift By ID
│   ├── Create Shift (Valid)
│   ├── Create Shift (Invalid)
│   ├── Create Overnight Shift
│   ├── Update Shift (Admin)
│   ├── Update Shift (Manager - Should Fail)
│   └── Delete Shift
├── 03. Shift Assignments
│   ├── Assign Shift (Valid)
│   ├── Assign Shift (Invalid User)
│   ├── Assign Shift (Invalid Shift)
│   ├── Assign Shift (Past Date)
│   ├── Assign Shift (Duplicate)
│   ├── Get All Assignments
│   ├── Get My Schedule
│   ├── Get User Schedule (Admin)
│   └── Remove Assignment
├── 04. Error Scenarios
│   ├── Unauthorized Requests
│   ├── Forbidden Requests
│   ├── Not Found Requests
│   └── Validation Errors
└── 05. Business Rules
    ├── Conflict Detection
    ├── Duration Calculation
    └── Delete Restrictions
```

## Expected Results Summary

| Test Case | Admin | Manager | Employee |
|-----------|-------|---------|----------|
| View Shifts | ✅ 200 | ✅ 200 | ❌ 403 |
| Create Shift | ✅ 201 | ✅ 201 | ❌ 403 |
| Update Shift | ✅ 200 | ❌ 403 | ❌ 403 |
| Delete Shift | ✅ 204 | ❌ 403 | ❌ 403 |
| Assign Shift | ✅ 201 | ✅ 201 | ❌ 403 |
| View Assignments | ✅ 200 | ✅ 200 | ❌ 403 |
| View My Schedule | ✅ 200 | ✅ 200 | ✅ 200 |
| Remove Assignment | ✅ 204 | ✅ 204 | ❌ 403 |

## Notes
- Tất cả timestamps trong response sẽ ở UTC format
- Duration được tính bằng phút
- Overnight shifts được handle tự động (end time < start time)
- Conflict detection hoạt động cho cả same-day và overnight shifts
- Audit logs được tạo tự động cho tất cả operations