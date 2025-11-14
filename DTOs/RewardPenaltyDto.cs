using System.ComponentModel.DataAnnotations;

namespace HRMCyberse.DTOs
{
    // Request DTOs
    public class CreateRewardPenaltyDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public string Type { get; set; } = string.Empty; // Reward, Penalty

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public string Reason { get; set; } = string.Empty;
    }

    // Response DTOs
    public class RewardPenaltyResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Reason { get; set; }
        public int? CreatedBy { get; set; }
        public string? CreatedByName { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
