using HRMCyberse.Data;
using HRMCyberse.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace HRMCyberse.Services
{
    /// <summary>
    /// Service for logging shift management activities
    /// </summary>
    public class AuditLogService : IAuditLogService
    {
        private readonly CybersehrmContext _context;
        private readonly ILogger<AuditLogService> _logger;

        public AuditLogService(CybersehrmContext context, ILogger<AuditLogService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task LogShiftActivityAsync(int userId, string action, string entityType, int? entityId = null, 
            string? details = null, object? oldValues = null, object? newValues = null)
        {
            try
            {
                var activityDetails = new
                {
                    EntityType = entityType,
                    EntityId = entityId,
                    Action = action,
                    Details = details,
                    OldValues = oldValues,
                    NewValues = newValues,
                    Timestamp = DateTime.UtcNow
                };

                var activityLog = new Activitylog
                {
                    Userid = userId,
                    Action = $"SHIFT_MANAGEMENT: {action}",
                    Description = JsonSerializer.Serialize(activityDetails, new JsonSerializerOptions 
                    { 
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        WriteIndented = false
                    }),
                    Createdat = DateTime.UtcNow
                };

                _context.Activitylogs.Add(activityLog);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Shift activity logged: User {UserId} performed {Action} on {EntityType} {EntityId}", 
                    userId, action, entityType, entityId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log shift activity for user {UserId}, action {Action}", userId, action);
                // Don't throw - audit logging failure shouldn't break the main operation
            }
        }

        public async Task LogShiftAssignmentActivityAsync(int userId, string action, int? assignmentId = null, 
            int? targetUserId = null, int? shiftId = null, string? details = null)
        {
            try
            {
                // Get additional context information
                string? targetUserName = null;
                string? shiftName = null;

                if (targetUserId.HasValue)
                {
                    var targetUser = await _context.Users
                        .Where(u => u.Id == targetUserId.Value)
                        .Select(u => u.Fullname)
                        .FirstOrDefaultAsync();
                    targetUserName = targetUser;
                }

                if (shiftId.HasValue)
                {
                    var shift = await _context.Shifts
                        .Where(s => s.Id == shiftId.Value)
                        .Select(s => s.Name)
                        .FirstOrDefaultAsync();
                    shiftName = shift;
                }

                var activityDetails = new
                {
                    EntityType = "UserShift",
                    AssignmentId = assignmentId,
                    TargetUserId = targetUserId,
                    TargetUserName = targetUserName,
                    ShiftId = shiftId,
                    ShiftName = shiftName,
                    Action = action,
                    Details = details,
                    Timestamp = DateTime.UtcNow
                };

                var activityLog = new Activitylog
                {
                    Userid = userId,
                    Action = $"SHIFT_ASSIGNMENT: {action}",
                    Description = JsonSerializer.Serialize(activityDetails, new JsonSerializerOptions 
                    { 
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        WriteIndented = false
                    }),
                    Createdat = DateTime.UtcNow
                };

                _context.Activitylogs.Add(activityLog);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Shift assignment activity logged: User {UserId} performed {Action} for assignment {AssignmentId}", 
                    userId, action, assignmentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log shift assignment activity for user {UserId}, action {Action}", userId, action);
                // Don't throw - audit logging failure shouldn't break the main operation
            }
        }

        public async Task LogGeneralShiftActivityAsync(int userId, string action, string? details = null)
        {
            try
            {
                var activityDetails = new
                {
                    EntityType = "ShiftManagement",
                    Action = action,
                    Details = details,
                    Timestamp = DateTime.UtcNow
                };

                var activityLog = new Activitylog
                {
                    Userid = userId,
                    Action = $"SHIFT_MANAGEMENT: {action}",
                    Description = JsonSerializer.Serialize(activityDetails, new JsonSerializerOptions 
                    { 
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        WriteIndented = false
                    }),
                    Createdat = DateTime.UtcNow
                };

                _context.Activitylogs.Add(activityLog);
                await _context.SaveChangesAsync();

                _logger.LogInformation("General shift activity logged: User {UserId} performed {Action}", userId, action);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log general shift activity for user {UserId}, action {Action}", userId, action);
                // Don't throw - audit logging failure shouldn't break the main operation
            }
        }
    }
}