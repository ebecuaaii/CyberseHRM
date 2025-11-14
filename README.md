# CyberseHRM - Human Resource Management System

Complete HRM system built with ASP.NET Core 9.0 and PostgreSQL, featuring 5 main modules for comprehensive employee management.

## 🚀 Features

### 1. Authentication & User Management
- User registration and login with JWT authentication
- Role-based access control (Admin, Manager, Employee)
- Secure password hashing with BCrypt
- User profile management

### 2. Shift Management
- Create and manage work shifts
- Assign shifts to employees
- View shift schedules and assignments
- Shift templates with caching for performance

### 3. Attendance Management
- Check-in/Check-out with GPS coordinates
- Photo capture for attendance verification
- Automatic late detection based on shift times
- Attendance reports and analytics
- Manual attendance entry for managers
- Attendance summary with statistics

### 4. Request Management
- **Leave Requests:** Vacation and sick leave with approval workflow
- **Shift Change Requests:** Request to change assigned shifts
- **Late Arrival Requests:** Request permission to arrive late with expected time
- Manager approval/rejection workflow
- Request history and status tracking

### 5. Payroll & Rewards
- Automatic payroll generation based on attendance
- Rewards and penalties management
- Salary calculation with overtime
- Payroll reports and summaries
- Payroll adjustments and updates

## 🛠️ Technology Stack

- **Backend:** ASP.NET Core 9.0
- **Database:** PostgreSQL
- **ORM:** Entity Framework Core 9.0
- **Authentication:** JWT Bearer Token
- **API Documentation:** Swagger/OpenAPI
- **Caching:** MemoryCache
- **Password Hashing:** BCrypt.Net

## 📋 Prerequisites

- .NET 9.0 SDK
- PostgreSQL 14+
- Visual Studio 2022 or VS Code

## 🔧 Installation

1. **Clone the repository:**
```bash
git clone https://github.com/ebecuaaii/CyberseHRM.git
cd CyberseHRM
```

2. **Configure database connection:**

Edit `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=cybersehrm;Username=postgres;Password=your_password"
  }
}
```

3. **Run database migrations:**

Execute SQL scripts in `Database/` folder in PostgreSQL.

4. **Build and run:**
```bash
dotnet restore
dotnet build
dotnet run
```

5. **Access Swagger UI:**
```
http://localhost:5267/swagger
```

## 📚 API Documentation

### Swagger UI
Access interactive API documentation at: `http://localhost:5267/swagger`

### Documentation Files
- `SWAGGER_DOCUMENTATION.md` - Complete Swagger guide
- `COMPLETE_API_SUMMARY.md` - System overview
- `ATTENDANCE_API_TESTING.md` - Attendance API testing
- `REQUEST_API_TESTING.md` - Request API testing
- `PAYROLL_API_TESTING.md` - Payroll API testing
- `SHIFT_MANAGEMENT_API_TESTING.md` - Shift API testing

## 🔐 Authentication

### Login
```bash
POST /api/auth/login
{
  "username": "admin",
  "password": "your_password"
}
```

### Use JWT Token
Add to request headers:
```
Authorization: Bearer YOUR_JWT_TOKEN
```

## 🎯 Quick Start

1. **Login to get JWT token**
2. **Authorize in Swagger UI** (click 🔒 button)
3. **Test APIs:**
   - Create shifts
   - Assign shifts to employees
   - Test check-in/check-out
   - Create and approve requests
   - Generate payroll

## 📊 Database Schema

### Core Tables
- `users` - User accounts and profiles
- `roles` - User roles
- `departments` - Company departments
- `shifts` - Work shift definitions
- `attendances` - Check-in/check-out records
- `leaverequests` - Leave requests
- `shiftrequests` - Shift change requests
- `laterequests` - Late arrival requests
- `payrolls` - Monthly payroll records
- `rewardpenalties` - Rewards and penalties

## 🔑 User Roles

### Employee
- Check-in/Check-out
- View own attendance
- Create requests (leave, shift change, late)
- View own payroll

### Manager
- All Employee permissions
- Approve/Reject requests
- View team attendance
- Generate payroll
- Add rewards/penalties
- Assign shifts

### Admin
- All Manager permissions
- Create/Delete shifts
- Manage users
- Full system access

## 🧪 Testing

Run the application and use Swagger UI for testing all endpoints.

See testing guides in documentation files for detailed test cases.

## 📁 Project Structure

```
HRMCyberse/
├── Controllers/        # API endpoints
├── Services/          # Business logic
├── DTOs/              # Data transfer objects
├── Models/            # Database entities
├── Data/              # DbContext
├── Constants/         # Constant values
├── Database/          # SQL migrations
└── Documentation/     # API testing guides
```

## 🚀 Performance Features

- Response compression (Gzip)
- Memory caching for frequently accessed data
- Optimized database queries with AsNoTracking
- Connection pooling
- Indexed database columns

## 📝 API Endpoints Overview

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login

### Shifts
- `GET /api/shifts` - Get all shifts
- `POST /api/shifts` - Create shift
- `POST /api/shifts/assign` - Assign shift

### Attendance
- `POST /api/attendance/check-in` - Check in
- `POST /api/attendance/check-out` - Check out
- `GET /api/attendance/history/{userId}` - Get history
- `POST /api/attendance/report` - Generate report

### Requests
- `POST /api/requests/leave` - Create leave request
- `POST /api/shiftrequests` - Create shift request
- `POST /api/laterequests` - Create late request
- `POST /api/requests/leave/review` - Review request

### Payroll
- `POST /api/payroll/generate` - Generate payroll
- `GET /api/payroll/user/{userId}` - Get user payroll
- `POST /api/rewardpenalty` - Add reward/penalty

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📄 License

This project is licensed under the MIT License.

## 👥 Authors

- **HRM Cyberse Team**

## 📞 Support

For support, email support@hrmcyberse.com or open an issue in the repository.

## 🎉 Status

**Version:** 1.0.0  
**Status:** Production Ready ✅

All 5 main features are complete and tested!
