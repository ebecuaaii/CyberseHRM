# Quick Start - Thông Báo Trúng Tuyển

## Gửi thông báo trúng tuyển

```bash
POST /api/EmployeeInvitation
Authorization: Bearer {admin_token}

{
  "email": "newemployee@company.com",
  "branchId": 1,
  "roleId": 3,
  "departmentId": 1,
  "positionId": 1,
  "salaryRate": 50000
}
```

## Nhân viên nhận email thông báo trúng tuyển

Email sẽ chứa thông tin:
- **Department Name**: Phòng ban (VD: "Phòng IT")
- **Position Title**: Chức vụ (VD: "Senior Developer")
- **Salary Rate**: Mức lương (VD: "50,000 VND")
- **Role Name**: Vai trò (VD: "Employee")
- **Branch Code**: Mã chi nhánh (VD: "HN001")

## Nhân viên tự đăng ký tài khoản

Sau khi nhận email, nhân viên tự đăng ký tài khoản với Branch Code đã được cung cấp:

```bash
POST /api/Auth/register

{
  "username": "newuser",
  "password": "password123",
  "fullName": "Nguyễn Văn A",
  "email": "newemployee@company.com",
  "phone": "0901234567",
  "branchCode": "HN001"
}
```

Hệ thống sẽ tự động gán đúng department, position, salary và role theo thông tin đã được thiết lập trong invitation.

## Cấu hình Email (Optional)

Nếu không config SMTP, email sẽ log ra console (dev mode).

```json
// appsettings.json
{
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587",
    "Username": "your-email@gmail.com",
    "Password": "your-app-password"
  }
}
```

## Files đã tạo

- ✅ `Controllers/EmployeeInvitationController.cs` - API quản lý invitation
- ✅ `Services/EmailService.cs` - Service gửi email
- ✅ `DTOs/EmployeeInvitationDto.cs` - DTOs
- ✅ `employee_invitation_api.http` - Test file
- ✅ `EMPLOYEE_INVITATION_GUIDE.md` - Hướng dẫn chi tiết

## Test ngay

1. Login admin: `POST /api/Auth/login`
2. Gửi thông báo trúng tuyển: `POST /api/EmployeeInvitation`
3. Nhân viên nhận email với thông tin department, position, salary, role và branch code
4. Nhân viên tự đăng ký: `POST /api/Auth/register` với branch code đã nhận
