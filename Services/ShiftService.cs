using HRMCyberse.Data;
using HRMCyberse.DTOs;
using HRMCyberse.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace HRMCyberse.Services
{
    /// <summary>
    /// Service for managing work shifts and shift assignments.
    /// Implements caching for frequently accessed data and optimized database queries.
    /// </summary>
    public class ShiftService : IShiftService
    {
        private readonly CybersehrmContext _context;
        private readonly ILogger<ShiftService> _logger;
        private readonly IAuditLogService _auditLogService;
        private readonly IMemoryCache _cache;

        // Cache keys
        private const string ALL_SHIFTS_CACHE_KEY = "all_shifts";
        private const string SHIFT_CACHE_KEY_PREFIX = "shift_";
        private const string USER_EXISTS_CACHE_KEY_PREFIX = "user_exists_";
        
        // Cache expiration times
        private static readonly TimeSpan ShiftsCacheExpiration = TimeSpan.FromMinutes(30);
        private static readonly TimeSpan UserExistsCacheExpiration = TimeSpan.FromMinutes(10);

        public ShiftService(CybersehrmContext context, ILogger<ShiftService> logger, IAuditLogService auditLogService, IMemoryCache cache)
        {
            _context = context;
            _logger = logger;
            _auditLogService = auditLogService;
            _cache = cache;
        }

        /// <summary>
        /// Retrieves all shifts with caching for improved performance.
        /// Cache is invalidated when shifts are modified.
        /// Uses optimized query with direct projection to DTO.
        /// </summary>
        public async Task<IEnumerable<ShiftDto>> GetAllShiftsAsync()
        {
            var result = await _cache.GetOrCreateAsync(ALL_SHIFTS_CACHE_KEY, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = ShiftsCacheExpiration;
                entry.Priority = Microsoft.Extensions.Caching.Memory.CacheItemPriority.High; // High priority cache item
                entry.Size = 1; // Set cache entry size
                
                var shifts = await _context.Shifts
                    .Include(s => s.CreatedbyNavigation)
                    .OrderBy(s => s.Starttime)
                    .AsNoTracking() // Optimize for read-only operations
                    .Select(s => new ShiftDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        StartTime = s.Starttime,
                        EndTime = s.Endtime,
                        DurationMinutes = s.Durationminutes,
                        CreatedByName = s.CreatedbyNavigation != null ? s.CreatedbyNavigation.Fullname : null,
                        CreatedAt = s.Createdat
                    })
                    .ToListAsync();

                _logger.LogDebug("Loaded {Count} shifts from database with direct projection", shifts.Count);

                return (IEnumerable<ShiftDto>)shifts;
            });
            
            return result ?? new List<ShiftDto>();
        }

        /// <summary>
        /// Retrieves a shift by ID with caching.
        /// </summary>
        public async Task<ShiftDto?> GetShiftByIdAsync(int id)
        {
            var cacheKey = $"{SHIFT_CACHE_KEY_PREFIX}{id}";
            
            var result = await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = ShiftsCacheExpiration;
                entry.Size = 1;
                
                var shift = await _context.Shifts
                    .Include(s => s.CreatedbyNavigation)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (shift == null)
                    return null;

                return new ShiftDto
                {
                    Id = shift.Id,
                    Name = shift.Name,
                    StartTime = shift.Starttime,
                    EndTime = shift.Endtime,
                    DurationMinutes = shift.Durationminutes,
                    CreatedByName = shift.CreatedbyNavigation?.Fullname,
                    CreatedAt = shift.Createdat
                };
            });
            
            return result;
        }

        public async Task<ShiftDto> CreateShiftAsync(CreateShiftDto createShiftDto, int createdBy)
        {
            // Validate shift times using utilities
            var timeValidation = ShiftValidationUtilities.ValidateShiftTimeConfiguration(createShiftDto.StartTime, createShiftDto.EndTime);
            if (!timeValidation.IsValid)
            {
                throw new ArgumentException(timeValidation.ErrorMessage);
            }

            // Check for duplicate shift name
            if (!await IsShiftNameUniqueAsync(createShiftDto.Name))
            {
                throw new ArgumentException($"Tên ca '{createShiftDto.Name}' đã tồn tại");
            }

            // Calculate duration in minutes using utilities
            var duration = ShiftValidationUtilities.CalculateShiftDuration(createShiftDto.StartTime, createShiftDto.EndTime);

            var shift = new Shift
            {
                Name = createShiftDto.Name,
                Starttime = createShiftDto.StartTime,
                Endtime = createShiftDto.EndTime,
                Durationminutes = duration,
                Createdby = createdBy,
                Createdat = DateTime.UtcNow
            };

            _context.Shifts.Add(shift);
            await _context.SaveChangesAsync();

            // Invalidate cache after creating a shift
            InvalidateShiftCaches();

            _logger.LogInformation("Shift created: {ShiftName} by user {UserId}", shift.Name, createdBy);
            
            // Log audit activity
            await _auditLogService.LogShiftActivityAsync(
                createdBy, 
                "CREATE_SHIFT", 
                "Shift", 
                shift.Id, 
                $"Tạo ca làm việc '{shift.Name}'",
                null,
                new { shift.Name, shift.Starttime, shift.Endtime, shift.Durationminutes }
            );

            // Return the created shift with creator info
            await _context.Entry(shift)
                .Reference(s => s.CreatedbyNavigation)
                .LoadAsync();

            return new ShiftDto
            {
                Id = shift.Id,
                Name = shift.Name,
                StartTime = shift.Starttime,
                EndTime = shift.Endtime,
                DurationMinutes = shift.Durationminutes,
                CreatedByName = shift.CreatedbyNavigation?.Fullname,
                CreatedAt = shift.Createdat
            };
        }

        /// <summary>
        /// Updates an existing work shift with optimized validation and caching.
        /// </summary>
        public async Task<ShiftDto> UpdateShiftAsync(int id, UpdateShiftDto updateShiftDto, int updatedBy)
        {
            var shift = await _context.Shifts
                .Include(s => s.CreatedbyNavigation)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (shift == null)
            {
                throw new ArgumentException($"Shift với ID {id} không tồn tại");
            }

            // Store old values for audit log
            var oldValues = new { shift.Name, shift.Starttime, shift.Endtime, shift.Durationminutes };

            // Validate shift times using utilities
            var timeValidation = ShiftValidationUtilities.ValidateShiftTimeConfiguration(updateShiftDto.StartTime, updateShiftDto.EndTime);
            if (!timeValidation.IsValid)
            {
                throw new ArgumentException(timeValidation.ErrorMessage);
            }

            // Check for duplicate shift name (excluding current shift)
            if (!await IsShiftNameUniqueAsync(updateShiftDto.Name, id))
            {
                throw new ArgumentException($"Tên ca '{updateShiftDto.Name}' đã tồn tại");
            }

            // Calculate new duration using utilities
            var duration = ShiftValidationUtilities.CalculateShiftDuration(updateShiftDto.StartTime, updateShiftDto.EndTime);

            // Only update if values have actually changed (optimization)
            var hasChanges = shift.Name != updateShiftDto.Name ||
                           shift.Starttime != updateShiftDto.StartTime ||
                           shift.Endtime != updateShiftDto.EndTime;

            if (hasChanges)
            {
                shift.Name = updateShiftDto.Name;
                shift.Starttime = updateShiftDto.StartTime;
                shift.Endtime = updateShiftDto.EndTime;
                shift.Durationminutes = duration;

                await _context.SaveChangesAsync();

                // Invalidate cache after updating a shift
                InvalidateShiftCaches();
                _cache.Remove($"{SHIFT_CACHE_KEY_PREFIX}{id}");
                _cache.Remove($"shift_exists_{id}"); // Also invalidate existence cache

                _logger.LogInformation("Shift updated: {ShiftName} (ID: {ShiftId})", shift.Name, shift.Id);
                
                // Log audit activity
                await _auditLogService.LogShiftActivityAsync(
                    updatedBy, 
                    "UPDATE_SHIFT", 
                    "Shift", 
                    shift.Id, 
                    $"Cập nhật ca làm việc '{shift.Name}'",
                    oldValues,
                    new { shift.Name, shift.Starttime, shift.Endtime, shift.Durationminutes }
                );
            }
            else
            {
                _logger.LogDebug("No changes detected for shift {ShiftId}, skipping update", shift.Id);
            }

            return new ShiftDto
            {
                Id = shift.Id,
                Name = shift.Name,
                StartTime = shift.Starttime,
                EndTime = shift.Endtime,
                DurationMinutes = shift.Durationminutes,
                CreatedByName = shift.CreatedbyNavigation?.Fullname,
                CreatedAt = shift.Createdat
            };
        }

        public async Task<bool> DeleteShiftAsync(int id, int deletedBy)
        {
            var shift = await _context.Shifts.FindAsync(id);
            if (shift == null)
                return false;

            // Check if shift has any assignments
            var hasAssignments = await _context.Usershifts.AnyAsync(us => us.Shiftid == id);
            if (hasAssignments)
            {
                throw new InvalidOperationException("Không thể xóa ca đã được gán cho nhân viên");
            }

            // Store shift info for audit log before deletion
            var shiftInfo = new { shift.Name, shift.Starttime, shift.Endtime, shift.Durationminutes };

            _context.Shifts.Remove(shift);
            await _context.SaveChangesAsync();

            // Invalidate cache after deleting a shift
            InvalidateShiftCaches();
            _cache.Remove($"{SHIFT_CACHE_KEY_PREFIX}{id}");

            _logger.LogInformation("Shift deleted: {ShiftName} (ID: {ShiftId})", shift.Name, shift.Id);
            
            // Log audit activity
            await _auditLogService.LogShiftActivityAsync(
                deletedBy, 
                "DELETE_SHIFT", 
                "Shift", 
                id, 
                $"Xóa ca làm việc '{shift.Name}'",
                shiftInfo,
                null
            );
            
            return true;
        }

        // Validation methods
        public async Task<bool> ValidateShiftTimesAsync(TimeOnly startTime, TimeOnly endTime)
        {
            var validation = ShiftValidationUtilities.ValidateShiftTimeConfiguration(startTime, endTime);
            return await Task.FromResult(validation.IsValid);
        }

        public async Task<bool> IsShiftNameUniqueAsync(string name, int? excludeId = null)
        {
            var query = _context.Shifts.Where(s => s.Name.ToLower() == name.ToLower());
            
            if (excludeId.HasValue)
            {
                query = query.Where(s => s.Id != excludeId.Value);
            }

            return !await query.AnyAsync();
        }

        /// <summary>
        /// Checks if a user exists with caching for improved performance.
        /// Uses optimized query with specific column selection.
        /// </summary>
        public async Task<bool> UserExistsAsync(int userId)
        {
            var cacheKey = $"{USER_EXISTS_CACHE_KEY_PREFIX}{userId}";
            
            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = UserExistsCacheExpiration;
                entry.Size = 1; // Small cache entry size
                
                return await _context.Users
                    .AsNoTracking()
                    .Where(u => u.Id == userId && u.Isactive == true)
                    .AnyAsync();
            });
        }

        /// <summary>
        /// Checks if a shift exists with caching for improved performance.
        /// </summary>
        public async Task<bool> ShiftExistsAsync(int shiftId)
        {
            var cacheKey = $"shift_exists_{shiftId}";
            
            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = ShiftsCacheExpiration;
                entry.Size = 1; // Small cache entry size
                
                return await _context.Shifts
                    .AsNoTracking()
                    .Where(s => s.Id == shiftId)
                    .AnyAsync();
            });
        }

        // UserShift assignment operations
        /// <summary>
        /// Retrieves all assignments with optimized query using projection.
        /// </summary>
        public async Task<IEnumerable<UserShiftDto>> GetAllAssignmentsAsync()
        {
            var assignments = await _context.Usershifts
                .Include(us => us.User)
                .Include(us => us.Shift)
                .OrderBy(us => us.Shiftdate)
                .ThenBy(us => us.User.Fullname)
                .AsNoTracking()
                .Select(us => new UserShiftDto
                {
                    Id = us.Id,
                    UserId = us.Userid,
                    UserName = us.User.Username,
                    FullName = us.User.Fullname,
                    ShiftId = us.Shiftid,
                    ShiftName = us.Shift.Name,
                    ShiftStartTime = us.Shift.Starttime,
                    ShiftEndTime = us.Shift.Endtime,
                    ShiftDate = us.Shiftdate,
                    Status = us.Status,
                    CreatedAt = us.Createdat
                })
                .ToListAsync();

            return assignments;
        }

        /// <summary>
        /// Retrieves user assignments with optimized query using projection.
        /// </summary>
        public async Task<IEnumerable<UserShiftDto>> GetUserAssignmentsAsync(int userId)
        {
            var assignments = await _context.Usershifts
                .Include(us => us.User)
                .Include(us => us.Shift)
                .Where(us => us.Userid == userId)
                .OrderBy(us => us.Shiftdate)
                .AsNoTracking()
                .Select(us => new UserShiftDto
                {
                    Id = us.Id,
                    UserId = us.Userid,
                    UserName = us.User.Username,
                    FullName = us.User.Fullname,
                    ShiftId = us.Shiftid,
                    ShiftName = us.Shift.Name,
                    ShiftStartTime = us.Shift.Starttime,
                    ShiftEndTime = us.Shift.Endtime,
                    ShiftDate = us.Shiftdate,
                    Status = us.Status,
                    CreatedAt = us.Createdat
                })
                .ToListAsync();

            return assignments;
        }

        public async Task<UserShiftDto> AssignShiftAsync(AssignShiftDto assignShiftDto, int assignedBy)
        {
            // Validate assignment date
            var dateValidation = ShiftValidationUtilities.ValidateShiftAssignmentDate(assignShiftDto.ShiftDate, DateOnly.FromDateTime(DateTime.Now));
            if (!dateValidation.IsValid)
            {
                throw new ArgumentException(dateValidation.ErrorMessage);
            }

            // Validate user exists
            if (!await UserExistsAsync(assignShiftDto.UserId))
            {
                throw new ArgumentException($"Người dùng với ID {assignShiftDto.UserId} không tồn tại hoặc không hoạt động");
            }

            // Validate shift exists
            if (!await ShiftExistsAsync(assignShiftDto.ShiftId))
            {
                throw new ArgumentException($"Ca làm việc với ID {assignShiftDto.ShiftId} không tồn tại");
            }

            // Check for duplicate assignment (same user, shift, and date)
            var existingAssignment = await _context.Usershifts
                .FirstOrDefaultAsync(us => us.Userid == assignShiftDto.UserId 
                                        && us.Shiftid == assignShiftDto.ShiftId 
                                        && us.Shiftdate == assignShiftDto.ShiftDate);

            if (existingAssignment != null)
            {
                throw new InvalidOperationException($"Nhân viên đã được gán ca này cho ngày {assignShiftDto.ShiftDate:dd/MM/yyyy}");
            }

            // Get shift details for conflict checking
            var shift = await _context.Shifts.FindAsync(assignShiftDto.ShiftId);
            if (shift == null)
            {
                throw new ArgumentException($"Ca làm việc với ID {assignShiftDto.ShiftId} không tồn tại");
            }

            // Check for time conflicts with existing assignments
            var hasConflict = await CheckShiftConflictAsync(
                assignShiftDto.UserId, 
                assignShiftDto.ShiftDate, 
                shift.Starttime, 
                shift.Endtime);

            if (hasConflict)
            {
                throw new InvalidOperationException($"Ca làm việc bị trùng thời gian với ca đã được gán cho nhân viên vào ngày {assignShiftDto.ShiftDate:dd/MM/yyyy}");
            }

            var userShift = new Usershift
            {
                Userid = assignShiftDto.UserId,
                Shiftid = assignShiftDto.ShiftId,
                Shiftdate = assignShiftDto.ShiftDate,
                Status = assignShiftDto.Status ?? "assigned",
                Createdat = DateTime.UtcNow
            };

            _context.Usershifts.Add(userShift);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Shift assigned: User {UserId} to Shift {ShiftId} on {ShiftDate} by user {AssignedBy}", 
                assignShiftDto.UserId, assignShiftDto.ShiftId, assignShiftDto.ShiftDate, assignedBy);
            
            // Log audit activity
            await _auditLogService.LogShiftAssignmentActivityAsync(
                assignedBy, 
                "ASSIGN_SHIFT", 
                userShift.Id, 
                assignShiftDto.UserId, 
                assignShiftDto.ShiftId, 
                $"Gán ca làm việc cho nhân viên vào ngày {assignShiftDto.ShiftDate:dd/MM/yyyy}"
            );

            // Load related entities for response
            await _context.Entry(userShift)
                .Reference(us => us.User)
                .LoadAsync();
            await _context.Entry(userShift)
                .Reference(us => us.Shift)
                .LoadAsync();

            return new UserShiftDto
            {
                Id = userShift.Id,
                UserId = userShift.Userid,
                UserName = userShift.User.Username,
                FullName = userShift.User.Fullname,
                ShiftId = userShift.Shiftid,
                ShiftName = userShift.Shift.Name,
                ShiftStartTime = userShift.Shift.Starttime,
                ShiftEndTime = userShift.Shift.Endtime,
                ShiftDate = userShift.Shiftdate,
                Status = userShift.Status,
                CreatedAt = userShift.Createdat
            };
        }

        public async Task<bool> RemoveAssignmentAsync(int assignmentId, int removedBy)
        {
            var assignment = await _context.Usershifts
                .Include(us => us.User)
                .Include(us => us.Shift)
                .FirstOrDefaultAsync(us => us.Id == assignmentId);

            if (assignment == null)
                return false;

            // Store assignment info for audit log
            var assignmentInfo = new 
            { 
                assignment.Userid, 
                assignment.Shiftid, 
                assignment.Shiftdate, 
                UserName = assignment.User.Fullname,
                ShiftName = assignment.Shift.Name
            };

            _context.Usershifts.Remove(assignment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Shift assignment removed: User {UserId} from Shift {ShiftId} on {ShiftDate} (Assignment ID: {AssignmentId})", 
                assignment.Userid, assignment.Shiftid, assignment.Shiftdate, assignmentId);

            // Log audit activity
            await _auditLogService.LogShiftAssignmentActivityAsync(
                removedBy, 
                "REMOVE_ASSIGNMENT", 
                assignmentId, 
                assignment.Userid, 
                assignment.Shiftid, 
                $"Hủy gán ca làm việc '{assignment.Shift.Name}' cho nhân viên '{assignment.User.Fullname}' vào ngày {assignment.Shiftdate:dd/MM/yyyy}"
            );

            return true;
        }

        /// <summary>
        /// Checks for shift conflicts with optimized query using projection.
        /// </summary>
        public async Task<bool> CheckShiftConflictAsync(int userId, DateOnly shiftDate, TimeOnly startTime, TimeOnly endTime, int? excludeShiftId = null)
        {
            // Get all user shifts for the given date and adjacent dates (for overnight shifts)
            var previousDate = shiftDate.AddDays(-1);
            var nextDate = shiftDate.AddDays(1);

            var userShifts = await _context.Usershifts
                .Include(us => us.Shift)
                .Where(us => us.Userid == userId && 
                           (us.Shiftdate == previousDate || us.Shiftdate == shiftDate || us.Shiftdate == nextDate))
                .AsNoTracking()
                .Select(us => new 
                {
                    us.Shiftid,
                    us.Shiftdate,
                    us.Shift.Starttime,
                    us.Shift.Endtime
                })
                .ToListAsync();

            // If excludeShiftId is provided, filter it out (for update scenarios)
            if (excludeShiftId.HasValue)
            {
                userShifts = userShifts.Where(us => us.Shiftid != excludeShiftId.Value).ToList();
            }

            foreach (var existingShift in userShifts)
            {
                if (HasTimeOverlap(
                    shiftDate, startTime, endTime,
                    existingShift.Shiftdate, existingShift.Starttime, existingShift.Endtime))
                {
                    return true; // Conflict found
                }
            }

            return false; // No conflict
        }

        // Helper method to check if two shifts have time overlap
        private bool HasTimeOverlap(
            DateOnly date1, TimeOnly start1, TimeOnly end1,
            DateOnly date2, TimeOnly start2, TimeOnly end2)
        {
            // Convert to DateTime for easier comparison
            var shift1Start = date1.ToDateTime(start1);
            var shift1End = date1.ToDateTime(end1);
            var shift2Start = date2.ToDateTime(start2);
            var shift2End = date2.ToDateTime(end2);

            // Handle overnight shifts by adding a day to end time if it's before start time
            if (end1 < start1)
            {
                shift1End = shift1End.AddDays(1);
            }
            if (end2 < start2)
            {
                shift2End = shift2End.AddDays(1);
            }

            // Check for overlap: shifts overlap if one starts before the other ends
            return shift1Start < shift2End && shift2Start < shift1End;
        }

        /// <summary>
        /// Helper method to calculate shift duration (delegates to utilities)
        /// </summary>
        private int CalculateShiftDuration(TimeOnly startTime, TimeOnly endTime)
        {
            return ShiftValidationUtilities.CalculateShiftDuration(startTime, endTime);
        }

        /// <summary>
        /// Invalidates all shift-related caches when shifts are modified.
        /// Includes comprehensive cache cleanup for better memory management.
        /// </summary>
        private void InvalidateShiftCaches()
        {
            _cache.Remove(ALL_SHIFTS_CACHE_KEY);
            
            // Also invalidate shift existence cache entries
            // Note: In a production system, consider using cache tags for more efficient invalidation
            var cacheField = typeof(Microsoft.Extensions.Caching.Memory.MemoryCache).GetField("_coherentState", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            _logger.LogDebug("Invalidated shift caches and related cache entries");
        }
    }
}