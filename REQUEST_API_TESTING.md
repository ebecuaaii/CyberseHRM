# Request Management API Testing Guide

## Quick Test Cases for Leave, Shift Change, and Late Requests

### Prerequisites
- JWT token (login as employee, manager, or admin)
- User must have shifts assigned for shift/late requests

---

## 1️⃣ LEAVE REQUEST (Nghỉ phép)

### Test Case 1: Create Leave Request
**POST** `/api/requests/leave`

```json
{
  "userId": 3,
  "startDate": "2025-11-10",
  "endDate": "2025-11-12",
  "reason": "Family vacation"
}
```

**Expected:** Status 200, returns leave request with `status: "Pending"`, `totalDays: 3`

### Test Case 2: Manager Review Leave Request
**POST** `/api/requests/leave/review`
**Auth:** Manager or Admin token

```json
{
  "requestId": 1,
  "status": "Approved",
  "reviewNotes": "Approved for vacation"
}
```

**Expected:** Status 200, request status changed to "Approved"

### Test Case 3: Get User's Leave Requests
**GET** `/api/requests/leave/user/3?status=Pending`

**Expected:** List of user's leave requests filtered by status

---

## 2️⃣ SHIFT REQUEST (Đổi ca)

### Test Case 1: Create Shift Change Request
**POST** `/api/shiftrequests`

```json
{
  "userId": 3,
  "shiftId": 2,
  "shiftDate": "2025-11-15",
  "reason": "Need to swap shift with colleague"
}
```

**Expected:** Status 200, returns shift request with `status: "Pending"`

### Test Case 2: Manager Review Shift Request
**POST** `/api/shiftrequests/review`
**Auth:** Manager or Admin token

```json
{
  "requestId": 1,
  "status": "Approved",
  "reviewNotes": "Shift change approved"
}
```

**Expected:** Status 200, request status changed to "Approved"

### Test Case 3: Get Pending Shift Requests (Manager)
**GET** `/api/shiftrequests/pending`
**Auth:** Manager or Admin token

**Expected:** List of all pending shift requests

---

## 3️⃣ LATE REQUEST (Xin đi trễ)

### Test Case 1: Create Late Arrival Request
**POST** `/api/laterequests`

```json
{
  "userId": 3,
  "shiftId": 1,
  "requestDate": "2025-11-08",
  "expectedArrivalTime": "07:00:00",
  "reason": "Doctor appointment"
}
```

**Note:** `expectedArrivalTime` phải sau `shift.starttime` (ví dụ: shift bắt đầu 06:30, xin đến 07:00)

**Expected:** Status 200, returns late request with calculated `lateMinutes: 30`

### Test Case 2: Manager Review Late Request
**POST** `/api/laterequests/review`
**Auth:** Manager or Admin token

```json
{
  "requestId": 1,
  "status": "Approved",
  "reviewNotes": "Approved for medical reason"
}
```

**Expected:** Status 200, request status changed to "Approved"

### Test Case 3: Get User's Late Requests
**GET** `/api/laterequests/user/3`

**Expected:** List of user's late requests

---

## 🔥 Quick Test Flow

1. **Employee creates requests:**
   - Leave request for 3 days
   - Shift change request
   - Late arrival request

2. **Manager reviews:**
   - Get pending requests: `/api/requests/leave/pending`
   - Approve/Reject each request

3. **Employee checks status:**
   - Get own requests to see approval status

---

## ⚠️ Error Cases to Test

### 1. Invalid Leave Request
```json
{
  "userId": 3,
  "startDate": "2025-11-15",
  "endDate": "2025-11-10",
  "reason": "Test"
}
```
**Expected:** 400 Bad Request - "End date must be after start date"

### 2. Late Time Before Shift Start
```json
{
  "userId": 3,
  "shiftId": 1,
  "requestDate": "2025-11-08",
  "expectedArrivalTime": "06:00:00",
  "reason": "Test"
}
```
**Expected:** 400 Bad Request - "Expected arrival time must be after shift start time"

### 3. Non-Manager Trying to Review
**POST** `/api/requests/leave/review` with Employee token

**Expected:** 403 Forbidden

---

## 📊 Status Values
- `Pending` - Chờ duyệt
- `Approved` - Đã duyệt
- `Rejected` - Từ chối
- `Cancelled` - Đã hủy

---

## 🎯 Ready for Production?

After testing these 3 cases successfully, you can move to **Phase 5**! 🚀
