# Hướng dẫn Admin Gán Role và Position cho Nhân viên

## 📋 Tổng quan

Khi nhân viên mới tạo tài khoản, họ sẽ **chưa có role và position**. Admin cần gán role và position cho nhân viên đó sau khi tạo tài khoản.

## 🔑 API Endpoint

### Update User (Admin only)

**Endpoint:** `PUT /api/auth/user/{id}`

**Authorization:** Chỉ Admin mới được phép

**Request Body:**
```json
{
    "roleName": "Employee",           // Tên role (Admin, Manager, Employee)
    "positionName": "Nhân viên",      // Tên position
    "departmentName": "Pha chế",      // Tên department
    "fullname": "Nguyễn Văn A",        // (Optional) Cập nhật họ tên
    "email": "nguyenvana@example.com", // (Optional) Cập nhật email
    "phone": "0123456789",            // (Optional) Cập nhật số điện thoại
    "isActive": true                   // (Optional) Kích hoạt/khóa tài khoản
}
```

**Response (200):**
```json
{
    "id": 9,
    "username": "thanhhien",
    "fullname": "nguyen thanh hien",
    "email": "thanhhien@example.com",
    "phone": "0123456789",
    "roleName": "Employee",
    "departmentName": "Thu ngân",
    "positionName": "Nhân viên",
    "isActive": true,
    "createdAt": "2024-01-01T00:00:00Z"
}
```

## 📝 Các bước thực hiện

### Bước 1: Lấy danh sách users chưa có role/position

```http
GET /api/auth/users/debug
Authorization: Bearer <admin_token>
```

Tìm các user có:
- `roleIsNull: true` → Chưa có role
- `positionIsNull: true` → Chưa có position

### Bước 2: Xem danh sách Roles, Positions, Departments có sẵn

```http
GET /api/lookups/roles
GET /api/lookups/positions
GET /api/lookups/departments
```

### Bước 3: Gán role và position cho user

```http
PUT /api/auth/user/{id}
Authorization: Bearer <admin_token>
Content-Type: application/json

{
    "roleName": "Employee",
    "positionName": "Nhân viên",
    "departmentName": "Pha chế"
}
```

### Bước 4: Kiểm tra kết quả

```http
GET /api/auth/user/{id}
Authorization: Bearer <admin_token>
```

## 🎯 Ví dụ sử dụng

### Ví dụ 1: Gán role và position cho user mới

User ID 9 (`thanhhien`) chưa có position:

**Request:**
```http
PUT http://localhost:5267/api/auth/user/9
Authorization: Bearer <admin_token>
Content-Type: application/json

{
    "roleName": "Employee",
    "positionName": "Nhân viên",
    "departmentName": "Thu ngân"
}
```

**Kết quả:** User sẽ có đầy đủ role, position và department.

### Ví dụ 2: Chỉ gán position (giữ nguyên role và department)

```http
PUT http://localhost:5267/api/auth/user/9
Authorization: Bearer <admin_token>
Content-Type: application/json

{
    "positionName": "Nhân viên"
}
```

### Ví dụ 3: Thay đổi role của user

```http
PUT http://localhost:5267/api/auth/user/17
Authorization: Bearer <admin_token>
Content-Type: application/json

{
    "roleName": "Manager",
    "positionName": "Quản lý",
    "departmentName": "Manager"
}
```

## ⚠️ Lưu ý quan trọng

1. **Chỉ Admin mới được phép**: Endpoint này yêu cầu role "Admin"
2. **Tên phải chính xác**: `roleName`, `positionName`, `departmentName` phải khớp với dữ liệu trong database
3. **Có thể update từng phần**: Không cần gửi tất cả fields, chỉ gửi những gì cần update
4. **Email unique**: Nếu update email, phải đảm bảo email chưa được sử dụng bởi user khác
5. **Validation**: API sẽ kiểm tra và trả về lỗi nếu:
   - Role/Position/Department không tồn tại
   - Email đã được sử dụng
   - User không tồn tại

## 🔍 Kiểm tra dữ liệu

### Xem user nào chưa có role/position:

```http
GET /api/auth/users/debug
```

Response sẽ cho biết:
- `roleIsNull: true` → Chưa có role
- `positionIsNull: true` → Chưa có position
- `departmentIsNull: true` → Chưa có department

### Xem danh sách users sau khi update:

```http
GET /api/auth/users
```

Kiểm tra field `roleName` và `positionName` đã có giá trị chưa.

## 🚀 Test với Postman

1. Import collection `Postman_Collection_Test_Users.json`
2. Login với tài khoản Admin
3. Chạy request **"Update User - Admin gán Role/Position/Department"**
4. Thay đổi `id` và body theo nhu cầu
5. Kiểm tra response để xác nhận đã update thành công

## 📊 Workflow đề xuất

1. **Nhân viên đăng ký** → Tài khoản được tạo (chưa có role/position)
2. **Admin xem danh sách** → `GET /api/auth/users/debug` để tìm user chưa có role/position
3. **Admin gán role/position** → `PUT /api/auth/user/{id}` với roleName, positionName, departmentName
4. **Kiểm tra kết quả** → `GET /api/auth/users` để xác nhận

## 🎯 Best Practices

1. **Gán role và position ngay sau khi tạo tài khoản** để tránh user không thể sử dụng hệ thống
2. **Sử dụng endpoint debug** để tìm user chưa có role/position
3. **Kiểm tra dữ liệu trước khi gán**: Đảm bảo role/position/department tồn tại
4. **Logging**: Tất cả thao tác update đều được log để audit



