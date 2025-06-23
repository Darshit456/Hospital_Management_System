// File: Models/Notification.cs - Updated to match your existing table
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital_Management_System.Models
{
    public class Notification
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid(); // Your existing uniqueidentifier primary key

        [Required]
        public int UserID { get; set; } // Your existing UserID foreign key

        [Required]
        [StringLength(500)]
        public string Message { get; set; } = string.Empty; // Your existing Message field

        public bool IsRead { get; set; } = false; // Your existing IsRead field

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Your existing CreatedAt field

        // Navigation property to User
        [ForeignKey("UserID")]
        public virtual User? User { get; set; }

        // Additional properties for enhanced functionality (we'll store as JSON in Message if needed)
        [NotMapped]
        public string NotificationType { get; set; } = "general"; // Not stored in DB, computed from Message

        [NotMapped]
        public string Title { get; set; } = string.Empty; // Not stored in DB, computed from Message

        [NotMapped]
        public string Icon { get; set; } = "📢"; // Not stored in DB, computed from Message

        [NotMapped]
        public string Priority { get; set; } = "normal"; // Not stored in DB, computed from Message

        [NotMapped]
        public string? AppointmentToken { get; set; } // Not stored in DB, computed from Message

        [NotMapped]
        public DateTime? ReadAt { get; set; } // We can add this later if needed
    }
}