# HRM Cyberse API - Swagger Documentation

## 🚀 Access Swagger UI

**Development:**
```
http://localhost:5267/swagger
```

**Swagger JSON:**
```
http://localhost:5267/swagger/v1/swagger.json
```

---

## 📚 API Groups Overview

### 1️⃣ Authentication & User Management
**Base Path:** `/api/auth`, `/api/users`

**Features:**
- User registration and login
- JWT token generation
- User profile management
- Role-based access control

**Key Endpoints:**
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login and get JWT token
- `GET /api/users` - Get all users (Admin only)
- `GET /api/users/{id}` - Get user by ID

---

### 2️⃣ Shift Management
**Base Path:** `/api/shifts`

**Features:**
- Create and manage work shifts
- Assign shifts to employees
- View shift schedules
- Shift templates

**Key Endpoints:**
- `GET /api/shifts` - Get all shifts (with caching)
- `POST /api/shifts` - Create new shift (Admin/Manager)
- `POST /api/shifts/assign` - Assign shift to employee
- `GET /api/shifts/{id}/assignments` - Get shift assignments
- `DELETE /api/shifts/{id}` - Delete shift (Admin only)

---

### 3️⃣ Attendance Management
**Base Path:** `/api/attendance`

**Features:**
- Check-in/Check-out with GPS coordinates
- Photo capture for attendance
- Automatic late detection
- Attendance reports and analytics
- Manual attendance entry (Manager)

**Key Endpoints:**
- `POST /api/attendance/check-in` - Employee check-in
- `POST /api/attendance/check-out` - Employee check-out
- `GET /api/attendance/today/{userId}` - Today's attendance
- `GET /api/attendance/history/{userId}` - Attendance history
- `POST /api/attendance/report` - Generate report (Admin/Manager)
- `POST /api/attendance/manual` - Manual entry (Admin/Manager)
- `GET /api/attendance/summary/{userId}` - Attendance statistics

---

### 4️⃣ Request Management
**Base Paths:** `/api/requests`, `/api/shiftrequests`, `/api/laterequests`

**Features:**
- Leave requests (vacation, sick leave)
- Shift change requests
- Late arrival requests
- Manager approval workflow
- Request history and status tracking

**Leave Requests:**
- `POST /api/requests/leave` - Create leave request
- `POST /api/requests/leave/review` - Review request (Manager)
- `GET /api/requests/leave/user/{userId}` - User's leave requests
- `GET /api/requests/leave/pending` - Pending requests (Manager)
- `POST /api/requests/leave/{id}/cancel` - Cancel request

**Shift Change Requests:**
- `POST /api/shiftrequests` - Create shift change request
- `POST /api/shiftrequests/review` - Review request (Manager)
- `GET /api/shiftrequests/user/{userId}` - User's shift requests
- `GET /api/shiftrequests/pending` - Pending requests (Manager)

**Late Arrival Requests:**
- `POST /api/laterequests` - Create late request
- `POST /api/laterequests/review` - Review request (Manager)
- `GET /api/laterequests/user/{userId}` - User's late requests
- `GET /api/laterequests/pending` - Pending requests (Manager)

---

### 5️⃣ Payroll & Rewards
**Base Paths:** `/api/payroll`, `/api/rewardpenalty`

**Features:**
- Salary calculation
- Payroll generation
- Rewards and penalties management
- Salary adjustments
- Payroll reports

**Key Endpoints:**
- `POST /api/payroll/calculate` - Calculate payroll
- `GET /api/payroll/user/{userId}` - User's payroll history
- `POST /api/rewardpenalty` - Add reward/penalty (Manager)
- `GET /api/rewardpenalty/user/{userId}` - User's rewards/penalties

---

## 🔐 Authentication in Swagger

### Step 1: Login
1. Go to **1. Authentication** section
2. Use `POST /api/auth/login` endpoint
3. Enter credentials:
```json
{
  "username": "admin",
  "password": "your_password"
}
```
4. Copy the `token` from response

### Step 2: Authorize
1. Click **"Authorize"** button (🔒 icon) at top right
2. Enter: `Bearer YOUR_TOKEN_HERE`
3. Click **"Authorize"**
4. Now you can test all protected endpoints!

---

## 📊 Request/Response Examples

### Example 1: Check-in
**Request:**
```json
POST /api/attendance/check-in
{
  "userId": 1,
  "shiftId": 1,
  "latitude": 10.7769,
  "longitude": 106.7009,
  "imageUrl": "https://cloudinary.com/image.jpg",
  "notes": "Checked in from office"
}
```

**Response:**
```json
{
  "id": 41,
  "userId": 1,
  "userName": "John Doe",
  "shiftId": 1,
  "shiftName": "Morning Shift",
  "checkInTime": "2025-11-06T01:30:00Z",
  "status": "On Time",
  "checkInLat": 10.7769,
  "checkInLng": 106.7009,
  "images": [...]
}
```

### Example 2: Leave Request
**Request:**
```json
POST /api/requests/leave
{
  "userId": 3,
  "startDate": "2025-11-10",
  "endDate": "2025-11-12",
  "reason": "Family vacation"
}
```

**Response:**
```json
{
  "id": 1,
  "userId": 3,
  "userName": "Jane Smith",
  "startDate": "2025-11-10",
  "endDate": "2025-11-12",
  "totalDays": 3,
  "reason": "Family vacation",
  "status": "Pending",
  "createdAt": "2025-11-06T02:00:00Z"
}
```

---

## 🎯 Role-Based Access

### Employee Role
- ✅ Check-in/Check-out
- ✅ View own attendance
- ✅ Create requests (leave, shift, late)
- ✅ View own requests
- ❌ Review requests
- ❌ View all users
- ❌ Manage shifts

### Manager Role
- ✅ All Employee permissions
- ✅ Review requests (approve/reject)
- ✅ View pending requests
- ✅ Create manual attendance
- ✅ View team attendance reports
- ✅ Assign shifts
- ❌ Delete shifts

### Admin Role
- ✅ All Manager permissions
- ✅ Create/Delete shifts
- ✅ Manage users
- ✅ Full system access

---

## 🔧 Testing Tips

1. **Use Swagger UI** for quick testing
2. **Copy curl commands** from Swagger for automation
3. **Check response schemas** in Swagger documentation
4. **Test error cases** to see validation messages
5. **Use filters** in GET endpoints (status, dates, etc.)

---

## 📝 Status Values

### Request Status
- `Pending` - Waiting for approval
- `Approved` - Approved by manager
- `Rejected` - Rejected by manager
- `Cancelled` - Cancelled by employee

### Attendance Status
- `On Time` - Checked in on time
- `Late` - Checked in late
- `Manual Entry` - Created by manager

---

## 🚀 Quick Start Guide

1. **Start the API:**
   ```bash
   dotnet run
   ```

2. **Open Swagger:**
   ```
   http://localhost:5267/swagger
   ```

3. **Login as Admin:**
   - Use `/api/auth/login`
   - Get JWT token

4. **Authorize in Swagger:**
   - Click "Authorize" button
   - Enter: `Bearer YOUR_TOKEN`

5. **Test APIs:**
   - Try creating a shift
   - Assign shift to user
   - Test check-in/check-out
   - Create and review requests

---

## 📞 Support

For API issues or questions:
- Email: support@hrmcyberse.com
- Documentation: Check inline Swagger descriptions
- Testing Guide: See `REQUEST_API_TESTING.md` and `ATTENDANCE_API_TESTING.md`
