using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ClassroomSchedulerCore.Models
{
    public enum UserRole
    {
        Admin,
        Faculty,
        Student,
        User
    }

    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        public UserRole Role { get; set; }

        // Navigation property
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

        public string FullName => $"{FirstName} {LastName}";
    }
}
