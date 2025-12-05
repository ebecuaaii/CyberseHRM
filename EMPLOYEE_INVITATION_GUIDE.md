# Hướng dẫn sử dụng tính năng mời nhân viên qua Email

## Tổng quan

Tính năng này cho phép Admin/Manager mời nhân viên mới tham gia hệ thống qua email. Email sẽ chứa:
- **Branch Code**: Mã chi nhánh để nhân viên biết mình thuộc chi nhánh nào
- **Invitation Link**: Link để nhân viên tạo tài khoản
- Thông tin về role, department, position, salary đã được cấu hình sẵn

## Flow hoạt động

### Flow mới (Recommended):
```
1. Admin/Manager tạo invitation → Hệ thống gửi email thông báo trúng tuyển
2. Nhân viên nhận email với thông tin: Department, Position, Salary, Role, Branch Code
3. Nhân viên tự đăng ký tài khoản với Branch Code
4. Hệ thống tự động match invitation và gán thông tin công việc
```

### Flow cũ (Legacy - vẫn hoạt động):
```
1. Admin/Manager tạo invitation → Hệ thống gửi email với link
2. Nhân viên nhận email → Click link hoặc dùng token
3. Nhân viên xem thông tin invitation → Điền form đăng ký
4. Hệ thống tạo tài khoản → Tự động login
```

## API Endpoints

### 1. Tạo lời mời (Admin/Manager)

**POST** `/api/EmployeeInvitation`

**Headers:**
```
Authorization: Bearer {token}
Content-Type: application/json
```

**Body:**
```json
{
  "email": "newemployee@example.com",
  "branchId": 1,
  "roleId": 3,
  "departmentId": 1,
  "positionId": 1,
  "salaryRate": 50000
}
```

**Response:**
```json
{
  "id": 1,
  "email": "newemployee@example.com",
  "branchCode": "HN001",
  "branchName": "Chi nhánh Hà Nội",
  "roleName": "Employee",
  "departmentName": "IT",
  "positionName": "Developer",
  "salaryRate": 50000,
  "invitationToken": "abc123xyz...",
  "expiresAt": "2024-12-12T10:00:00Z",
  "isUsed": false,
  "createdByName": "Admin User",
  "createdAt": "2024-12-05T10:00:00Z"
}
```

### 2. Lấy danh sách lời mời

**GET** `/api/EmployeeInvitation?isUsed=false&includeExpired=false`

**Query Parameters:**
- `isUsed` (optional): `true` | `false` - Lọc theo trạng thái đã sử dụng
- `includeExpired` (optional): `true` | `false` - Có bao gồm lời mời hết hạn không

### 3. Gửi lại email

**POST** `/api/EmployeeInvitation/{id}/resend`

Tự động gia hạn thêm 7 ngày nếu đã hết hạn.

### 4. Xóa lời mời

**DELETE** `/api/EmployeeInvitation/{id}`

Chỉ xóa được lời mời chưa sử dụng.

### 5. Xem thông tin invitation (Public - không cần auth)

**GET** `/api/Auth/invitation/{token}`

**Response:**
```json
{
  "email": "newemployee@example.com",
  "branchCode": "HN001",
  "branchName": "Chi nhánh Hà Nội",
  "roleName": "Employee",
  "departmentName": "IT",
  "positionName": "Developer",
  "salaryRate": 50000,
  "expiresAt": "2024-12-12T10:00:00Z",
  "isExpired": false,
  "isUsed": false
}
```

### 6. Đăng ký tài khoản với Branch Code (RECOMMENDED - Public)

**POST** `/api/Auth/register`

**Body:**
```json
{
  "username": "newuser123",
  "password": "SecurePass123!",
  "confirmPassword": "SecurePass123!",
  "fullname": "Nguyễn Văn A",
  "email": "newemployee@example.com",
  "phone": "0901234567",
  "branchCode": "HN001"
}
```

**Response:**
```json
{
  "id": 10,
  "username": "newuser123",
  "fullname": "Nguyễn Văn A",
  "email": "newemployee@example.com",
  "phone": "0901234567",
  "roleName": "Employee",
  "departmentName": "IT",
  "positionName": "Developer",
  "isActive": true,
  "createdAt": "2024-12-05T10:00:00Z"
}
```

**Lưu ý:**
- Hệ thống tự động tìm invitation chưa sử dụng cho email và branch
- Tự động gán department, position, salary, role từ invitation
- Đánh dấu invitation đã sử dụng

### 6.1. (Legacy) Chấp nhận lời mời - API cũ vẫn hoạt động

**POST** `/api/Auth/accept-invitation`

**Body:**
```json
{
  "token": "abc123xyz...",
  "username": "newuser123",
  "password": "SecurePass123!",
  "fullName": "Nguyễn Văn A",
  "phone": "0901234567"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Tạo tài khoản thành công",
  "user": {
    "id": 10,
    "username": "newuser123",
    "fullName": "Nguyễn Văn A",
    "email": "newemployee@example.com",
    "roleName": "Employee",
    "departmentName": "IT",
    "positionName": "Developer"
  },
  "token": "jwt_token_here..."
}
```

## Email Template

Email thông báo trúng tuyển sẽ có dạng:

```
┌─────────────────────────────────────┐
│  🎉 Chúc mừng! Bạn đã trúng tuyển  │
└─────────────────────────────────────┘

Xin chào,

Chúc mừng bạn đã trúng tuyển vào Chi nhánh Hà Nội!

┌─────────────────────────────────────┐
│ Thông tin công việc của bạn:       │
│                                     │
│ Phòng ban: IT                       │
│ Chức vụ: Senior Developer           │
│ Mức lương: 50,000 VND               │
│ Vai trò: Employee                   │
└─────────────────────────────────────┘

Mã chi nhánh để đăng ký tài khoản:
┌─────────────┐
│    HN001    │
└─────────────┘

Vui lòng sử dụng mã chi nhánh trên để đăng ký tài khoản 
trên hệ thống HRM Cyberse.

Sau khi đăng ký, hệ thống sẽ tự động gán cho bạn 
các thông tin công việc đã được thiết lập.

Lưu ý: Vui lòng đăng ký trong vòng 7 ngày kể từ khi nhận email này.
```

## Cấu hình Email (appsettings.json)

```json
{
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587",
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "FromEmail": "noreply@hrmcyberse.com",
    "FromName": "HRM Cyberse"
  },
  "App": {
    "BaseUrl": "http://localhost:5267"
  }
}
```

### Cấu hình Gmail SMTP

1. Bật 2-Step Verification trong Google Account
2. Tạo App Password: https://myaccount.google.com/apppasswords
3. Sử dụng App Password thay vì password thường

### Chế độ Development (không có SMTP)

Nếu chưa cấu hình SMTP, email sẽ được log ra console thay vì gửi thật:

```
SMTP chưa được cấu hình. Email invitation:
To: newemployee@example.com
Branch Code: HN001
Token: abc123xyz...
Link: http://localhost:5267/api/auth/accept-invitation?token=abc123xyz...
```

## Database Schema

### Bảng `employee_invitations`

```sql
CREATE TABLE employee_invitations (
    id SERIAL PRIMARY KEY,
    email VARCHAR(100) NOT NULL,
    branch_id INTEGER REFERENCES branches(id),
    roleid INTEGER REFERENCES roles(id),
    departmentid INTEGER REFERENCES departments(id),
    positionid INTEGER REFERENCES positiontitles(id),
    salaryrate NUMERIC(10,2),
    invitation_token VARCHAR(255) UNIQUE NOT NULL,
    expires_at TIMESTAMP NOT NULL,
    is_used BOOLEAN DEFAULT FALSE,
    used_at TIMESTAMP,
    created_by INTEGER REFERENCES users(id),
    created_at TIMESTAMP DEFAULT NOW()
);
```

## Security Features

1. **Token bảo mật**: Sử dụng RandomNumberGenerator để tạo token 32 bytes
2. **Hết hạn tự động**: Invitation hết hạn sau 7 ngày
3. **Một lần sử dụng**: Token chỉ dùng được 1 lần
4. **Kiểm tra email**: Không cho phép email trùng lặp
5. **Validation**: Kiểm tra branch, role, department, position tồn tại

## Testing

Sử dụng file `employee_invitation_api.http` để test:

```bash
# 1. Login để lấy token
POST /api/Auth/login

# 2. Tạo invitation
POST /api/EmployeeInvitation

# 3. Copy invitation token từ response

# 4. Test accept invitation (không cần auth)
POST /api/Auth/accept-invitation
```

## Frontend Integration

### React Native Example

```javascript
// 1. Admin tạo invitation (gửi email thông báo trúng tuyển)
const createInvitation = async (data) => {
  const response = await fetch('http://api/EmployeeInvitation', {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(data)
  });
  return response.json();
};

// 2. Nhân viên đăng ký với Branch Code (RECOMMENDED)
const registerWithBranchCode = async (data) => {
  const response = await fetch('http://api/Auth/register', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      username: data.username,
      password: data.password,
      confirmPassword: data.password,
      fullname: data.fullname,
      email: data.email,
      phone: data.phone,
      branchCode: data.branchCode // Từ email
    })
  });
  return response.json();
};

// 3. (Legacy) Nhân viên xem invitation
const getInvitationDetails = async (token) => {
  const response = await fetch(`http://api/Auth/invitation/${token}`);
  return response.json();
};

// 4. (Legacy) Nhân viên accept invitation
const acceptInvitation = async (data) => {
  const response = await fetch('http://api/Auth/accept-invitation', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(data)
  });
  return response.json();
};
```

## Troubleshooting

### Email không gửi được

1. Kiểm tra SMTP config trong appsettings.json
2. Kiểm tra log console để xem error
3. Test với Gmail App Password
4. Kiểm tra firewall/network

### Token không hợp lệ

1. Kiểm tra token có đúng không (copy đầy đủ)
2. Kiểm tra invitation đã hết hạn chưa
3. Kiểm tra invitation đã được sử dụng chưa

### Không tạo được tài khoản

1. Kiểm tra username đã tồn tại chưa
2. Kiểm tra password đủ mạnh chưa (min 6 ký tự)
3. Kiểm tra branch/role/department/position có tồn tại không

## Notes

- Invitation token hết hạn sau 7 ngày
- Có thể resend email để gia hạn thêm 7 ngày
- Khi accept invitation, user được tự động login
- Salary rate từ invitation sẽ được copy sang user
- Branch code hiển thị trong email để nhân viên dễ nhận biết
