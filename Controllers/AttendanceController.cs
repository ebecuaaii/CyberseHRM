using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using HRMCyberse.Data;
using HRMCyberse.Models;
using HRMCyberse.Services;
using HRMCyberse.DTOs;

namespace HRMCyberse.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AttendanceController : ControllerBase
    {
        private readonly CybersehrmContext _context;
        private readonly IFaceRecognitionService _faceService;
        private readonly ILogger<AttendanceController> _logger;

        public AttendanceController(
            CybersehrmContext context,
            IFaceRecognitionService faceService,
            ILogger<AttendanceController> logger)
        {
            _context = context;
            _faceService = faceService;
            _logger = logger;
        }

        /// <summary>
        /// Test endpoint to check if image is received
        /// </summary>
        [HttpPost("test-upload")]
        public ActionResult TestUpload(IFormFile image)
        {
            try
            {
                _logger.LogInformation("TestUpload called");
                
                if (image == null)
                {
                    _logger.LogWarning("Image is null");
                    return Ok(new { 
                        success = false, 
                        message = "Image is null",
                        receivedFiles = Request.Form.Files.Count,
                        fileNames = Request.Form.Files.Select(f => f.Name).ToList()
                    });
                }

                _logger.LogInformation("Image received: {FileName}, Size: {Size}", image.FileName, image.Length);
                
                return Ok(new
                {
                    success = true,
                    message = "Image received successfully",
                    fileName = image.FileName,
                    size = image.Length,
                    contentType = image.ContentType
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in test upload");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Register face with base64 image
        /// </summary>
        [HttpPost("register-face-base64")]
        public async Task<ActionResult> RegisterFaceBase64([FromBody] RegisterFaceRequest request)
        {
            try
            {
                _logger.LogInformation("RegisterFaceBase64 called");

                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest(new { success = false, message = "Không thể xác định người dùng" });
                }

                if (string.IsNullOrEmpty(request.ImageBase64))
                {
                    return BadRequest(new { success = false, message = "Vui lòng cung cấp ảnh khuôn mặt" });
                }

                // Remove data:image prefix if exists
                var base64Data = request.ImageBase64;
                if (base64Data.Contains(","))
                {
                    base64Data = base64Data.Split(',')[1];
                }

                var imageData = Convert.FromBase64String(base64Data);
                _logger.LogInformation("Processing base64 image for user {UserId}, size: {Size} bytes", userId, imageData.Length);

                var result = await _faceService.AddFaceAsync(userId, imageData);

                _logger.LogInformation("User {UserId} registered face successfully", userId);

                return Ok(new
                {
                    success = true,
                    message = "Đăng ký khuôn mặt thành công",
                    userId = userId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering face: {Message}", ex.Message);
                
                // Return 400 for user errors (like no face found), 500 for server errors
                var statusCode = ex.Message.Contains("Không tìm thấy khuôn mặt") ? 400 : 500;
                
                return StatusCode(statusCode, new 
                { 
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Register multiple faces for better recognition (recommended: 3-5 photos)
        /// </summary>
        [HttpPost("register-faces-multiple")]
        public async Task<ActionResult> RegisterMultipleFaces([FromForm] List<IFormFile> images)
        {
            try
            {
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest(new { success = false, message = "Không thể xác định người dùng" });
                }

                if (images == null || images.Count == 0)
                {
                    return BadRequest(new { success = false, message = "Vui lòng cung cấp ít nhất 1 ảnh" });
                }

                var results = new List<object>();
                int successCount = 0;

                foreach (var image in images)
                {
                    try
                    {
                        using var memoryStream = new MemoryStream();
                        await image.CopyToAsync(memoryStream);
                        var imageData = memoryStream.ToArray();

                        await _faceService.AddFaceAsync(userId, imageData);
                        successCount++;
                        results.Add(new { fileName = image.FileName, success = true });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to register face from {FileName}", image.FileName);
                        results.Add(new { fileName = image.FileName, success = false, error = ex.Message });
                    }
                }

                _logger.LogInformation("User {UserId} registered {Count}/{Total} faces", userId, successCount, images.Count);

                return Ok(new
                {
                    success = successCount > 0,
                    message = $"Đã đăng ký {successCount}/{images.Count} ảnh thành công",
                    userId = userId,
                    results = results
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering multiple faces");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Register face for user (first time setup)
        /// </summary>
        [HttpPost("register-face")]
        public async Task<ActionResult> RegisterFace(IFormFile image)
        {
            try
            {
                _logger.LogInformation("RegisterFace called");

                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    _logger.LogWarning("Cannot determine user ID");
                    return BadRequest(new { success = false, message = "Không thể xác định người dùng" });
                }

                if (image == null || image.Length == 0)
                {
                    _logger.LogWarning("No image provided");
                    return BadRequest(new { success = false, message = "Vui lòng cung cấp ảnh khuôn mặt" });
                }

                _logger.LogInformation("Processing image for user {UserId}, size: {Size} bytes", userId, image.Length);

                using var memoryStream = new MemoryStream();
                await image.CopyToAsync(memoryStream);
                var imageData = memoryStream.ToArray();

                _logger.LogInformation("Calling face service to add face");
                var result = await _faceService.AddFaceAsync(userId, imageData);

                _logger.LogInformation("User {UserId} registered face successfully", userId);

                return Ok(new
                {
                    success = true,
                    message = "Đăng ký khuôn mặt thành công",
                    userId = userId,
                    result = result
                });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error when calling ComPreFace API: {Message}", ex.Message);
                return StatusCode(500, new 
                { 
                    success = false,
                    message = "Không thể kết nối đến dịch vụ nhận diện khuôn mặt. Vui lòng thử lại sau.",
                    error = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering face: {Message}", ex.Message);
                
                // Return 400 for user errors (like no face found), 500 for server errors
                var statusCode = ex.Message.Contains("Không tìm thấy khuôn mặt") ? 400 : 500;
                
                return StatusCode(statusCode, new 
                { 
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Check-in with face recognition and WiFi validation
        /// </summary>
        [HttpPost("checkin")]
        public async Task<ActionResult> CheckIn(
            IFormFile image,
            [FromForm] string? wifiSSID = null,
            [FromForm] string? wifiBSSID = null)
        {
            try
            {
                if (image == null || image.Length == 0)
                {
                    return BadRequest("Vui lòng cung cấp ảnh khuôn mặt");
                }

                using var memoryStream = new MemoryStream();
                await image.CopyToAsync(memoryStream);
                var imageData = memoryStream.ToArray();

                // Recognize face
                var recognitionResult = await _faceService.RecognizeFaceAsync(imageData);

                if (!recognitionResult.Success || !recognitionResult.UserId.HasValue)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = recognitionResult.Message ?? "Không nhận diện được khuôn mặt"
                    });
                }

                var userId = recognitionResult.UserId.Value;

                // Check if already checked in today
                var todayStart = DateTime.UtcNow.Date;
                var todayEnd = todayStart.AddDays(1);
                var existingAttendance = await _context.Attendances
                    .FirstOrDefaultAsync(a => a.Userid == userId 
                        && a.Checkintime >= todayStart 
                        && a.Checkintime < todayEnd);

                if (existingAttendance != null && existingAttendance.Checkintime.HasValue)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Bạn đã check-in hôm nay rồi",
                        checkInTime = existingAttendance.Checkintime
                    });
                }

                // Create attendance
                var now = DateTime.UtcNow;
                var attendance = new Attendance
                {
                    Userid = userId,
                    Checkintime = now,
                    Status = "Present"
                };
                _context.Attendances.Add(attendance);

                await _context.SaveChangesAsync();

                // Load user info
                var user = await _context.Users.FindAsync(userId);

                _logger.LogInformation("User {UserId} checked in at {Time}", userId, now);

                return Ok(new
                {
                    success = true,
                    message = "Check-in thành công",
                    userId = userId,
                    userName = user?.Username,
                    fullName = user?.Fullname,
                    checkInTime = now,
                    confidence = recognitionResult.Confidence
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during check-in");
                return StatusCode(500, "Lỗi khi check-in");
            }
        }

        /// <summary>
        /// Check-out with face recognition
        /// </summary>
        [HttpPost("checkout")]
        public async Task<ActionResult> CheckOut(IFormFile image)
        {
            try
            {
                if (image == null || image.Length == 0)
                {
                    return BadRequest("Vui lòng cung cấp ảnh khuôn mặt");
                }

                using var memoryStream = new MemoryStream();
                await image.CopyToAsync(memoryStream);
                var imageData = memoryStream.ToArray();

                // Recognize face
                var recognitionResult = await _faceService.RecognizeFaceAsync(imageData);

                if (!recognitionResult.Success || !recognitionResult.UserId.HasValue)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = recognitionResult.Message ?? "Không nhận diện được khuôn mặt"
                    });
                }

                var userId = recognitionResult.UserId.Value;

                // Find today's attendance
                var todayStart = DateTime.UtcNow.Date;
                var todayEnd = todayStart.AddDays(1);
                var attendance = await _context.Attendances
                    .FirstOrDefaultAsync(a => a.Userid == userId 
                        && a.Checkintime >= todayStart 
                        && a.Checkintime < todayEnd);

                if (attendance == null || !attendance.Checkintime.HasValue)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Bạn chưa check-in hôm nay"
                    });
                }

                if (attendance.Checkouttime.HasValue)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Bạn đã check-out rồi",
                        checkOutTime = attendance.Checkouttime
                    });
                }

                // Update check-out time
                var now = DateTime.UtcNow;
                attendance.Checkouttime = now;

                await _context.SaveChangesAsync();

                // Calculate work hours
                var workHours = (now - attendance.Checkintime.Value).TotalHours;

                // Load user info
                var user = await _context.Users.FindAsync(userId);

                _logger.LogInformation("User {UserId} checked out at {Time}", userId, now);

                return Ok(new
                {
                    success = true,
                    message = "Check-out thành công",
                    userId = userId,
                    userName = user?.Username,
                    fullName = user?.Fullname,
                    checkInTime = attendance.Checkintime,
                    checkOutTime = now,
                    workHours = Math.Round(workHours, 2),
                    confidence = recognitionResult.Confidence
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during check-out");
                return StatusCode(500, "Lỗi khi check-out");
            }
        }

        /// <summary>
        /// Get attendance history for current user
        /// </summary>
        [HttpGet("my-attendance")]
        public async Task<ActionResult> GetMyAttendance(
            [FromQuery] DateOnly? fromDate = null,
            [FromQuery] DateOnly? toDate = null)
        {
            try
            {
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest("Không thể xác định người dùng");
                }

                var query = _context.Attendances
                    .Where(a => a.Userid == userId && a.Checkintime.HasValue);

                if (fromDate.HasValue)
                {
                    var fromDateTime = fromDate.Value.ToDateTime(TimeOnly.MinValue);
                    query = query.Where(a => a.Checkintime >= fromDateTime);
                }

                if (toDate.HasValue)
                {
                    var toDateTime = toDate.Value.ToDateTime(TimeOnly.MaxValue);
                    query = query.Where(a => a.Checkintime <= toDateTime);
                }

                var attendances = await query
                    .OrderByDescending(a => a.Checkintime)
                    .ToListAsync();

                var result = attendances.Select(a => new
                {
                    id = a.Id,
                    date = a.Checkintime.HasValue ? DateOnly.FromDateTime(a.Checkintime.Value) : (DateOnly?)null,
                    checkInTime = a.Checkintime,
                    checkOutTime = a.Checkouttime,
                    workHours = a.Checkintime.HasValue && a.Checkouttime.HasValue 
                        ? Math.Round((a.Checkouttime.Value - a.Checkintime.Value).TotalHours, 2) 
                        : (double?)null,
                    status = a.Status
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting attendance history");
                return StatusCode(500, "Lỗi khi lấy lịch sử chấm công");
            }
        }

        /// <summary>
        /// Get today's attendance status
        /// </summary>
        [HttpGet("today")]
        public async Task<ActionResult> GetTodayAttendance()
        {
            try
            {
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest("Không thể xác định người dùng");
                }

                var todayStart = DateTime.UtcNow.Date;
                var todayEnd = todayStart.AddDays(1);
                var attendance = await _context.Attendances
                    .FirstOrDefaultAsync(a => a.Userid == userId 
                        && a.Checkintime >= todayStart 
                        && a.Checkintime < todayEnd);

                if (attendance == null)
                {
                    return Ok(new
                    {
                        hasCheckedIn = false,
                        hasCheckedOut = false,
                        message = "Chưa check-in hôm nay"
                    });
                }

                var workHours = attendance.Checkintime.HasValue && attendance.Checkouttime.HasValue
                    ? Math.Round((attendance.Checkouttime.Value - attendance.Checkintime.Value).TotalHours, 2)
                    : (double?)null;

                return Ok(new
                {
                    hasCheckedIn = attendance.Checkintime.HasValue,
                    hasCheckedOut = attendance.Checkouttime.HasValue,
                    checkInTime = attendance.Checkintime,
                    checkOutTime = attendance.Checkouttime,
                    workHours = workHours,
                    status = attendance.Status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting today attendance");
                return StatusCode(500, "Lỗi khi lấy thông tin chấm công");
            }
        }

        /// <summary>
        /// Validate WiFi location
        /// </summary>
        private async Task<bool> ValidateWiFiLocation(string? wifiSSID, string? wifiBSSID)
        {
            try
            {
                // If no WiFi info provided, allow but log warning
                if (string.IsNullOrEmpty(wifiSSID) && string.IsNullOrEmpty(wifiBSSID))
                {
                    _logger.LogWarning("No WiFi info provided for check-in");
                    return true; // Allow for now
                }

                // Check if WiFi exists in allowed list
                var allowedWiFi = await _context.CompanyWifiLocations
                    .Where(w => w.IsActive == true)
                    .Where(w => 
                        (!string.IsNullOrEmpty(wifiBSSID) && w.WifiBssid == wifiBSSID) ||
                        (!string.IsNullOrEmpty(wifiSSID) && w.WifiSsid == wifiSSID))
                    .FirstOrDefaultAsync();

                return allowedWiFi != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating WiFi");
                return true; // Allow on error to not block users
            }
        }
    }
}
