using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HRMCyberse.Attributes;
using HRMCyberse.Constants;

namespace HRMCyberse.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Tất cả endpoint đều cần đăng nhập
    public class PermissionTestController : ControllerBase
    {
        [HttpGet("my-info")]
        public ActionResult GetMyInfo()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var fullName = User.FindFirst("FullName")?.Value;

            return Ok(new { 
                message = "Thông tin của tôi - Tất cả user đều truy cập được",
                userId = userId,
                username = username,
                role = role,
                fullName = fullName
            });
        }

        [HttpGet("admin-dashboard")]
        [AdminOnly]
        public ActionResult GetAdminDashboard()
        {
            return Ok(new { 
                message = "Dashboard Admin - Chỉ Admin truy cập được",
                features = new[] { "Quản lý user", "Xem báo cáo tổng", "Cấu hình hệ thống" }
            });
        }

        [HttpGet("management-reports")]
        [AdminOrManager]
        public ActionResult GetManagementReports()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            
            return Ok(new { 
                message = "Báo cáo quản lý - Admin và Manager truy cập được",
                userRole = role,
                reports = role == Roles.Admin 
                    ? new[] { "Báo cáo tổng công ty", "Báo cáo tài chính", "Báo cáo nhân sự" }
                    : new[] { "Báo cáo phòng ban", "Báo cáo nhân viên" }
            });
        }

        [HttpGet("employee-tasks")]
        [RequireRole(Roles.Admin, Roles.Manager, Roles.Employee)]
        public ActionResult GetEmployeeTasks()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            
            var tasks = role switch
            {
                Roles.Admin => new[] { "Quản lý toàn bộ hệ thống", "Xem tất cả công việc", "Phê duyệt mọi yêu cầu" },
                Roles.Manager => new[] { "Phân công công việc", "Đánh giá nhân viên", "Duyệt đơn nghỉ phép" },
                Roles.Employee => new[] { "Xem công việc được giao", "Chấm công", "Đăng ký nghỉ phép" },
                _ => new[] { "Không có quyền" }
            };
            
            return Ok(new { 
                message = "Công việc nhân viên - Admin, Manager và Employee đều truy cập được",
                userRole = role,
                tasks = tasks
            });
        }

        [HttpGet("all-users")]
        [RequireRole(Roles.Admin, Roles.Manager, Roles.Employee)]
        public ActionResult GetAllUsers()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            
            return Ok(new { 
                message = "Danh sách user - Tất cả role đều truy cập được",
                userRole = role,
                note = "Nhưng dữ liệu sẽ khác nhau tùy theo quyền"
            });
        }

        [HttpPost("create-user")]
        [AdminOnly]
        public ActionResult CreateUser()
        {
            return Ok(new { 
                message = "Tạo user mới - Chỉ Admin được phép",
                success = true
            });
        }

        [HttpPut("update-employee")]
        [AdminOrManager]
        public ActionResult UpdateEmployee()
        {
            return Ok(new { 
                message = "Cập nhật thông tin nhân viên - Admin và Manager được phép",
                success = true
            });
        }

        [HttpGet("check-permissions")]
        public ActionResult CheckPermissions()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            
            var permissions = new Dictionary<string, bool>
            {
                ["canAccessAdminDashboard"] = role == Roles.Admin,
                ["canViewManagementReports"] = role == Roles.Admin || role == Roles.Manager,
                ["canManageEmployees"] = role == Roles.Admin || role == Roles.Manager,
                ["canCreateUsers"] = role == Roles.Admin,
                ["canViewEmployeeTasks"] = role == Roles.Admin || role == Roles.Manager || role == Roles.Employee,
                ["canApproveLeave"] = role == Roles.Admin || role == Roles.Manager,
                ["canViewAllData"] = role == Roles.Admin,
                ["canManageDepartment"] = role == Roles.Admin || role == Roles.Manager
            };

            return Ok(new { 
                message = "Kiểm tra quyền hạn của user hiện tại",
                userRole = role,
                permissions = permissions
            });
        }
    }
}