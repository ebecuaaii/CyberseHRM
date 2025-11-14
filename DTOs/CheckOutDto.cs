using System.ComponentModel.DataAnnotations;

namespace HRMCyberse.DTOs
{
    public class CheckOutDto
    {
        [Required]
        public int AttendanceId { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        public string? ImageUrl { get; set; }

        public string? Notes { get; set; }
    }
}