using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClassroomSchedulerCore.Models
{
    public enum BookingStatus
    {
        Available,
        Reserved,
        Emergency
    }

    public class Booking
    {
        public int Id { get; set; }

        [Required]
        public int RoomId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Start Time")]
        public DateTime StartTime { get; set; }

        [Required]
        [Display(Name = "End Time")]
        public DateTime EndTime { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Event Title")]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public BookingStatus Status { get; set; }
        
        [Display(Name = "Emergency Booking")]
        public bool IsEmergency { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime? UpdatedAt { get; set; }

        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;
        
        // Navigation properties
        [ForeignKey("RoomId")]
        public virtual Room? Room { get; set; }
        
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
    }
}
