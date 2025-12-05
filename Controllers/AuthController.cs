using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using HRMCyberse.Data;
using HRMCyberse.Models;
using HRMCyberse.DTOs;
using HRMCyberse.Services;

namespace HRMCyberse.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly CybersehrmContext _context;
        private readonly IPasswordService _passwordService;
        private readonly IJwtService _jwtService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(CybersehrmContext context, IPasswordService passwordService, IJwtService jwtService, ILogger<AuthController> logger)
        {
            _context = context;
            _passwordService = passwordService;
            _jwtService = jwtService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserResponseDto>> Register(RegisterDto registerDto)
        {
            try
            {
                // Kiểm tra username đã tồn tại
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == registerDto.Username);
                
                if (existingUser != null)
                {
                    return BadRequest("Tên đăng nhập đã tồn tại");
                }

                // Kiểm tra email đã tồn tại
                if (!string.IsNullOrEmpty(registerDto.Email))
                {
                    var existingEmail = await _context.Users
                        .FirstOrDefaultAsync(u => u.Email == registerDto.Email);
                    
                    if (existingEmail != null)
                    {
                        return BadRequest("Email đã được sử dụng");
                    }
                }

                // Tìm invitation nếu có branchCode
                EmployeeInvitation? invitation = null;
                Branch? branch = null;
                
                if (!string.IsNullOrEmpty(registerDto.BranchCode))
                {
                    // Tìm branch từ code
                    branch = await _context.Branches
                        .FirstOrDefaultAsync(b => b.BranchCode == registerDto.BranchCode);
                    
                    if (branch == null)
                    {
                        return BadRequest($"Mã chi nhánh '{registerDto.BranchCode}' không tồn tại");
                    }

                    // Tìm invitation chưa sử dụng và chưa hết hạn cho email này
                    invitation = await _context.EmployeeInvitations
                        .Include(i => i.Role)
                        .Include(i => i.Department)
                        .Include(i => i.Position)
                        .FirstOrDefaultAsync(i => 
                            i.Email == registerDto.Email && 
                            i.BranchId == branch.Id &&
                            i.IsUsed == false && 
                            i.ExpiresAt > DateTime.UtcNow);

                    if (invitation == null)
                    {
                        return BadRequest("Không tìm thấy lời mời hợp lệ cho email và chi nhánh này");
                    }
                }

                // Kiểm tra role có tồn tại không (nếu được cung cấp)
                if (!string.IsNullOrEmpty(registerDto.RoleName))
                {
                    var roleExists = await _context.Roles.AnyAsync(r => r.Rolename == registerDto.RoleName);
                    if (!roleExists)
                    {
                        return BadRequest($"Role '{registerDto.RoleName}' không tồn tại");
                    }
                }

                // Kiểm tra department có tồn tại không (nếu được cung cấp)
                if (!string.IsNullOrEmpty(registerDto.DepartmentName))
                {
                    var departmentExists = await _context.Departments.AnyAsync(d => d.Name == registerDto.DepartmentName);
                    if (!departmentExists)
                    {
                        return BadRequest($"Department '{registerDto.DepartmentName}' không tồn tại");
                    }
                }

                // Kiểm tra position có tồn tại không (nếu được cung cấp)
                if (!string.IsNullOrEmpty(registerDto.PositionName))
                {
                    var positionExists = await _context.Positiontitles.AnyAsync(p => p.Titlename == registerDto.PositionName);
                    if (!positionExists)
                    {
                        return BadRequest($"Position '{registerDto.PositionName}' không tồn tại");
                    }
                }

                // Tạo user mới
                var user = new User
                {
                    Username = registerDto.Username,
                    Passwordhash = _passwordService.HashPassword(registerDto.Password),
                    Fullname = registerDto.Fullname,
                    Email = registerDto.Email,
                    Phone = registerDto.Phone,
                    Isactive = true,
                    Createdat = DateTime.UtcNow,
                    Hiredate = DateOnly.FromDateTime(DateTime.Now)
                };

                // Nếu có invitation, ưu tiên dùng thông tin từ invitation
                if (invitation != null)
                {
                    user.BranchId = invitation.BranchId;
                    user.Roleid = invitation.Roleid;
                    user.Departmentid = invitation.Departmentid;
                    user.Positionid = invitation.Positionid;
                    user.Salaryrate = invitation.Salaryrate;

                    // Đánh dấu invitation đã sử dụng
                    invitation.IsUsed = true;
                    invitation.UsedAt = DateTime.UtcNow;
                }
                else
                {
                    // Không có invitation, dùng thông tin từ registerDto
                    if (!string.IsNullOrEmpty(registerDto.RoleName))
                    {
                        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Rolename == registerDto.RoleName);
                        user.Roleid = role?.Id;
                    }
                    else
                    {
                        user.Roleid = 3; // Mặc định role user thường
                    }

                    if (!string.IsNullOrEmpty(registerDto.DepartmentName))
                    {
                        var department = await _context.Departments.FirstOrDefaultAsync(d => d.Name == registerDto.DepartmentName);
                        user.Departmentid = department?.Id;
                    }

                    if (!string.IsNullOrEmpty(registerDto.PositionName))
                    {
                        var position = await _context.Positiontitles.FirstOrDefaultAsync(p => p.Titlename == registerDto.PositionName);
                        user.Positionid = position?.Id;
                    }
                }

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Load related data để lấy names
                await _context.Entry(user)
                    .Reference(u => u.Role)
                    .LoadAsync();
                await _context.Entry(user)
                    .Reference(u => u.Department)
                    .LoadAsync();
                await _context.Entry(user)
                    .Reference(u => u.Position)
                    .LoadAsync();

                var response = new UserResponseDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Fullname = user.Fullname,
                    Email = user.Email,
                    Phone = user.Phone,
                    RoleName = user.Role?.Rolename,
                    DepartmentName = user.Department?.Name,
                    PositionName = user.Position?.Titlename,
                    IsActive = user.Isactive,
                    CreatedAt = user.Createdat
                };

                _logger.LogInformation("Đăng ký thành công cho user: {Username}", user.Username);
                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đăng ký user");
                return StatusCode(500, "Lỗi server khi đăng ký");
            }
        }

        [HttpGet("users")]
        public async Task<ActionResult<List<UserResponseDto>>> GetAllUsers()
        {
            try
            {
                var users = await _context.Users
                    .Include(u => u.Role)
                    .Include(u => u.Department)
                    .Include(u => u.Position)
                    .Where(u => u.Isactive == true)
                    .OrderBy(u => u.Fullname)
                    .AsSplitQuery() // Tối ưu cho multiple includes
                    .ToListAsync();

                var response = users.Select(user => new UserResponseDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Fullname = user.Fullname,
                    Email = user.Email,
                    Phone = user.Phone,
                    RoleName = user.Role?.Rolename ?? null,
                    DepartmentName = user.Department?.Name ?? null,
                    PositionName = user.Position?.Titlename ?? null,
                    IsActive = user.Isactive,
                    CreatedAt = user.Createdat
                }).ToList();

                _logger.LogInformation("Lấy danh sách {Count} nhân viên. Số user có role: {RoleCount}, có position: {PositionCount}", 
                    response.Count, 
                    response.Count(r => !string.IsNullOrEmpty(r.RoleName)),
                    response.Count(r => !string.IsNullOrEmpty(r.PositionName)));
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách nhân viên");
                return StatusCode(500, "Lỗi server khi lấy dữ liệu");
            }
        }

        [HttpGet("users/debug")]
        public async Task<ActionResult> GetAllUsersDebug()
        {
            try
            {
                var users = await _context.Users
                    .Include(u => u.Role)
                    .Include(u => u.Department)
                    .Include(u => u.Position)
                    .Where(u => u.Isactive == true)
                    .OrderBy(u => u.Fullname)
                    .ToListAsync();

                var debugInfo = users.Select(user => new
                {
                    user.Id,
                    user.Username,
                    user.Fullname,
                    user.Roleid,
                    RoleName = user.Role?.Rolename,
                    RoleIsNull = user.Role == null,
                    user.Departmentid,
                    DepartmentName = user.Department?.Name,
                    DepartmentIsNull = user.Department == null,
                    user.Positionid,
                    PositionName = user.Position?.Titlename,
                    PositionIsNull = user.Position == null
                }).ToList();

                return Ok(debugInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi debug users");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("users/fix-data")]
        public async Task<ActionResult> FixUserData()
        {
            try
            {
                // Lấy role Employee mặc định (id = 3)
                var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.Rolename == "Employee");
                if (defaultRole == null)
                {
                    defaultRole = await _context.Roles.FirstOrDefaultAsync();
                }

                // Lấy position mặc định
                var defaultPosition = await _context.Positiontitles.FirstOrDefaultAsync(p => p.Titlename == "Employee");
                if (defaultPosition == null)
                {
                    defaultPosition = await _context.Positiontitles.FirstOrDefaultAsync();
                }

                // Lấy department mặc định
                var defaultDepartment = await _context.Departments.FirstOrDefaultAsync();

                // Lấy tất cả users active
                var users = await _context.Users
                    .Where(u => u.Isactive == true)
                    .ToListAsync();

                int fixedCount = 0;
                foreach (var user in users)
                {
                    bool needUpdate = false;

                    if (user.Roleid == null && defaultRole != null)
                    {
                        user.Roleid = defaultRole.Id;
                        needUpdate = true;
                    }

                    if (user.Positionid == null && defaultPosition != null)
                    {
                        user.Positionid = defaultPosition.Id;
                        needUpdate = true;
                    }

                    if (user.Departmentid == null && defaultDepartment != null)
                    {
                        user.Departmentid = defaultDepartment.Id;
                        needUpdate = true;
                    }

                    if (needUpdate)
                    {
                        fixedCount++;
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = $"Đã fix dữ liệu cho {fixedCount} users",
                    fixedCount,
                    totalUsers = users.Count,
                    defaultRole = defaultRole?.Rolename,
                    defaultPosition = defaultPosition?.Titlename,
                    defaultDepartment = defaultDepartment?.Name
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi fix user data");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("user/{id}")]
        public async Task<ActionResult<UserResponseDto>> GetUser(int id)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Role)
                    .Include(u => u.Department)
                    .Include(u => u.Position)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    return NotFound("Không tìm thấy người dùng");
                }

                var response = new UserResponseDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Fullname = user.Fullname,
                    Email = user.Email,
                    Phone = user.Phone,
                    RoleName = user.Role?.Rolename,
                    DepartmentName = user.Department?.Name,
                    PositionName = user.Position?.Titlename,
                    IsActive = user.Isactive,
                    CreatedAt = user.Createdat
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin user {Id}", id);
                return StatusCode(500, "Lỗi server khi lấy dữ liệu");
            }
        }

        /// <summary>
        /// Admin cập nhật thông tin user (role, position, department, và các thông tin khác)
        /// </summary>
        [HttpPut("user/{id}")]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<ActionResult<UserResponseDto>> UpdateUser(int id, UpdateUserDto updateDto)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    return NotFound("Không tìm thấy người dùng");
                }

                // Cập nhật Role nếu có
                if (!string.IsNullOrEmpty(updateDto.RoleName))
                {
                    var role = await _context.Roles.FirstOrDefaultAsync(r => r.Rolename == updateDto.RoleName);
                    if (role == null)
                    {
                        return BadRequest($"Không tìm thấy role: {updateDto.RoleName}");
                    }
                    user.Roleid = role.Id;
                }

                // Cập nhật Position nếu có
                if (!string.IsNullOrEmpty(updateDto.PositionName))
                {
                    var position = await _context.Positiontitles.FirstOrDefaultAsync(p => p.Titlename == updateDto.PositionName);
                    if (position == null)
                    {
                        return BadRequest($"Không tìm thấy position: {updateDto.PositionName}");
                    }
                    user.Positionid = position.Id;
                }

                // Cập nhật Department nếu có
                if (!string.IsNullOrEmpty(updateDto.DepartmentName))
                {
                    var department = await _context.Departments.FirstOrDefaultAsync(d => d.Name == updateDto.DepartmentName);
                    if (department == null)
                    {
                        return BadRequest($"Không tìm thấy department: {updateDto.DepartmentName}");
                    }
                    user.Departmentid = department.Id;
                }

                // Cập nhật các thông tin khác nếu có
                if (!string.IsNullOrEmpty(updateDto.Fullname))
                {
                    user.Fullname = updateDto.Fullname;
                }

                if (!string.IsNullOrEmpty(updateDto.Email))
                {
                    // Kiểm tra email đã được sử dụng bởi user khác chưa
                    var existingEmail = await _context.Users
                        .FirstOrDefaultAsync(u => u.Email == updateDto.Email && u.Id != id);
                    if (existingEmail != null)
                    {
                        return BadRequest("Email đã được sử dụng bởi user khác");
                    }
                    user.Email = updateDto.Email;
                }

                if (updateDto.Phone != null)
                {
                    user.Phone = updateDto.Phone;
                }

                if (updateDto.IsActive.HasValue)
                {
                    user.Isactive = updateDto.IsActive.Value;
                }

                await _context.SaveChangesAsync();

                // Load lại related data để trả về đầy đủ
                await _context.Entry(user)
                    .Reference(u => u.Role)
                    .LoadAsync();
                await _context.Entry(user)
                    .Reference(u => u.Department)
                    .LoadAsync();
                await _context.Entry(user)
                    .Reference(u => u.Position)
                    .LoadAsync();

                var response = new UserResponseDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Fullname = user.Fullname,
                    Email = user.Email,
                    Phone = user.Phone,
                    RoleName = user.Role?.Rolename,
                    DepartmentName = user.Department?.Name,
                    PositionName = user.Position?.Titlename,
                    IsActive = user.Isactive,
                    CreatedAt = user.Createdat
                };

                _logger.LogInformation("Admin đã cập nhật thông tin user {Id} ({Username})", user.Id, user.Username);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật thông tin user {Id}", id);
                return StatusCode(500, "Lỗi server khi cập nhật dữ liệu");
            }
        }

        [HttpPost("check-username")]
        public async Task<ActionResult<bool>> CheckUsername(string username)
        {
            try
            {
                var exists = await _context.Users
                    .AnyAsync(u => u.Username == username);
                
                return Ok(new { exists, message = exists ? "Tên đăng nhập đã tồn tại" : "Tên đăng nhập có thể sử dụng" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi kiểm tra username");
                return StatusCode(500, "Lỗi server");
            }
        }

        [HttpPost("check-email")]
        public async Task<ActionResult<bool>> CheckEmail(string email)
        {
            try
            {
                var exists = await _context.Users
                    .AnyAsync(u => u.Email == email);
                
                return Ok(new { exists, message = exists ? "Email đã được sử dụng" : "Email có thể sử dụng" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi kiểm tra email");
                return StatusCode(500, "Lỗi server");
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login(LoginDto loginDto)
        {
            try
            {
                // Tìm user theo username
                var user = await _context.Users
                    .Include(u => u.Role)
                    .Include(u => u.Department)
                    .Include(u => u.Position)
                    .FirstOrDefaultAsync(u => u.Username == loginDto.Username);

                if (user == null)
                {
                    return Ok(new LoginResponseDto
                    {
                        Success = false,
                        Message = "Tên đăng nhập không tồn tại"
                    });
                }

                // Kiểm tra tài khoản có bị khóa không
                if (user.Isactive != true)
                {
                    return Ok(new LoginResponseDto
                    {
                        Success = false,
                        Message = "Tài khoản đã bị khóa"
                    });
                }

                // Kiểm tra mật khẩu
                if (!_passwordService.VerifyPassword(loginDto.Password, user.Passwordhash))
                {
                    return Ok(new LoginResponseDto
                    {
                        Success = false,
                        Message = "Mật khẩu không đúng"
                    });
                }

                // Đăng nhập thành công
                var userResponse = new UserResponseDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Fullname = user.Fullname,
                    Email = user.Email,
                    Phone = user.Phone,
                    RoleName = user.Role?.Rolename,
                    DepartmentName = user.Department?.Name,
                    PositionName = user.Position?.Titlename,
                    IsActive = user.Isactive,
                    CreatedAt = user.Createdat
                };

                // Generate JWT token
                var token = _jwtService.GenerateToken(user);

                _logger.LogInformation("Đăng nhập thành công cho user: {Username}", user.Username);

                return Ok(new LoginResponseDto
                {
                    Success = true,
                    Message = "Đăng nhập thành công",
                    User = userResponse,
                    Token = token
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đăng nhập user: {Username}", loginDto.Username);
                return StatusCode(500, "Lỗi server khi đăng nhập");
            }
        }

        [HttpPost("logout")]
        public ActionResult Logout()
        {
            try
            {
                // Hiện tại chỉ trả về thông báo thành công
                // Sau này có thể thêm logic xóa token, session, etc.
                
                _logger.LogInformation("User đã đăng xuất");
                
                return Ok(new { 
                    success = true, 
                    message = "Đăng xuất thành công" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đăng xuất");
                return StatusCode(500, "Lỗi server khi đăng xuất");
            }
        }

        [HttpPost("refresh")]
        [Authorize]
        public async Task<ActionResult> RefreshToken()
        {
            try
            {
                // Lấy thông tin user từ token hiện tại
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Token không hợp lệ" });
                }

                // Lấy thông tin user từ database
                var user = await _context.Users
                    .Include(u => u.Role)
                    .Include(u => u.Department)
                    .Include(u => u.Position)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null || user.Isactive != true)
                {
                    return Unauthorized(new { message = "User không tồn tại hoặc đã bị khóa" });
                }

                // Generate token mới
                var newToken = _jwtService.GenerateToken(user);

                _logger.LogInformation("Refreshed token for user: {Username}", user.Username);

                return Ok(new
                {
                    success = true,
                    accessToken = newToken,
                    message = "Token đã được làm mới"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi refresh token");
                return StatusCode(500, "Lỗi server khi refresh token");
            }
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult> ResetPassword(string username, string newPassword)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (user == null)
                {
                    return NotFound("Không tìm thấy user");
                }

                // Cập nhật mật khẩu với hash mới
                user.Passwordhash = _passwordService.HashPassword(newPassword);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Đã reset mật khẩu cho user: {Username}", username);
                
                return Ok(new { 
                    success = true, 
                    message = "Reset mật khẩu thành công",
                    oldHash = user.Passwordhash.Substring(0, 10) + "...",
                    newHash = _passwordService.HashPassword(newPassword).Substring(0, 10) + "..."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi reset mật khẩu");
                return StatusCode(500, "Lỗi server");
            }
        }

        [HttpGet("check-password-hash/{username}")]
        public async Task<ActionResult> CheckPasswordHash(string username)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (user == null)
                {
                    return NotFound("Không tìm thấy user");
                }

                var testPassword = "123456";
                var ourHash = _passwordService.HashPassword(testPassword);
                var isMatch = _passwordService.VerifyPassword(testPassword, user.Passwordhash);

                return Ok(new { 
                    username = user.Username,
                    currentHash = user.Passwordhash.Substring(0, 20) + "...",
                    ourHash = ourHash.Substring(0, 20) + "...",
                    isMatch = isMatch,
                    message = isMatch ? "Mật khẩu khớp" : "Mật khẩu không khớp - cần reset"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi kiểm tra hash");
                return StatusCode(500, "Lỗi server");
            }
        }

        /// <summary>
        /// Lấy thông tin invitation từ token (không cần auth)
        /// </summary>
        [HttpGet("invitation/{token}")]
        [AllowAnonymous]
        public async Task<ActionResult<InvitationDetailsDto>> GetInvitationDetails(string token)
        {
            try
            {
                var invitation = await _context.EmployeeInvitations
                    .Include(i => i.Branch)
                    .Include(i => i.Role)
                    .Include(i => i.Department)
                    .Include(i => i.Position)
                    .FirstOrDefaultAsync(i => i.InvitationToken == token);

                if (invitation == null)
                    return NotFound(new { message = "Lời mời không tồn tại" });

                var isExpired = invitation.ExpiresAt <= DateTime.UtcNow;
                var isUsed = invitation.IsUsed ?? false;

                if (isUsed)
                    return BadRequest(new { message = "Lời mời đã được sử dụng" });

                if (isExpired)
                    return BadRequest(new { message = "Lời mời đã hết hạn" });

                var response = new InvitationDetailsDto
                {
                    Email = invitation.Email,
                    BranchCode = invitation.Branch.BranchCode,
                    BranchName = invitation.Branch.BranchName,
                    RoleName = invitation.Role?.Rolename,
                    DepartmentName = invitation.Department?.Name,
                    PositionName = invitation.Position?.Titlename,
                    SalaryRate = invitation.Salaryrate,
                    ExpiresAt = invitation.ExpiresAt,
                    IsExpired = isExpired,
                    IsUsed = isUsed
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin invitation");
                return StatusCode(500, new { message = "Đã xảy ra lỗi" });
            }
        }

        /// <summary>
        /// Chấp nhận lời mời và tạo tài khoản (không cần auth)
        /// </summary>
        [HttpPost("accept-invitation")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponseDto>> AcceptInvitation(AcceptInvitationDto dto)
        {
            try
            {
                // Tìm invitation
                var invitation = await _context.EmployeeInvitations
                    .Include(i => i.Branch)
                    .Include(i => i.Role)
                    .Include(i => i.Department)
                    .Include(i => i.Position)
                    .FirstOrDefaultAsync(i => i.InvitationToken == dto.Token);

                if (invitation == null)
                    return NotFound(new { message = "Lời mời không tồn tại" });

                if (invitation.IsUsed == true)
                    return BadRequest(new { message = "Lời mời đã được sử dụng" });

                if (invitation.ExpiresAt <= DateTime.UtcNow)
                    return BadRequest(new { message = "Lời mời đã hết hạn" });

                // Kiểm tra username đã tồn tại
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == dto.Username);
                if (existingUser != null)
                    return BadRequest(new { message = "Tên đăng nhập đã tồn tại" });

                // Tạo user mới từ invitation
                var user = new User
                {
                    Username = dto.Username,
                    Passwordhash = _passwordService.HashPassword(dto.Password),
                    Fullname = dto.FullName,
                    Email = invitation.Email,
                    Phone = dto.Phone,
                    BranchId = invitation.BranchId,
                    Roleid = invitation.Roleid,
                    Departmentid = invitation.Departmentid,
                    Positionid = invitation.Positionid,
                    Salaryrate = invitation.Salaryrate,
                    Hiredate = DateOnly.FromDateTime(DateTime.Now),
                    Isactive = true,
                    Createdat = DateTime.UtcNow
                };

                _context.Users.Add(user);

                // Đánh dấu invitation đã sử dụng
                invitation.IsUsed = true;
                invitation.UsedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Load related data
                await _context.Entry(user)
                    .Reference(u => u.Branch)
                    .LoadAsync();
                await _context.Entry(user)
                    .Reference(u => u.Role)
                    .LoadAsync();
                await _context.Entry(user)
                    .Reference(u => u.Department)
                    .LoadAsync();
                await _context.Entry(user)
                    .Reference(u => u.Position)
                    .LoadAsync();

                // Generate JWT token
                var token = _jwtService.GenerateToken(user);

                var userResponse = new UserResponseDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Fullname = user.Fullname,
                    Email = user.Email,
                    Phone = user.Phone,
                    RoleName = user.Role?.Rolename,
                    DepartmentName = user.Department?.Name,
                    PositionName = user.Position?.Titlename,
                    IsActive = user.Isactive,
                    CreatedAt = user.Createdat
                };

                _logger.LogInformation("Nhân viên {Username} đã chấp nhận lời mời và tạo tài khoản", user.Username);

                return Ok(new LoginResponseDto
                {
                    Success = true,
                    Message = "Tạo tài khoản thành công",
                    User = userResponse,
                    Token = token
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi chấp nhận lời mời");
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi tạo tài khoản" });
            }
        }
    }
}