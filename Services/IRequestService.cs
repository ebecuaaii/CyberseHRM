using HRMCyberse.DTOs;

namespace HRMCyberse.Services
{
    public interface IRequestService
    {
        // Leave Requests
        Task<LeaveRequestResponseDto> CreateLeaveRequestAsync(CreateLeaveRequestDto dto);
        Task<LeaveRequestResponseDto> ReviewLeaveRequestAsync(ReviewLeaveRequestDto dto, int reviewerId);
        Task<LeaveRequestResponseDto?> GetLeaveRequestByIdAsync(int id);
        Task<List<LeaveRequestResponseDto>> GetUserLeaveRequestsAsync(int userId, string? status = null);
        Task<List<LeaveRequestResponseDto>> GetPendingLeaveRequestsAsync();
        Task<bool> CancelLeaveRequestAsync(int requestId, int userId);

        // Shift Requests
        Task<ShiftRequestResponseDto> CreateShiftRequestAsync(CreateShiftRequestDto dto);
        Task<ShiftRequestResponseDto> ReviewShiftRequestAsync(ReviewShiftRequestDto dto, int reviewerId);
        Task<ShiftRequestResponseDto?> GetShiftRequestByIdAsync(int id);
        Task<List<ShiftRequestResponseDto>> GetUserShiftRequestsAsync(int userId, string? status = null);
        Task<List<ShiftRequestResponseDto>> GetPendingShiftRequestsAsync();
        Task<bool> CancelShiftRequestAsync(int requestId, int userId);

        // Late Requests
        Task<LateRequestResponseDto> CreateLateRequestAsync(CreateLateRequestDto dto);
        Task<LateRequestResponseDto> ReviewLateRequestAsync(ReviewLateRequestDto dto, int reviewerId);
        Task<LateRequestResponseDto?> GetLateRequestByIdAsync(int id);
        Task<List<LateRequestResponseDto>> GetUserLateRequestsAsync(int userId, string? status = null);
        Task<List<LateRequestResponseDto>> GetPendingLateRequestsAsync();
        Task<bool> CancelLateRequestAsync(int requestId, int userId);
    }
}
