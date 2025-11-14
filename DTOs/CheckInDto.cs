using System.ComponentModel.DataAnnotations;

namespace HRMCyberse.DTOs
{
    public class CheckInDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int ShiftId { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        public string? ImageUrl { get; set; }

        public string? Notes { get; set; }
    }
}