using HRMCyberse.DTOs;

namespace HRMCyberse.Services
{
    public interface IPayrollService
    {
        // Payroll Generation
        Task<List<PayrollResponseDto>> GeneratePayrollAsync(GeneratePayrollDto dto);
        Task<PayrollResponseDto?> GetPayrollByIdAsync(int id);
        Task<PayrollResponseDto?> GetUserPayrollAsync(int userId, int month, int year);
        Task<List<PayrollResponseDto>> GetUserPayrollHistoryAsync(int userId);
        Task<PayrollSummaryDto> GetPayrollSummaryAsync(int month, int year);
        Task<PayrollResponseDto> UpdatePayrollAsync(UpdatePayrollDto dto);

        // Reward & Penalty
        Task<RewardPenaltyResponseDto> CreateRewardPenaltyAsync(CreateRewardPenaltyDto dto, int createdBy);
        Task<List<RewardPenaltyResponseDto>> GetUserRewardPenaltiesAsync(int userId, int? month = null, int? year = null);
        Task<bool> DeleteRewardPenaltyAsync(int id);
    }
}
