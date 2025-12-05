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

                // Get attendance payroll (đã tính sẵn từ attendance_payroll)
                var attendancePayrolls = await _context.AttendancePayrolls
                    .Where(ap => ap.Userid == user.Id &&
                                ap.Createdat >= startDate &&
                                ap.Createdat < endDate.AddDays(1))
                    .ToListAsync();

                // Tổng lương ca = SUM(attendance_payroll.totalamount)
                decimal totalShiftSalary = attendancePayrolls.Sum(ap => ap.Totalamount ?? 0);
                decimal totalHours = attendancePayrolls.Sum(ap => ap.Hoursworked ?? 0);
                
                // Base salary (lương cứng) - chỉ cho Manager
                decimal baseSalary = user.Basesalary ?? 0;
                
                // OT (nếu có - tính riêng ngoài ca)
                decimal overtimeAmount = attendancePayrolls.Sum(ap => ap.Overtimeamount ?? 0);
                
                var salaryDetails = new List<Salarydetail>();

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
                // Employee: Lương ca + OT + Thưởng - Phạt
                // Manager: Lương cứng + Lương ca + OT + Thưởng - Phạt
                decimal netSalary = baseSalary + totalShiftSalary + overtimeAmount + totalRewards - totalPenalties;

                // Create payroll record
                var payroll = new Payroll
                {
                    Userid = user.Id,
                    Month = dto.Month,
                    Year = dto.Year,
                    Totalhours = totalHours,
                    Basesalary = baseSalary + totalShiftSalary, // Lương cứng + Lương ca
                    Bonuses = totalRewards + overtimeAmount, // Thưởng + OT
                    Penalties = totalPenalties,
                    Netsalary = netSalary,
                    Createdat = DateTime.UtcNow
                };

                _context.Payrolls.Add(payroll);
                await _context.SaveChangesAsync();

                // Create salary details
                if (baseSalary > 0)
                {
                    salaryDetails.Add(new Salarydetail
                    {
                        Payrollid = payroll.Id,
                        Description = "Lương cứng (Base Salary)",
                        Amount = baseSalary,
                        Createdat = DateTime.UtcNow
                    });
                }

                if (totalShiftSalary > 0)
                {
                    salaryDetails.Add(new Salarydetail
                    {
                        Payrollid = payroll.Id,
                        Description = $"Lương ca ({totalHours:F2} giờ)",
                        Amount = totalShiftSalary,
                        Createdat = DateTime.UtcNow
                    });
                }

                if (overtimeAmount > 0)
                {
                    salaryDetails.Add(new Salarydetail
                    {
                        Payrollid = payroll.Id,
                        Description = PayrollConstants.SalaryDetailType.Overtime,
                        Amount = overtimeAmount,
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
