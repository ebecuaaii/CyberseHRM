# Hướng dẫn Test API Users với Postman

## 📥 Import Collection vào Postman

1. Mở Postman
2. Click **Import** (góc trên bên trái)
3. Chọn file `Postman_Collection_Test_Users.json`
4. Collection sẽ xuất hiện trong sidebar

## 🚀 Cách Test

### Bước 1: Login để lấy Token

1. Mở request **"1. Authentication > Login - Lấy JWT Token"**
2. Thay đổi `username` và `password` trong body nếu cần (mặc định: `admin` / `123456`)
3. Click **Send**
4. Token sẽ tự động được lưu vào collection variable `token`

**Request:**
```http
POST http://localhost:5267/api/auth/login
Content-Type: application/json

{
    "username": "admin",
    "password": "123456"
}
```

### Bước 2: Test Get All Users

1. Mở request **"2. Users Management > Get All Users - Lấy danh sách nhân viên"**
2. Click **Send**
3. Xem kết quả trong **Test Results** tab:
   - ✅ Kiểm tra status code = 200
   - ✅ Kiểm tra response là array
   - ✅ Kiểm tra có field `roleName` và `positionName`
   - ⚠️ Cảnh báo nếu không có user nào có role/position

**Request:**
```http
GET http://localhost:5267/api/auth/users
Authorization: Bearer {{token}}
```

**Response mong đợi:**
```json
[
  {
    "id": 1,
    "username": "admin",
    "fullname": "Admin User",
    "email": "admin@example.com",
    "phone": "0123456789",
    "roleName": "Admin",        // ← Kiểm tra field này
    "departmentName": "IT",
    "positionName": "Manager",  // ← Kiểm tra field này
    "isActive": true,
    "createdAt": "2024-01-01T00:00:00Z"
  }
]
```

### Bước 3: Debug nếu Role/Position bị null

1. Mở request **"Get Users Debug - Chi tiết debug"**
2. Click **Send**
3. Xem trong **Console** (View > Show Postman Console):
   - `roleid`: ID của role (có thể null)
   - `roleIsNull`: true nếu không có role
   - `positionid`: ID của position (có thể null)
   - `positionIsNull`: true nếu không có position

**Request:**
```http
GET http://localhost:5267/api/auth/users/debug
Authorization: Bearer {{token}}
```

### Bước 4: Fix Data nếu cần

Nếu users không có role/position, chạy endpoint này để gán giá trị mặc định:

1. Mở request **"Fix User Data - Gán Role/Position mặc định"**
2. Click **Send**
3. Xem kết quả:
   - `fixedCount`: Số user đã được fix
   - `defaultRole`: Role mặc định được gán
   - `defaultPosition`: Position mặc định được gán

**Request:**
```http
POST http://localhost:5267/api/auth/users/fix-data
Authorization: Bearer {{token}}
```

## 🔧 Cấu hình Base URL

Nếu server chạy ở port khác, sửa collection variable:

1. Click vào collection name
2. Vào tab **Variables**
3. Sửa `baseUrl` nếu cần:
   - Mặc định: `http://localhost:5267/api`
   - Nếu chạy HTTPS: `https://localhost:7084/api`

## 📊 Kiểm tra Kết quả

### ✅ Thành công nếu:
- Response có field `roleName` và `positionName`
- Giá trị không null (trừ khi user thật sự chưa có role/position)
- Test cases pass

### ❌ Vấn đề nếu:
- `roleName` và `positionName` luôn null
- Test case fail
- Console log cảnh báo không có role/position

### 🔍 Debug Steps:
1. Chạy **Get Users Debug** để xem `roleid` và `positionid`
2. Nếu `roleid`/`positionid` = null → Chạy **Fix User Data**
3. Sau khi fix, chạy lại **Get All Users** để kiểm tra

## 📝 Test Scripts

Collection đã có sẵn test scripts tự động:
- Tự động lưu token sau khi login
- Tự động kiểm tra response structure
- Tự động đếm số user có role/position
- Cảnh báo nếu không có role/position

Xem kết quả trong tab **Test Results** sau mỗi request.

## 🎯 Quick Test Commands

### Test nhanh với cURL:

```bash
# 1. Login
curl -X POST http://localhost:5267/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"123456"}'

# 2. Lấy token từ response, sau đó:
curl -X GET http://localhost:5267/api/auth/users \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"

# 3. Debug
curl -X GET http://localhost:5267/api/auth/users/debug \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"

# 4. Fix data
curl -X POST http://localhost:5267/api/auth/users/fix-data \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

## ⚠️ Lưu ý

1. **Token hết hạn**: Nếu gặp lỗi 401, login lại để lấy token mới
2. **Server chưa chạy**: Đảm bảo backend đang chạy ở `http://localhost:5267`
3. **CORS**: Nếu test từ browser, có thể gặp CORS error (Postman không bị)
4. **Database**: Đảm bảo database có dữ liệu users, roles, và positiontitles



