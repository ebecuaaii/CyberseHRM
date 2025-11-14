using System.ComponentModel.DataAnnotations;

namespace HRMCyberse.DTOs
{
    // Request DTOs
    public class GeneratePayrollDto
    {
        [Required]
        public int Month { get; set; }

        [Required]
        public int Year { get; set; }

        public int? UserId { get; set; } // Null = generate for all users
    }

    public class UpdatePayrollDto
    {
        [Required]
        public int PayrollId { get; set; }

        public decimal? Bonuses { get; set; }
        public decimal? Penalties { get; set; }
        public string? Notes { get; set; }
    }

    // Response DTOs
    public class PayrollResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal TotalHours { get; set; }
        public decimal BaseSalary { get; set; }
        public decimal NightShiftBonus { get; set; }
        public decimal Bonuses { get; set; }
        public decimal Penalties { get; set; }
        public decimal NetSalary { get; set; }
        public DateTime? CreatedAt { get; set; }
        public List<SalaryDetailDto> Details { get; set; } = new List<SalaryDetailDto>();
    }

    public class SalaryDetailDto
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class PayrollSummaryDto
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public int TotalEmployees { get; set; }
        public decimal TotalBaseSalary { get; set; }
        public decimal TotalBonuses { get; set; }
        public decimal TotalPenalties { get; set; }
        public decimal TotalNetSalary { get; set; }
    }
}
