using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRMCyberse.Data;
using HRMCyberse.Models;

namespace HRMCyberse.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly CybersehrmContext _context;
        private readonly ILogger<RolesController> _logger;

        public RolesController(CybersehrmContext context, ILogger<RolesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Role>>> GetRoles()
        {
            try
            {
                var roles = await _context.Roles.ToListAsync();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách roles");
                return StatusCode(500, "Lỗi server khi lấy dữ liệu");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Role>> GetRole(int id)
        {
            try
            {
                var role = await _context.Roles.FindAsync(id);

                if (role == null)
                {
                    return NotFound("Không tìm thấy role");
                }

                return Ok(role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin role {Id}", id);
                return StatusCode(500, "Lỗi server khi lấy dữ liệu");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Role>> CreateRole(Role role)
        {
            try
            {
                // Kiểm tra tên role đã tồn tại
                var existingRole = await _context.Roles
                    .FirstOrDefaultAsync(r => r.Rolename == role.Rolename);
                
                if (existingRole != null)
                {
                    return BadRequest("Tên role đã tồn tại");
                }

                _context.Roles.Add(role);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetRole), new { id = role.Id }, role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo role mới");
                return StatusCode(500, "Lỗi server khi tạo role");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(int id, Role role)
        {
            if (id != role.Id)
            {
                return BadRequest("ID không khớp");
            }

            try
            {
                _context.Entry(role).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await RoleExists(id))
                {
                    return NotFound("Không tìm thấy role");
                }
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật role {Id}", id);
                return StatusCode(500, "Lỗi server khi cập nhật");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            try
            {
                var role = await _context.Roles.FindAsync(id);
                if (role == null)
                {
                    return NotFound("Không tìm thấy role");
                }

                // Kiểm tra có user nào đang sử dụng role này không
                var hasUsers = await _context.Users.AnyAsync(u => u.Roleid == id);
                if (hasUsers)
                {
                    return BadRequest("Không thể xóa role đang được sử dụng");
                }

                _context.Roles.Remove(role);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa role {Id}", id);
                return StatusCode(500, "Lỗi server khi xóa");
            }
        }

        private async Task<bool> RoleExists(int id)
        {
            return await _context.Roles.AnyAsync(e => e.Id == id);
        }

        [HttpGet("check-system")]
        public async Task<ActionResult> CheckSystem()
        {
            try
            {
                var roles = await _context.Roles.ToListAsync();
                var users = await _context.Users
                    .Include(u => u.Role)
                    .Select(u => new { u.Id, u.Username, u.Fullname, RoleName = u.Role != null ? u.Role.Rolename : "No Role", u.Roleid })
                    .ToListAsync();

                return Ok(new {
                    message = "Kiểm tra roles và users trong hệ thống",
                    roles = roles,
                    users = users
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi kiểm tra system");
                return StatusCode(500, "Lỗi server");
            }
        }

        [HttpPost("setup-system")]
        public async Task<ActionResult> SetupSystem()
        {
            try
            {
                var updates = new List<object>();

                // 1. Tạo các role cần thiết nếu chưa có
                var existingRoles = await _context.Roles.Select(r => r.Rolename).ToListAsync();
                var rolesToCreate = new List<Role>();

                if (!existingRoles.Contains("admin"))
                {
                    rolesToCreate.Add(new Role { Rolename = "admin", Description = "Quản trị viên hệ thống" });
                }

                if (!existingRoles.Contains("manager"))
                {
                    rolesToCreate.Add(new Role { Rolename = "manager", Description = "Quản lý phòng ban" });
                }

                if (!existingRoles.Contains("employee"))
                {
                    rolesToCreate.Add(new Role { Rolename = "employee", Description = "Nhân viên" });
                }

                if (rolesToCreate.Any())
                {
                    _context.Roles.AddRange(rolesToCreate);
                    await _context.SaveChangesAsync();
                    updates.Add(new { action = "created_roles", roles = rolesToCreate.Select(r => r.Rolename) });
                }

                // 2. Lấy roles sau khi tạo
                var roles = await _context.Roles.ToListAsync();
                var adminRole = roles.FirstOrDefault(r => r.Rolename == "admin");
                var managerRole = roles.FirstOrDefault(r => r.Rolename == "manager");
                var employeeRole = roles.FirstOrDefault(r => r.Rolename == "employee");

                // 3. Gán role admin cho user admin
                var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
                if (adminUser != null && adminRole != null)
                {
                    adminUser.Roleid = adminRole.Id;
                    updates.Add(new { action = "assigned_role", username = "admin", role = "admin" });
                }

                // 4. Gán role manager cho user manager
                var managerUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == "manager");
                if (managerUser != null && managerRole != null)
                {
                    managerUser.Roleid = managerRole.Id;
                    updates.Add(new { action = "assigned_role", username = "manager", role = "manager" });
                }

                // 5. Gán role employee cho tất cả user còn lại
                var otherUsers = await _context.Users
                    .Where(u => u.Username != "admin" && u.Username != "manager")
                    .ToListAsync();

                foreach (var user in otherUsers)
                {
                    if (employeeRole != null)
                    {
                        user.Roleid = employeeRole.Id;
                        updates.Add(new { action = "assigned_role", username = user.Username, role = "employee" });
                    }
                }

                await _context.SaveChangesAsync();

                // 6. Lấy kết quả cuối cùng
                var finalUsers = await _context.Users
                    .Include(u => u.Role)
                    .Select(u => new { 
                        u.Username, 
                        u.Fullname, 
                        RoleName = u.Role != null ? u.Role.Rolename : "No Role"
                    })
                    .ToListAsync();

                return Ok(new {
                    message = "Setup hệ thống phân quyền thành công",
                    updates = updates,
                    finalResult = finalUsers
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi setup system");
                return StatusCode(500, "Lỗi server");
            }
        }

        [HttpPost("assign-role-to-user")]
        public async Task<ActionResult> AssignRoleToUser(string username, string roleName)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (user == null)
                {
                    return NotFound($"Không tìm thấy user: {username}");
                }

                var role = await _context.Roles.FirstOrDefaultAsync(r => r.Rolename == roleName);
                if (role == null)
                {
                    return NotFound($"Không tìm thấy role: {roleName}");
                }

                user.Roleid = role.Id;
                await _context.SaveChangesAsync();

                return Ok(new {
                    message = "Gán role thành công",
                    username = username,
                    roleName = roleName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gán role");
                return StatusCode(500, "Lỗi server");
            }
        }

        [HttpPost("fix-admin-role")]
        public async Task<ActionResult> FixAdminRole()
        {
            try
            {
                // Đảm bảo user admin có role admin
                var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
                var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Rolename == "admin");

                if (adminUser == null)
                {
                    return NotFound("Không tìm thấy user admin");
                }

                if (adminRole == null)
                {
                    return NotFound("Không tìm thấy role admin");
                }

                adminUser.Roleid = adminRole.Id;
                await _context.SaveChangesAsync();

                // Kiểm tra lại
                var updatedUser = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Username == "admin");

                return Ok(new {
                    message = "Fix admin role thành công",
                    user = new {
                        username = updatedUser?.Username,
                        roleName = updatedUser?.Role?.Rolename,
                        roleId = updatedUser?.Roleid
                    },
                    note = "Bây giờ đăng nhập lại để lấy JWT token mới"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi fix admin role");
                return StatusCode(500, "Lỗi server");
            }
        }
    }
}