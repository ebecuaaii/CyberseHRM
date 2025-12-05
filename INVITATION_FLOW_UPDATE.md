# Cập nhật Flow Invitation - Thông báo trúng tuyển

## Thay đổi chính

### Flow CŨ (Legacy - vẫn hoạt động):
1. Admin tạo invitation → Email có link accept
2. Nhân viên click link → Điền form với token
3. API `POST /api/Auth/accept-invitation` với token

### Flow MỚI (Recommended):
1. Admin tạo invitation → Email thông báo trúng tuyển
2. Email chứa: Department, Position, Salary, Role, **Branch Code**
3. Nhân viên tự đăng ký: `POST /api/Auth/register` với **branchCode**
4. Hệ thống tự động match invitation và gán thông tin

## API Changes

### 1. Email Service - Thêm thông tin vào email

**Trước:**
```csharp
SendEmployeeInvitationAsync(email, branchCode, token, branchName)
```

**Sau:**
```csharp
SendEmployeeInvitationAsync(email, branchCode, token, branchName, 
    departmentName, positionTitle, salaryRate, roleName)
```

### 2. Register API - Thêm branchCode

**RegisterDto:**
```csharp
public class RegisterDto
{
    // ... existing fields
    public string? BranchCode { get; set; }  // NEW
}
```

**Logic:**
- Nếu có `branchCode`, tìm invitation chưa dùng cho email + branch
- Tự động gán: department, position, salary, role từ invitation
- Đánh dấu invitation đã sử dụng

## Email Template Mới

```html
🎉 Chúc mừng! Bạn đã trúng tuyển

Thông tin công việc:
- Phòng ban: IT
- Chức vụ: Senior Developer  
- Mức lương: 50,000 VND
- Vai trò: Employee

Mã chi nhánh: HN001

Vui lòng đăng ký tài khoản với mã chi nhánh trên.
```

## Testing

```http
# 1. Admin tạo invitation
POST /api/EmployeeInvitation
{
  "email": "test@company.com",
  "branchId": 1,
  "roleId": 3,
  "departmentId": 1,
  "positionId": 1,
  "salaryRate": 50000
}

# 2. Nhân viên nhận email với Branch Code: HN001

# 3. Nhân viên đăng ký
POST /api/Auth/register
{
  "username": "newuser",
  "password": "pass123",
  "confirmPassword": "pass123",
  "fullname": "Nguyễn Văn A",
  "email": "test@company.com",
  "phone": "0901234567",
  "branchCode": "HN001"  // Từ email
}

# Hệ thống tự động gán department, position, salary, role
```

## Files Updated

- ✅ `Services/EmailService.cs` - Email template mới
- ✅ `Services/IEmailService.cs` - Interface signature
- ✅ `Controllers/EmployeeInvitationController.cs` - Truyền thông tin vào email
- ✅ `Controllers/AuthController.cs` - Logic register với branchCode
- ✅ `DTOs/RegisterDto.cs` - Thêm BranchCode field
- ✅ `INVITATION_QUICK_START.md` - Cập nhật hướng dẫn
- ✅ `EMPLOYEE_INVITATION_GUIDE.md` - Cập nhật chi tiết
- ✅ `employee_invitation_api.http` - Cập nhật test cases

## Backward Compatibility

API cũ `POST /api/Auth/accept-invitation` vẫn hoạt động bình thường cho các hệ thống đang dùng.
