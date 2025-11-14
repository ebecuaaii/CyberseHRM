# Payroll & Rewards API Testing Guide

## Quick Test Cases for Payroll and Reward/Penalty Management

### Prerequisites
- JWT token (Manager or Admin for most operations)
- Users must have attendance records for payroll calculation
- Salary details must be configured for users

---

## 1️⃣ REWARD & PENALTY MANAGEMENT

### Test Case 1: Add Reward
**POST** `/api/rewardpenalty`
**Auth:** Manager or Admin token

```json
{
  "userId": 3,
  "type": "Reward",
  "amount": 500000,
  "reason": "Outstanding performance this month",
  "effectiveMonth": 11,
  "effectiveYear": 2025
}
```

**Expected:** Status 200, returns reward record with details

### Test Case 2: Add Penalty
**POST** `/api/rewardpenalty`
**Auth:** Manager or Admin token

```json
{
  "userId": 3,
  "type": "Penalty",
  "amount": 200000,
  "reason": "Late arrival 3 times this month",
  "effectiveMonth": 11,
  "effectiveYear": 2025
}
```

**Expected:** Status 200, returns penalty record

### Test Case 3: Get User's Rewards/Penalties
**GET** `/api/rewardpenalty/user/3?month=11&year=2025`

**Expected:** List of user's rewards and penalties for November 2025

### Test Case 4: Delete Reward/Penalty
**DELETE** `/api/rewardpenalty/1`
**Auth:** Manager or Admin token

**Expected:** Status 200, record deleted

---

## 2️⃣ PAYROLL GENERATION

### Test Case 1: Generate Payroll for All Users
**POST** `/api/payroll/generate`
**Auth:** Manager or Admin token

```json
{
  "month": 11,
  "year": 2025,
  "userId": null
}
```

**Expected:** Status 200, returns list of generated payroll records for all active users

**Calculation includes:**
- Base salary
- Worked hours
- Overtime
- Rewards
- Penalties
- Deductions
- Net salary

### Test Case 2: Generate Payroll for Specific User
**POST** `/api/payroll/generate`
**Auth:** Manager or Admin token

```json
{
  "month": 11,
  "year": 2025,
  "userId": 3
}
```

**Expected:** Status 200, returns payroll for user 3 only

### Test Case 3: Get Payroll by ID
**GET** `/api/payroll/1`

**Expected:** Detailed payroll information

**Response Example:**
```json
{
  "id": 1,
  "userId": 3,
  "userName": "John Doe",
  "month": 11,
  "year": 2025,
  "baseSalary": 10000000,
  "workedHours": 176,
  "overtimeHours": 8,
  "overtimePay": 500000,
  "rewards": 500000,
  "penalties": 200000,
  "deductions": 1000000,
  "netSalary": 9800000,
  "status": "Pending",
  "createdAt": "2025-11-06T10:00:00Z"
}
```

---

## 3️⃣ PAYROLL QUERIES

### Test Case 1: Get User's Payroll for Specific Month
**GET** `/api/payroll/user/3?month=11&year=2025`

**Expected:** Payroll record for user 3 in November 2025

### Test Case 2: Get User's Payroll History
**GET** `/api/payroll/user/3/history`

**Expected:** List of all payroll records for user 3, ordered by date

### Test Case 3: Get Payroll Summary (Manager/Admin)
**GET** `/api/payroll/summary?month=11&year=2025`
**Auth:** Manager or Admin token

**Expected:** Summary statistics for the month

**Response Example:**
```json
{
  "month": 11,
  "year": 2025,
  "totalEmployees": 50,
  "totalBaseSalary": 500000000,
  "totalRewards": 10000000,
  "totalPenalties": 5000000,
  "totalDeductions": 50000000,
  "totalNetSalary": 455000000,
  "averageSalary": 9100000
}
```

---

## 4️⃣ PAYROLL UPDATE

### Test Case 1: Update Payroll
**PUT** `/api/payroll/update`
**Auth:** Manager or Admin token

```json
{
  "payrollId": 1,
  "bonuses": 1000000,
  "deductions": 500000,
  "notes": "Added year-end bonus"
}
```

**Expected:** Status 200, updated payroll record

---

## 🔥 Complete Test Flow

### Step 1: Setup (Manager/Admin)
1. **Add rewards for good employees:**
   ```
   POST /api/rewardpenalty
   - Type: Reward
   - Amount: 500,000 VND
   ```

2. **Add penalties for late employees:**
   ```
   POST /api/rewardpenalty
   - Type: Penalty
   - Amount: 200,000 VND
   ```

### Step 2: Generate Payroll (Manager/Admin)
```
POST /api/payroll/generate
{
  "month": 11,
  "year": 2025
}
```

### Step 3: Review Payroll
1. **Get summary:**
   ```
   GET /api/payroll/summary?month=11&year=2025
   ```

2. **Check individual payrolls:**
   ```
   GET /api/payroll/user/3?month=11&year=2025
   ```

### Step 4: Adjust if Needed
```
PUT /api/payroll/update
{
  "payrollId": 1,
  "bonuses": 1000000,
  "notes": "Year-end bonus"
}
```

### Step 5: Employee Views Own Payroll
```
GET /api/payroll/user/3/history
```

---

## ⚠️ Error Cases to Test

### 1. Generate Payroll Without Attendance
**Scenario:** User has no attendance records for the month

**Expected:** Payroll generated with 0 worked hours, only base salary

### 2. Duplicate Payroll Generation
**Scenario:** Try to generate payroll for same month/year twice

**Expected:** Skip existing payroll or return error

### 3. Invalid Month/Year
```json
{
  "month": 13,
  "year": 2025
}
```

**Expected:** 400 Bad Request - Invalid month

### 4. Non-Manager Trying to Generate Payroll
**Auth:** Employee token

**Expected:** 403 Forbidden

---

## 📊 Payroll Calculation Formula

```
Net Salary = Base Salary 
           + Overtime Pay 
           + Rewards 
           + Bonuses
           - Penalties 
           - Deductions
```

**Where:**
- **Base Salary:** From `salarydetails` table
- **Overtime Pay:** Extra hours × hourly rate × overtime multiplier
- **Rewards:** Sum of rewards for the month
- **Penalties:** Sum of penalties for the month
- **Deductions:** Tax, insurance, etc.

---

## 🎯 Business Rules

### Rewards
- ✅ Can be added anytime by Manager/Admin
- ✅ Applied to specific month/year
- ✅ Positive amount only
- ✅ Requires reason

### Penalties
- ✅ Can be added anytime by Manager/Admin
- ✅ Applied to specific month/year
- ✅ Positive amount only (deducted from salary)
- ✅ Requires reason

### Payroll
- ✅ Generated monthly by Manager/Admin
- ✅ Based on attendance records
- ✅ Includes all rewards/penalties for that month
- ✅ Can be updated before finalization
- ❌ Cannot be deleted (audit trail)

---

## 📝 Status Values

### Payroll Status
- `Pending` - Generated, not yet approved
- `Approved` - Approved by manager
- `Paid` - Payment completed
- `Cancelled` - Cancelled (rare)

### Reward/Penalty Type
- `Reward` - Bonus, incentive
- `Penalty` - Fine, deduction

---

## 🚀 Quick Start

1. **Add some rewards/penalties:**
   - Test adding rewards for good performance
   - Test adding penalties for violations

2. **Generate payroll:**
   - Generate for current month
   - Check calculations

3. **Review and adjust:**
   - Get summary
   - Update if needed

4. **Employee checks:**
   - View own payroll history
   - See breakdown of salary

---

## 📞 Testing Checklist

- [ ] Add reward successfully
- [ ] Add penalty successfully
- [ ] Generate payroll for all users
- [ ] Generate payroll for specific user
- [ ] Get payroll summary
- [ ] Get user's payroll history
- [ ] Update payroll
- [ ] Delete reward/penalty
- [ ] Test with employee token (should fail for admin operations)
- [ ] Verify calculations are correct

---

**Ready to test?** Start with adding a reward, then generate payroll! 🎯
