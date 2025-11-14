using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HRMCyberse.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet("public")]
        public ActionResult GetPublic()
        {
            return Ok(new { message = "Đây là endpoint public, không cần token" });
        }

        [HttpGet("protected")]
        [Authorize]
        public ActionResult GetProtected()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var fullName = User.FindFirst("FullName")?.Value;

            return Ok(new { 
                message = "Đây là endpoint được bảo vệ bởi JWT",
                userId = userId,
                username = username,
                role = role,
                fullName = fullName,
                allClaims = User.Claims.Select(c => new { c.Type, c.Value })
            });
        }

        [HttpGet("admin-only")]
        [Authorize]
        public ActionResult GetAdminOnly()
        {
            // Kiểm tra role manually
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            
            if (role != "admin")
            {
                return Forbid("Chỉ admin mới được truy cập endpoint này");
            }
            
            return Ok(new { message = "Chỉ admin mới truy cập được endpoint này" });
        }

        [HttpGet("check-role")]
        [Authorize]
        public ActionResult CheckRole()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var roleId = User.FindFirst("RoleId")?.Value;

            return Ok(new { 
                message = "Thông tin role của user hiện tại",
                userId = userId,
                username = username,
                roleName = role,
                roleId = roleId,
                hasAdminRole = User.IsInRole("Admin"),
                allRoles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value)
            });
        }
    }
}