// File: DTOs/NotificationDTOs.cs
using System.ComponentModel.DataAnnotations;

namespace Hospital_Management_System.DTOs
{
    public class NotificationCreateDTO
    {
        [Required]
        public int UserID { get; set; }

        [Required]
        [StringLength(500)]
        public string Message { get; set; } = string.Empty;

        public string NotificationType { get; set; } = "general";
        public string Title { get; set; } = "Notification";
        public string Icon { get; set; } = "📢";
        public string Priority { get; set; } = "normal";
    }

    public class NotificationResponseDTO
    {
        public Guid Id { get; set; }
        public int UserID { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public string NotificationType { get; set; } = "general";
        public string Title { get; set; } = string.Empty;
        public string Icon { get; set; } = "📢";
        public string Priority { get; set; } = "normal";
        public string? AppointmentToken { get; set; }
        public string TimeAgo { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
    }

    public class NotificationFilterDTO
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public bool? IsRead { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class NotificationMarkReadDTO
    {
        [Required]
        public Guid Id { get; set; }
    }

    public class TestNotificationDTO
    {
        [Required]
        public int UserID { get; set; }

        [Required]
        public string Message { get; set; } = "Test notification";
    }

    public class StructuredNotificationData
    {
        public string Type { get; set; } = "general";
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Icon { get; set; } = "📢";
        public string Priority { get; set; } = "normal";
        public string? AppointmentToken { get; set; }
        public Dictionary<string, object> AdditionalData { get; set; } = new();
    }
}