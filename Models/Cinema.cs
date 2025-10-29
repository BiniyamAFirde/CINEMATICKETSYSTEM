using System.ComponentModel.DataAnnotations;

namespace CinemaTicketSystem.Models
{
    public class Cinema
    {
        [Key]
        public int CinemaId { get; set; }

        [Required(ErrorMessage = "Cinema name is required")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(1, 50, ErrorMessage = "Rows must be between 1 and 50")]
        public int Rows { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Seats per row must be between 1 and 100")]
        public int SeatsPerRow { get; set; }

        // Calculated property
        public int TotalSeats => Rows * SeatsPerRow;

        // Navigation property
        public virtual ICollection<Screening> Screenings { get; set; } = new List<Screening>();
    }
}
