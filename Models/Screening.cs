using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaTicketSystem.Models
{
    public class Screening
    {
        [Key]
        public int ScreeningId { get; set; }

        [Required(ErrorMessage = "Film title is required")]
        [StringLength(200)]
        public string FilmTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Screening date and time is required")]
        [DataType(DataType.DateTime)]
        public DateTime ScreeningDateTime { get; set; }

        // Foreign key
        [Required]
        public int CinemaId { get; set; }

        // Navigation properties
        [ForeignKey("CinemaId")]
        public virtual Cinema Cinema { get; set; } = null!;

        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
