using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaTicketSystem.Models
{
    public class Reservation
    {
        [Key]
        public int ReservationId { get; set; }

        // Foreign keys
        [Required]
        public int ScreeningId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [Range(1, 50)]
        public int Row { get; set; }

        [Required]
        [Range(1, 100)]
        public int SeatNumber { get; set; }

        [Required]
        public DateTime ReservationTime { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("ScreeningId")]
        public virtual Screening Screening { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}
