using HRMCyberse.Constants;
using HRMCyberse.Data;
using HRMCyberse.DTOs;
using HRMCyberse.Models;
using Microsoft.EntityFrameworkCore;

namespace HRMCyberse.Services
{
    public class RequestService : IRequestService
    {
        private readonly CybersehrmContext _context;

        public RequestService(CybersehrmContext context)
        {
            _context = context;
        }

        // Leave Requests
        public async Task<LeaveRequestResponseDto> CreateLeaveRequestAsync(CreateLeaveRequestDto dto)
        {
            if (dto.EndDate < dto.StartDate)
                throw new ArgumentException("End date must be after start date.");

            var leaveRequest = new Leaverequest
            {
                Userid = dto.UserId,
                Startdate = dto.StartDate,
                Enddate = dto.EndDate,
                Reason = dto.Reason,
                Status = RequestConstants.Status.Pending,
                Createdat = DateTime.UtcNow
            };

            _context.Leaverequests.Add(leaveRequest);
            await _context.SaveChangesAsync();

            return await GetLeaveRequestByIdAsync(leaveRequest.Id) 
                ?? throw new Exception("Failed to retrieve created leave request.");
        }

        public async Task<LeaveRequestResponseDto> ReviewLeaveRequestAsync(ReviewLeaveRequestDto dto, int reviewerId)
        {
            var request = await _context.Leaverequests.FindAsync(dto.RequestId);
            if (request == null) throw new ArgumentException("Leave request not found.");
            if (request.Status != RequestConstants.Status.Pending)
                throw new InvalidOperationException("Only pending requests can be reviewed.");

            request.Status = dto.Status;
            request.Reviewedby = reviewerId;
            request.Reviewedat = DateTime.UtcNow;

            _context.Leaverequests.Update(request);
            await _context.SaveChangesAsync();

            return await GetLeaveRequestByIdAsync(request.Id)
                ?? throw new Exception("Failed to retrieve reviewed leave request.");
        }

        public async Task<LeaveRequestResponseDto?> GetLeaveRequestByIdAsync(int id)
        {
            var request = await _context.Leaverequests
                .Include(lr => lr.User)
                .Include(lr => lr.ReviewedbyNavigation)
                .FirstOrDefaultAsync(lr => lr.Id == id);

            if (request == null) return null;

            var totalDays = request.Enddate.DayNumber - request.Startdate.DayNumber + 1;
            return new LeaveRequestResponseDto
            {
                Id = request.Id,
                UserId = request.Userid,
                UserName = request.User?.Fullname ?? "Unknown",
                StartDate = request.Startdate,
                EndDate = request.Enddate,
                TotalDays = totalDays,
                Reason = request.Reason,
                Status = request.Status,
                ReviewedBy = request.Reviewedby,
                ReviewerName = request.ReviewedbyNavigation?.Fullname,
                ReviewedAt = request.Reviewedat,
                CreatedAt = request.Createdat
            };
        }

        public async Task<List<LeaveRequestResponseDto>> GetUserLeaveRequestsAsync(int userId, string? status = null)
        {
            var query = _context.Leaverequests
                .Include(lr => lr.User)
                .Include(lr => lr.ReviewedbyNavigation)
                .Where(lr => lr.Userid == userId);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(lr => lr.Status == status);

            var requests = await query.OrderByDescending(lr => lr.Createdat).ToListAsync();
            return requests.Select(r => GetLeaveRequestByIdAsync(r.Id).Result!).ToList();
        }

        public async Task<List<LeaveRequestResponseDto>> GetPendingLeaveRequestsAsync()
        {
            var requests = await _context.Leaverequests
                .Include(lr => lr.User)
                .Include(lr => lr.ReviewedbyNavigation)
                .Where(lr => lr.Status == RequestConstants.Status.Pending)
                .OrderBy(lr => lr.Createdat)
                .ToListAsync();

            return requests.Select(r => GetLeaveRequestByIdAsync(r.Id).Result!).ToList();
        }

        public async Task<bool> CancelLeaveRequestAsync(int requestId, int userId)
        {
            var request = await _context.Leaverequests.FindAsync(requestId);
            if (request == null || request.Userid != userId) return false;
            if (request.Status != RequestConstants.Status.Pending)
                throw new InvalidOperationException("Only pending requests can be cancelled.");

            request.Status = RequestConstants.Status.Cancelled;
            _context.Leaverequests.Update(request);
            await _context.SaveChangesAsync();
            return true;
        }

        // Shift Requests - Similar implementation
        public async Task<ShiftRequestResponseDto> CreateShiftRequestAsync(CreateShiftRequestDto dto)
        {
            var shift = await _context.Shifts.FindAsync(dto.ShiftId);
            if (shift == null) throw new ArgumentException("Shift not found.");

            var shiftRequest = new Shiftrequest
            {
                Userid = dto.UserId,
                Shiftid = dto.ShiftId,
                Shiftdate = dto.ShiftDate,
                Reason = dto.Reason,
                Status = RequestConstants.Status.Pending,
                Createdat = DateTime.UtcNow
            };

            _context.Shiftrequests.Add(shiftRequest);
            await _context.SaveChangesAsync();

            return await GetShiftRequestByIdAsync(shiftRequest.Id)
                ?? throw new Exception("Failed to retrieve created shift request.");
        }

        public async Task<ShiftRequestResponseDto> ReviewShiftRequestAsync(ReviewShiftRequestDto dto, int reviewerId)
        {
            var request = await _context.Shiftrequests.FindAsync(dto.RequestId);
            if (request == null) throw new ArgumentException("Shift request not found.");
            if (request.Status != RequestConstants.Status.Pending)
                throw new InvalidOperationException("Only pending requests can be reviewed.");

            request.Status = dto.Status;
            request.Reviewedby = reviewerId;
            request.Reviewedat = DateTime.UtcNow;

            _context.Shiftrequests.Update(request);
            await _context.SaveChangesAsync();

            return await GetShiftRequestByIdAsync(request.Id)
                ?? throw new Exception("Failed to retrieve reviewed shift request.");
        }

        public async Task<ShiftRequestResponseDto?> GetShiftRequestByIdAsync(int id)
        {
            var request = await _context.Shiftrequests
                .Include(sr => sr.User)
                .Include(sr => sr.Shift)
                .Include(sr => sr.ReviewedbyNavigation)
                .FirstOrDefaultAsync(sr => sr.Id == id);

            if (request == null) return null;

            return new ShiftRequestResponseDto
            {
                Id = request.Id,
                UserId = request.Userid,
                UserName = request.User?.Fullname ?? "Unknown",
                ShiftId = request.Shiftid,
                ShiftName = request.Shift?.Name ?? "Unknown",
                ShiftDate = request.Shiftdate,
                Reason = request.Reason,
                Status = request.Status,
                ReviewedBy = request.Reviewedby,
                ReviewerName = request.ReviewedbyNavigation?.Fullname,
                ReviewedAt = request.Reviewedat,
                CreatedAt = request.Createdat
            };
        }

        public async Task<List<ShiftRequestResponseDto>> GetUserShiftRequestsAsync(int userId, string? status = null)
        {
            var query = _context.Shiftrequests
                .Include(sr => sr.User)
                .Include(sr => sr.Shift)
                .Include(sr => sr.ReviewedbyNavigation)
                .Where(sr => sr.Userid == userId);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(sr => sr.Status == status);

            var requests = await query.OrderByDescending(sr => sr.Createdat).ToListAsync();
            return requests.Select(r => GetShiftRequestByIdAsync(r.Id).Result!).ToList();
        }

        public async Task<List<ShiftRequestResponseDto>> GetPendingShiftRequestsAsync()
        {
            var requests = await _context.Shiftrequests
                .Include(sr => sr.User)
                .Include(sr => sr.Shift)
                .Include(sr => sr.ReviewedbyNavigation)
                .Where(sr => sr.Status == RequestConstants.Status.Pending)
                .OrderBy(sr => sr.Createdat)
                .ToListAsync();

            return requests.Select(r => GetShiftRequestByIdAsync(r.Id).Result!).ToList();
        }

        public async Task<bool> CancelShiftRequestAsync(int requestId, int userId)
        {
            var request = await _context.Shiftrequests.FindAsync(requestId);
            if (request == null || request.Userid != userId) return false;
            if (request.Status != RequestConstants.Status.Pending)
                throw new InvalidOperationException("Only pending requests can be cancelled.");

            request.Status = RequestConstants.Status.Cancelled;
            _context.Shiftrequests.Update(request);
            await _context.SaveChangesAsync();
            return true;
        }

        // Late Requests - Similar implementation
        public async Task<LateRequestResponseDto> CreateLateRequestAsync(CreateLateRequestDto dto)
        {
            var shift = await _context.Shifts.FindAsync(dto.ShiftId);
            if (shift == null) throw new ArgumentException("Shift not found.");
            if (dto.ExpectedArrivalTime <= shift.Starttime)
                throw new ArgumentException("Expected arrival time must be after shift start time.");

            var lateRequest = new Laterequest
            {
                Userid = dto.UserId,
                Shiftid = dto.ShiftId,
                Requestdate = dto.RequestDate,
                Expectedarrivaltime = dto.ExpectedArrivalTime,
                Reason = dto.Reason,
                Status = RequestConstants.Status.Pending,
                Createdat = DateTime.UtcNow
            };

            _context.Laterequests.Add(lateRequest);
            await _context.SaveChangesAsync();

            return await GetLateRequestByIdAsync(lateRequest.Id)
                ?? throw new Exception("Failed to retrieve created late request.");
        }

        public async Task<LateRequestResponseDto> ReviewLateRequestAsync(ReviewLateRequestDto dto, int reviewerId)
        {
            var request = await _context.Laterequests.FindAsync(dto.RequestId);
            if (request == null) throw new ArgumentException("Late request not found.");
            if (request.Status != RequestConstants.Status.Pending)
                throw new InvalidOperationException("Only pending requests can be reviewed.");

            request.Status = dto.Status;
            request.Reviewedby = reviewerId;
            request.Reviewedat = DateTime.UtcNow;

            _context.Laterequests.Update(request);
            await _context.SaveChangesAsync();

            return await GetLateRequestByIdAsync(request.Id)
                ?? throw new Exception("Failed to retrieve reviewed late request.");
        }

        public async Task<LateRequestResponseDto?> GetLateRequestByIdAsync(int id)
        {
            var request = await _context.Laterequests
                .Include(lr => lr.User)
                .Include(lr => lr.Shift)
                .Include(lr => lr.ReviewedbyNavigation)
                .FirstOrDefaultAsync(lr => lr.Id == id);

            if (request == null) return null;

            TimeSpan? lateMinutes = null;
            if (request.Shift != null)
                lateMinutes = request.Expectedarrivaltime.ToTimeSpan() - request.Shift.Starttime.ToTimeSpan();

            return new LateRequestResponseDto
            {
                Id = request.Id,
                UserId = request.Userid,
                UserName = request.User?.Fullname ?? "Unknown",
                ShiftId = request.Shiftid,
                ShiftName = request.Shift?.Name ?? "Unknown",
                RequestDate = request.Requestdate,
                ExpectedArrivalTime = request.Expectedarrivaltime,
                ShiftStartTime = request.Shift?.Starttime,
                LateMinutes = lateMinutes,
                Reason = request.Reason,
                Status = request.Status,
                ReviewedBy = request.Reviewedby,
                ReviewerName = request.ReviewedbyNavigation?.Fullname,
                ReviewedAt = request.Reviewedat,
                CreatedAt = request.Createdat
            };
        }

        public async Task<List<LateRequestResponseDto>> GetUserLateRequestsAsync(int userId, string? status = null)
        {
            var query = _context.Laterequests
                .Include(lr => lr.User)
                .Include(lr => lr.Shift)
                .Include(lr => lr.ReviewedbyNavigation)
                .Where(lr => lr.Userid == userId);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(lr => lr.Status == status);

            var requests = await query.OrderByDescending(lr => lr.Createdat).ToListAsync();
            return requests.Select(r => GetLateRequestByIdAsync(r.Id).Result!).ToList();
        }

        public async Task<List<LateRequestResponseDto>> GetPendingLateRequestsAsync()
        {
            var requests = await _context.Laterequests
                .Include(lr => lr.User)
                .Include(lr => lr.Shift)
                .Include(lr => lr.ReviewedbyNavigation)
                .Where(lr => lr.Status == RequestConstants.Status.Pending)
                .OrderBy(lr => lr.Createdat)
                .ToListAsync();

            return requests.Select(r => GetLateRequestByIdAsync(r.Id).Result!).ToList();
        }

        public async Task<bool> CancelLateRequestAsync(int requestId, int userId)
        {
            var request = await _context.Laterequests.FindAsync(requestId);
            if (request == null || request.Userid != userId) return false;
            if (request.Status != RequestConstants.Status.Pending)
                throw new InvalidOperationException("Only pending requests can be cancelled.");

            request.Status = RequestConstants.Status.Cancelled;
            _context.Laterequests.Update(request);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
