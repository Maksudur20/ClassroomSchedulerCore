using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClassroomSchedulerCore.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        
        [Required]
        public required string UserId { get; set; }
        
        [StringLength(256)]
        public required string UserName { get; set; }
        
        [Required]
        [StringLength(50)]
        public required string Action { get; set; }
        
        [Required]
        [StringLength(100)]
        public required string EntityName { get; set; }
        
        public int? EntityId { get; set; }
        
        [Required]
        public DateTime Timestamp { get; set; } = DateTime.Now;
        
        [StringLength(500)]
        public string? Details { get; set; }
        
        // Navigation property
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
    }
}
