using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using HRMCyberse.Data;
using HRMCyberse.Models;

namespace HRMCyberse.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class WiFiLocationController : ControllerBase
    {
        private readonly CybersehrmContext _context;
        private readonly ILogger<WiFiLocationController> _logger;

        public WiFiLocationController(CybersehrmContext context, ILogger<WiFiLocationController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all WiFi locations
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            try
            {
                var locations = await _context.CompanyWifiLocations
                    .Where(w => w.IsActive == true)
                    .OrderBy(w => w.LocationName)
                    .ToListAsync();

                return Ok(locations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting WiFi locations");
                return StatusCode(500, "Lỗi khi lấy danh sách WiFi");
            }
        }

        /// <summary>
        /// Add new WiFi location
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> Add([FromBody] AddWiFiRequest request)
        {
            try
            {
                var location = new CompanyWifiLocation
                {
                    LocationName = request.LocationName,
                    WifiSsid = request.WifiSsid,
                    WifiBssid = request.WifiBssid,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.CompanyWifiLocations.Add(location);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Added WiFi location: {Name}", request.LocationName);

                return Ok(new
                {
                    success = true,
                    message = "Thêm WiFi thành công",
                    location = location
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding WiFi location");
                return StatusCode(500, "Lỗi khi thêm WiFi");
            }
        }

        /// <summary>
        /// Update WiFi location
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] AddWiFiRequest request)
        {
            try
            {
                var location = await _context.CompanyWifiLocations.FindAsync(id);
                if (location == null)
                {
                    return NotFound("Không tìm thấy WiFi");
                }

                location.LocationName = request.LocationName;
                location.WifiSsid = request.WifiSsid;
                location.WifiBssid = request.WifiBssid;
                location.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated WiFi location: {Id}", id);

                return Ok(new
                {
                    success = true,
                    message = "Cập nhật WiFi thành công",
                    location = location
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating WiFi location");
                return StatusCode(500, "Lỗi khi cập nhật WiFi");
            }
        }

        /// <summary>
        /// Delete WiFi location
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var location = await _context.CompanyWifiLocations.FindAsync(id);
                if (location == null)
                {
                    return NotFound("Không tìm thấy WiFi");
                }

                location.IsActive = false;
                location.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted WiFi location: {Id}", id);

                return Ok(new { success = true, message = "Xóa WiFi thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting WiFi location");
                return StatusCode(500, "Lỗi khi xóa WiFi");
            }
        }
    }

    public class AddWiFiRequest
    {
        public string LocationName { get; set; } = null!;
        public string WifiSsid { get; set; } = null!;
        public string? WifiBssid { get; set; }
    }
}
