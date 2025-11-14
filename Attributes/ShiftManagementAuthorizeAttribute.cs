using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using HRMCyberse.Constants;
using HRMCyberse.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMCyberse.Attributes
{
    /// <summary>
    /// Custom authorization attribute for shift management operations
    /// Supports role-based and department-based authorization
    /// </summary>
    public class ShiftManagementAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string[] _allowedRoles;
        private readonly bool _requireSameDepartment;
        private readonly string _operation;

        public ShiftManagementAuthorizeAttribute(string operation, bool requireSameDepartment = false, params string[] allowedRoles)
        {
            _operation = operation;
            _requireSameDepartment = requireSameDepartment;
            _allowedRoles = allowedRoles.Length > 0 ? allowedRoles : new[] { Roles.Admin, Roles.Manager };
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Kiểm tra user đã đăng nhập chưa
            if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedObjectResult(new { message = "Yêu cầu đăng nhập để thực hiện thao tác này" });
                return;
            }

            // Lấy thông tin user từ claims
            var userIdClaim = context.HttpContext.User.FindFirst("UserId")?.Value;
            var userRole = context.HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userRole) || !int.TryParse(userIdClaim, out int userId))
            {
                context.Result = new ForbidResult("Thông tin xác thực không hợp lệ");
                return;
            }

            // Kiểm tra role cơ bản
            if (!_allowedRoles.Contains(userRole))
            {
                context.Result = new ForbidResult($"Không có quyền thực hiện thao tác '{_operation}'. Cần quyền: {string.Join(" hoặc ", _allowedRoles)}");
                return;
            }

            // Nếu là Admin, cho phép tất cả
            if (userRole == Roles.Admin)
            {
                return;
            }

            // Nếu yêu cầu kiểm tra department và user là Manager
            if (_requireSameDepartment && userRole == Roles.Manager)
            {
                var dbContext = context.HttpContext.RequestServices.GetRequiredService<CybersehrmContext>();
                
                // Lấy thông tin department của manager
                var manager = await dbContext.Users
                    .Where(u => u.Id == userId)
                    .Select(u => new { u.Departmentid })
                    .FirstOrDefaultAsync();

                if (manager?.Departmentid == null)
                {
                    context.Result = new ForbidResult("Manager chưa được phân công department");
                    return;
                }

                // Lưu department ID vào HttpContext để sử dụng trong controller
                context.HttpContext.Items["ManagerDepartmentId"] = manager.Departmentid;
            }
        }
    }

    /// <summary>
    /// Attribute for shift creation and modification operations
    /// </summary>
    public class ShiftCreateAuthorizeAttribute : ShiftManagementAuthorizeAttribute
    {
        public ShiftCreateAuthorizeAttribute() : base("Tạo ca làm việc", false, Roles.Admin, Roles.Manager) { }
    }

    /// <summary>
    /// Attribute for shift update operations (Admin only)
    /// </summary>
    public class ShiftUpdateAuthorizeAttribute : ShiftManagementAuthorizeAttribute
    {
        public ShiftUpdateAuthorizeAttribute() : base("Cập nhật ca làm việc", false, Roles.Admin) { }
    }

    /// <summary>
    /// Attribute for shift deletion operations (Admin only)
    /// </summary>
    public class ShiftDeleteAuthorizeAttribute : ShiftManagementAuthorizeAttribute
    {
        public ShiftDeleteAuthorizeAttribute() : base("Xóa ca làm việc", false, Roles.Admin) { }
    }

    /// <summary>
    /// Attribute for shift assignment operations with department checking for managers
    /// </summary>
    public class ShiftAssignAuthorizeAttribute : ShiftManagementAuthorizeAttribute
    {
        public ShiftAssignAuthorizeAttribute() : base("Phân công ca làm việc", true, Roles.Admin, Roles.Manager) { }
    }

    /// <summary>
    /// Attribute for viewing shift assignments with department restrictions for managers
    /// </summary>
    public class ShiftViewAssignmentsAuthorizeAttribute : ShiftManagementAuthorizeAttribute
    {
        public ShiftViewAssignmentsAuthorizeAttribute() : base("Xem phân công ca làm việc", true, Roles.Admin, Roles.Manager) { }
    }

    /// <summary>
    /// Attribute for personal schedule viewing (all authenticated users)
    /// </summary>
    public class ViewPersonalScheduleAuthorizeAttribute : ShiftManagementAuthorizeAttribute
    {
        public ViewPersonalScheduleAuthorizeAttribute() : base("Xem lịch cá nhân", false, Roles.Admin, Roles.Manager, Roles.Employee) { }
    }
}