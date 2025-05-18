using System.ComponentModel.DataAnnotations;

namespace ClassroomSchedulerCore.Models
{
    public class Room
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public required string Name { get; set; }

        [Required]
        [StringLength(100)]
        public required string Location { get; set; }

        [Range(1, 500)]
        public int Capacity { get; set; }
        
        public bool HasProjector { get; set; }
        
        public bool HasComputers { get; set; }

        [StringLength(500)]
        public required string Description { get; set; }

        // Navigation property for bookings
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
