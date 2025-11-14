using HRMCyberse.Constants;
using HRMCyberse.Data;
using HRMCyberse.DTOs;
using HRMCyberse.Models;
using Microsoft.EntityFrameworkCore;

namespace HRMCyberse.Services
{
    public class PayrollService : IPayrollService
    {
        private readonly CybersehrmContext _context;

        public PayrollService(CybersehrmContext context)
        {
            _context = context;
        }

        public async Task<List<PayrollResponseDto>> GeneratePayrollAsync(GeneratePayrollDto dto)
        {
            var startDate = new DateTime(dto.Year, dto.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            // Get users to generate payroll for
            var userQuery = _context.Users.Where(u => u.Isactive == true);
            if (dto.UserId.HasValue)
            {
                userQuery = userQuery.Where(u => u.Id == dto.UserId.Value);
            }

            var users = await userQuery.ToListAsync();
            var results = new List<PayrollResponseDto>();

            foreach (var user in users)
            {
                // Check if payroll already exists
                var existingPayroll = await _context.Payrolls
                    .FirstOrDefaultAsync(p => p.Userid == user.Id && p.Month == dto.Month && p.Year == dto.Year);

                if (existingPayroll != null)
                {
                    // Skip or update existing
                    continue;
                }

                // Calculate total hours from attendance
                var attendances = await _context.Attendances
                    .Include(a => a.Shift)
                    .Where(a => a.Userid == user.Id &&
                               a.Checkintime.HasValue &&
                               a.Checkintime.Value >= startDate &&
                               a.Checkintime.Value <= endDate &&
                               a.Checkouttime.HasValue)
                    .ToListAsync();

                decimal totalHours = 0;
                decimal nightShiftBonus = 0;
                var salaryDetails = new List<Salarydetail>();

                foreach (var attendance in attendances)
                {
                    if (attendance.Checkintime.HasValue && attendance.Checkouttime.HasValue)
                    {
                        var hours = (decimal)(attendance.Checkouttime.Value - attendance.Checkintime.Value).TotalHours;
                        totalHours += hours;

                        // Check if night shift (shift name contains "đêm" or "night")
                        if (attendance.Shift?.Name?.ToLower().Contains("đêm") == true ||
                            attendance.Shift?.Name?.ToLower().Contains("night") == true)
                        {
                            nightShiftBonus += PayrollConstants.NightShiftBonus;
                        }
                    }
                }

                // Get base salary (from user's salary rate or default)
                decimal baseSalary = user.Salaryrate ?? 0;
                decimal totalBaseSalary = baseSalary * totalHours;

                // Get rewards and penalties for this month
                var rewardsAndPenalties = await _context.Rewardpenalties
                    .Where(rp => rp.Userid == user.Id &&
                                rp.Createdat.HasValue &&
                                rp.Createdat.Value.Month == dto.Month &&
                                rp.Createdat.Value.Year == dto.Year)
                    .ToListAsync();

                decimal totalRewards = rewardsAndPenalties
                    .Where(rp => rp.Type == PayrollConstants.RewardPenaltyType.Reward)
                    .Sum(rp => rp.Amount);

                decimal totalPenalties = rewardsAndPenalties
                    .Where(rp => rp.Type == PayrollConstants.RewardPenaltyType.Penalty)
                    .Sum(rp => rp.Amount);

                // Calculate net salary
                decimal netSalary = totalBaseSalary + nightShiftBonus + totalRewards - totalPenalties;

                // Create payroll record
                var payroll = new Payroll
                {
                    Userid = user.Id,
                    Month = dto.Month,
                    Year = dto.Year,
                    Totalhours = totalHours,
                    Basesalary = totalBaseSalary,
                    Bonuses = totalRewards + nightShiftBonus,
                    Penalties = totalPenalties,
                    Netsalary = netSalary,
                    Createdat = DateTime.UtcNow
                };

                _context.Payrolls.Add(payroll);
                await _context.SaveChangesAsync();

                // Create salary details
                salaryDetails.Add(new Salarydetail
                {
                    Payrollid = payroll.Id,
                    Description = $"{PayrollConstants.SalaryDetailType.BaseSalary} ({totalHours:F2} hours x {baseSalary:N0} VND)",
                    Amount = totalBaseSalary,
                    Createdat = DateTime.UtcNow
                });

                if (nightShiftBonus > 0)
                {
                    salaryDetails.Add(new Salarydetail
                    {
                        Payrollid = payroll.Id,
                        Description = PayrollConstants.SalaryDetailType.NightShiftBonus,
                        Amount = nightShiftBonus,
                        Createdat = DateTime.UtcNow
                    });
                }

                foreach (var rp in rewardsAndPenalties)
                {
                    salaryDetails.Add(new Salarydetail
                    {
                        Payrollid = payroll.Id,
                        Description = $"{rp.Type}: {rp.Reason}",
                        Amount = rp.Type == PayrollConstants.RewardPenaltyType.Reward ? rp.Amount : -rp.Amount,
                        Createdat = DateTime.UtcNow
                    });
                }

                _context.Salarydetails.AddRange(salaryDetails);
                await _context.SaveChangesAsync();

                var result = await GetPayrollByIdAsync(payroll.Id);
                if (result != null)
                {
                    results.Add(result);
                }
            }

            return results;
        }

        public async Task<PayrollResponseDto?> GetPayrollByIdAsync(int id)
        {
            var payroll = await _context.Payrolls
                .Include(p => p.User)
                .Include(p => p.Salarydetails)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payroll == null) return null;

            return new PayrollResponseDto
            {
                Id = payroll.Id,
                UserId = payroll.Userid,
                UserName = payroll.User?.Fullname ?? "Unknown",
                Month = payroll.Month,
                Year = payroll.Year,
                TotalHours = payroll.Totalhours ?? 0,
                BaseSalary = payroll.Basesalary ?? 0,
                NightShiftBonus = payroll.Salarydetails?
                    .Where(sd => sd.Description.Contains(PayrollConstants.SalaryDetailType.NightShiftBonus))
                    .Sum(sd => sd.Amount) ?? 0,
                Bonuses = payroll.Bonuses ?? 0,
                Penalties = payroll.Penalties ?? 0,
                NetSalary = payroll.Netsalary ?? 0,
                CreatedAt = payroll.Createdat,
                Details = payroll.Salarydetails?.Select(sd => new SalaryDetailDto
                {
                    Id = sd.Id,
                    Description = sd.Description,
                    Amount = sd.Amount,
                    CreatedAt = sd.Createdat
                }).ToList() ?? new List<SalaryDetailDto>()
            };
        }

        public async Task<PayrollResponseDto?> GetUserPayrollAsync(int userId, int month, int year)
        {
            var payroll = await _context.Payrolls
                .Include(p => p.User)
                .Include(p => p.Salarydetails)
                .FirstOrDefaultAsync(p => p.Userid == userId && p.Month == month && p.Year == year);

            if (payroll == null) return null;

            return await GetPayrollByIdAsync(payroll.Id);
        }

        public async Task<List<PayrollResponseDto>> GetUserPayrollHistoryAsync(int userId)
        {
            var payrolls = await _context.Payrolls
                .Include(p => p.User)
                .Include(p => p.Salarydetails)
                .Where(p => p.Userid == userId)
                .OrderByDescending(p => p.Year)
                .ThenByDescending(p => p.Month)
                .ToListAsync();

            return payrolls.Select(p => GetPayrollByIdAsync(p.Id).Result!).ToList();
        }

        public async Task<PayrollSummaryDto> GetPayrollSummaryAsync(int month, int year)
        {
            var payrolls = await _context.Payrolls
                .Where(p => p.Month == month && p.Year == year)
                .ToListAsync();

            return new PayrollSummaryDto
            {
                Month = month,
                Year = year,
                TotalEmployees = payrolls.Count,
                TotalBaseSalary = payrolls.Sum(p => p.Basesalary ?? 0),
                TotalBonuses = payrolls.Sum(p => p.Bonuses ?? 0),
                TotalPenalties = payrolls.Sum(p => p.Penalties ?? 0),
                TotalNetSalary = payrolls.Sum(p => p.Netsalary ?? 0)
            };
        }

        public async Task<PayrollResponseDto> UpdatePayrollAsync(UpdatePayrollDto dto)
        {
            var payroll = await _context.Payrolls.FindAsync(dto.PayrollId);
            if (payroll == null)
            {
                throw new ArgumentException("Payroll not found.");
            }

            if (dto.Bonuses.HasValue)
            {
                payroll.Bonuses = dto.Bonuses.Value;
            }

            if (dto.Penalties.HasValue)
            {
                payroll.Penalties = dto.Penalties.Value;
            }

            // Recalculate net salary
            payroll.Netsalary = (payroll.Basesalary ?? 0) + (payroll.Bonuses ?? 0) - (payroll.Penalties ?? 0);

            _context.Payrolls.Update(payroll);
            await _context.SaveChangesAsync();

            return await GetPayrollByIdAsync(payroll.Id)
                ?? throw new Exception("Failed to retrieve updated payroll.");
        }

        // Reward & Penalty methods
        public async Task<RewardPenaltyResponseDto> CreateRewardPenaltyAsync(CreateRewardPenaltyDto dto, int createdBy)
        {
            var rewardPenalty = new Rewardpenalty
            {
                Userid = dto.UserId,
                Type = dto.Type,
                Amount = dto.Amount,
                Reason = dto.Reason,
                Createdby = createdBy,
                Createdat = DateTime.UtcNow
            };

            _context.Rewardpenalties.Add(rewardPenalty);
            await _context.SaveChangesAsync();

            var result = await _context.Rewardpenalties
                .Include(rp => rp.User)
                .Include(rp => rp.CreatedbyNavigation)
                .FirstOrDefaultAsync(rp => rp.Id == rewardPenalty.Id);

            return new RewardPenaltyResponseDto
            {
                Id = result!.Id,
                UserId = result.Userid,
                UserName = result.User?.Fullname ?? "Unknown",
                Type = result.Type,
                Amount = result.Amount,
                Reason = result.Reason,
                CreatedBy = result.Createdby,
                CreatedByName = result.CreatedbyNavigation?.Fullname,
                CreatedAt = result.Createdat
            };
        }

        public async Task<List<RewardPenaltyResponseDto>> GetUserRewardPenaltiesAsync(int userId, int? month = null, int? year = null)
        {
            var query = _context.Rewardpenalties
                .Include(rp => rp.User)
                .Include(rp => rp.CreatedbyNavigation)
                .Where(rp => rp.Userid == userId);

            if (month.HasValue && year.HasValue)
            {
                query = query.Where(rp => rp.Createdat.HasValue &&
                                         rp.Createdat.Value.Month == month.Value &&
                                         rp.Createdat.Value.Year == year.Value);
            }

            var results = await query.OrderByDescending(rp => rp.Createdat).ToListAsync();

            return results.Select(rp => new RewardPenaltyResponseDto
            {
                Id = rp.Id,
                UserId = rp.Userid,
                UserName = rp.User?.Fullname ?? "Unknown",
                Type = rp.Type,
                Amount = rp.Amount,
                Reason = rp.Reason,
                CreatedBy = rp.Createdby,
                CreatedByName = rp.CreatedbyNavigation?.Fullname,
                CreatedAt = rp.Createdat
            }).ToList();
        }

        public async Task<bool> DeleteRewardPenaltyAsync(int id)
        {
            var rewardPenalty = await _context.Rewardpenalties.FindAsync(id);
            if (rewardPenalty == null) return false;

            _context.Rewardpenalties.Remove(rewardPenalty);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
