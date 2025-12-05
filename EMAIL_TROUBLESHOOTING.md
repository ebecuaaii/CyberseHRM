# Email Troubleshooting Guide

## Vấn đề: Không nhận được email

### 1. Kiểm tra Console Log
Xem console có log:
- ✅ "Đã gửi email thông báo trúng tuyển đến..."
- ❌ "Lỗi khi gửi email đến..."
- ❌ "SMTP chưa được cấu hình..."

### 2. Kiểm tra App Password
```json
// SAI - có dấu cách
"Password": "qbmk qezk jfii frdm"

// ĐÚNG - không có dấu cách
"Password": "qbmkqezkjfiifrdm"
```

### 3. Kiểm tra Gmail Settings

**Bước 1: Bật 2-Step Verification**
- Vào: https://myaccount.google.com/security
- Tìm "2-Step Verification" → Bật

**Bước 2: Tạo App Password**
- Vào: https://myaccount.google.com/apppasswords
- Chọn "Mail" và "Other (Custom name)"
- Nhập tên: "HRM Cyberse"
- Copy password 16 ký tự (bỏ dấu cách)

**Bước 3: Update appsettings.json**
```json
"Email": {
  "SmtpHost": "smtp.gmail.com",
  "SmtpPort": 587,
  "Username": "your-email@gmail.com",
  "Password": "app-password-16-chars",
  "FromEmail": "your-email@gmail.com",
  "FromName": "HRM Cyberse"
}
```

**Bước 4: Restart App**

### 4. Kiểm tra Spam/Junk
- Gmail: Check tab "Spam" và "Promotions"
- Outlook: Check "Junk Email"
- Yahoo: Check "Spam"

### 5. Test API
```http
POST http://localhost:5267/api/EmployeeInvitation/test-email?toEmail=your-email@gmail.com
Authorization: Bearer {token}
```

### 6. Các lỗi thường gặp

**Lỗi: "Authentication failed"**
- App Password sai
- Chưa bật 2-Step Verification
- Dùng password thường thay vì App Password

**Lỗi: "Mailbox unavailable"**
- Email không tồn tại
- Email bị khóa

**Lỗi: "Connection timeout"**
- Port sai (phải là 587)
- Firewall block
- Network issue

### 7. Alternative: Dùng Outlook thay Gmail

```json
"Email": {
  "SmtpHost": "smtp-mail.outlook.com",
  "SmtpPort": 587,
  "Username": "your-email@outlook.com",
  "Password": "your-password",
  "FromEmail": "your-email@outlook.com",
  "FromName": "HRM Cyberse"
}
```

### 8. Alternative: Dùng SendGrid (Free tier)

1. Đăng ký: https://sendgrid.com/
2. Tạo API Key
3. Update code để dùng SendGrid API

### 9. Debug Mode

Thêm vào appsettings.Development.json:
```json
"Logging": {
  "LogLevel": {
    "Default": "Debug",
    "Microsoft.AspNetCore": "Debug"
  }
}
```

### 10. Check Email Sent từ Gmail

Vào Gmail → Settings → "See all settings" → "Accounts and Import" → "Send mail as"
- Đảm bảo email được verify

### 11. Kiểm tra Daily Limit

Gmail có giới hạn:
- Free: 500 emails/day
- Google Workspace: 2000 emails/day

Nếu vượt limit, email sẽ không gửi được.

## Test Command

```bash
# Test SMTP connection
telnet smtp.gmail.com 587

# Nếu connect được sẽ thấy:
# 220 smtp.gmail.com ESMTP
```

## Contact Support

Nếu vẫn không được, có thể:
1. Thử email khác (Outlook, Yahoo)
2. Dùng service như SendGrid, Mailgun
3. Check Gmail security alerts: https://myaccount.google.com/notifications
