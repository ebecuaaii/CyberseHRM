# Kế hoạch Triển khai - Hệ thống Quản lý Ca làm việc

- [x] 1. Tạo DTOs và Models hỗ trợ





  - Tạo các DTO classes cho Shift management
  - Tạo validation attributes cho input data
  - Tạo response models cho API endpoints
  - _Requirements: 1.2, 2.1, 3.1_

- [x] 1.1 Tạo ShiftDto và CreateShiftDto


  - Viết ShiftDto class với các properties cần thiết
  - Implement CreateShiftDto với validation attributes
  - Tạo UpdateShiftDto cho chức năng cập nhật
  - _Requirements: 1.2, 2.1, 7.1_

- [x] 1.2 Tạo UserShiftDto và AssignShiftDto


  - Viết UserShiftDto class để hiển thị thông tin gán ca
  - Implement AssignShiftDto với validation cho việc gán ca
  - Tạo các response DTOs cho API responses
  - _Requirements: 3.1, 4.1, 5.1_

- [x] 2. Tạo Shift Service layer





  - Tạo IShiftService interface với các methods cần thiết
  - Implement ShiftService class với business logic
  - Thêm validation logic cho shift times và conflicts
  - _Requirements: 2.3, 6.1, 7.2_

- [x] 2.1 Implement IShiftService interface


  - Định nghĩa interface với tất cả methods cần thiết
  - Thêm async/await support cho database operations
  - Include error handling và validation methods
  - _Requirements: 1.1, 2.1, 3.1_

- [x] 2.2 Implement ShiftService business logic


  - Viết logic cho CRUD operations của shifts
  - Implement validation cho shift times (start < end)
  - Thêm logic kiểm tra duplicate shift names
  - _Requirements: 2.3, 2.6, 7.2_

- [x] 2.3 Implement UserShift assignment logic


  - Viết logic gán ca cho nhân viên
  - Implement validation cho user và shift existence
  - Thêm logic remove assignments
  - _Requirements: 3.2, 3.3, 6.1_

- [x] 3. Tạo ShiftsController với authorization





  - Tạo controller class với proper routing
  - Implement role-based authorization attributes
  - Thêm error handling và logging
  - _Requirements: 1.4, 2.5, 3.5_

- [x] 3.1 Implement shift CRUD endpoints


  - Tạo GET /api/shifts endpoint (Admin/Manager only)
  - Tạo POST /api/shifts endpoint (Admin/Manager only)
  - Tạo PUT /api/shifts/{id} endpoint (Admin only)
  - Tạo DELETE /api/shifts/{id} endpoint (Admin only)
  - _Requirements: 1.1, 2.1, 7.1_

- [x] 3.2 Implement shift assignment endpoints


  - Tạo GET /api/shifts/assignments endpoint (Admin/Manager)
  - Tạo POST /api/shifts/assign endpoint (Admin/Manager)
  - Tạo DELETE /api/shifts/assignments/{id} endpoint (Admin/Manager)
  - _Requirements: 3.1, 5.1, 6.1_

- [x] 3.3 Implement employee schedule endpoints


  - Tạo GET /api/shifts/my-schedule endpoint (All users)
  - Implement logic để chỉ trả về shifts của user hiện tại
  - Thêm filtering và sorting cho schedule data
  - _Requirements: 4.1, 4.2, 4.4_

- [x] 4. Thêm authorization attributes và middleware





  - Tạo custom authorization attributes cho shift management
  - Implement role checking logic
  - Thêm audit logging cho shift operations
  - _Requirements: 8.1, 8.2, 8.5_

- [x] 4.1 Tạo custom authorization attributes


  - Extend existing RequireRole attribute cho shift operations
  - Tạo ShiftManagementAuthorize attribute
  - Implement department-based authorization cho managers
  - _Requirements: 5.2, 8.2_


- [x] 4.2 Implement audit logging






  - Thêm logging cho tất cả shift creation/modification
  - Log shift assignment và removal operations
  - Include user context trong audit logs
  - _Requirements: 6.6, 8.5_

- [x] 5. Tạo dữ liệu mẫu và setup endpoints





  - Tạo endpoint để setup shifts mặc định
  - Implement data seeding cho 6 ca làm việc có sẵn
  - Thêm validation và error handling
  - _Requirements: 1.3_

- [x] 5.1 Implement shift seeding endpoint


  - Tạo POST /api/shifts/seed-default-shifts endpoint
  - Implement logic tạo 6 ca mặc định (Ca1, Ca2, Ca3, Ca4, Ca5, Ca đêm)
  - Thêm check để không duplicate shifts
  - _Requirements: 1.3_

- [x] 5.2 Tạo shift validation utilities


  - Implement helper methods cho time validation
  - Thêm business rule validation (overnight shifts)
  - Create utility methods cho shift conflict detection
  - _Requirements: 2.3, 2.4_

- [x] 6. Testing và integration





  - Viết unit tests cho service layer
  - Tạo integration tests cho controller endpoints
  - Test authorization và error scenarios
  - _Requirements: 8.3, 8.4_

- [ ]* 6.1 Viết unit tests cho ShiftService
  - Test tất cả CRUD operations
  - Test validation logic và error cases
  - Test assignment và removal logic
  - _Requirements: 2.1, 3.1, 6.1_

- [ ]* 6.2 Viết integration tests cho ShiftsController
  - Test tất cả API endpoints với different roles
  - Test authorization scenarios (403, 401 responses)
  - Test end-to-end workflows
  - _Requirements: 1.4, 3.5, 8.3_

- [x] 7. Documentation và cleanup







  - Cập nhật API documentation
  - Thêm code comments và XML documentation
  - Review và optimize performance
  - _Requirements: All_

- [x] 7.1 Cập nhật API documentation


  - Document tất cả endpoints với examples
  - Thêm authentication requirements
  - Include error response examples
  - _Requirements: 8.4_

- [x] 7.2 Code review và optimization



  - Review code quality và best practices
  - Optimize database queries với proper includes
  - Add caching cho frequently accessed data
  - _Requirements: Performance considerations_