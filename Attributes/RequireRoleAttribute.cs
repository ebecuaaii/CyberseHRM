using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using HRMCyberse.Constants;

namespace HRMCyberse.Attributes
{
    public class RequireRoleAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _roles;

        public RequireRoleAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Kiểm tra user đã đăng nhập chưa
            if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Lấy role của user
            var userRole = context.HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userRole) || !_roles.Contains(userRole))
            {
                context.Result = new ForbidResult($"Cần quyền: {string.Join(" hoặc ", _roles)}");
                return;
            }
        }
    }

    // Các attribute tiện lợi
    public class AdminOnlyAttribute : RequireRoleAttribute
    {
        public AdminOnlyAttribute() : base(Roles.Admin) { }
    }

    public class AdminOrManagerAttribute : RequireRoleAttribute
    {
        public AdminOrManagerAttribute() : base(Roles.Admin, Roles.Manager) { }
    }

    public class ManagerOrEmployeeAttribute : RequireRoleAttribute
    {
        public ManagerOrEmployeeAttribute() : base(Roles.Manager, Roles.Employee) { }
    }
}