// File: Controllers/NotificationController.cs - Updated for your existing table structure
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Hospital_Management_System.Models;
using Hospital_Management_System.DTOs;
using System.Security.Claims;
using Hospital_Managemant_System.Data;
using System.Text.Json;

namespace Hospital_Management_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly HospitalDbContext _context;
        private readonly ILogger<NotificationController> _logger;
        private readonly IHubContext<Hospital_Managemant_System.NotificationHub> _hubContext;

        public NotificationController(
            HospitalDbContext context,
            ILogger<NotificationController> logger,
            IHubContext<Hospital_Managemant_System.NotificationHub> hubContext)
        {
            _context = context;
            _logger = logger;
            _hubContext = hubContext;
        }

        // GET: api/Notification/my-notifications
        [HttpGet("my-notifications")]
        public async Task<ActionResult<IEnumerable<NotificationResponseDTO>>> GetMyNotifications([FromQuery] NotificationFilterDTO filter)
        {
            try
            {
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userRole) || string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized("User information not found");
                }

                int userId = int.Parse(userIdClaim);
                var query = _context.Notifications.Include(n => n.User).AsQueryable();

                // Filter by current user
                query = query.Where(n => n.UserID == userId);

                // Apply additional filters
                if (filter.IsRead.HasValue)
                {
                    query = query.Where(n => n.IsRead == filter.IsRead.Value);
                }

                if (filter.FromDate.HasValue)
                {
                    query = query.Where(n => n.CreatedAt >= filter.FromDate.Value);
                }

                if (filter.ToDate.HasValue)
                {
                    query = query.Where(n => n.CreatedAt <= filter.ToDate.Value);
                }

                // Order by creation date (newest first)
                query = query.OrderByDescending(n => n.CreatedAt);

                // Pagination
                var totalCount = await query.CountAsync();
                var notifications = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                var response = notifications.Select(n => {
                    var structuredData = ParseNotificationMessage(n.Message);
                    return new NotificationResponseDTO
                    {
                        Id = n.Id,
                        UserID = n.UserID,
                        Message = structuredData.Message,
                        IsRead = n.IsRead,
                        CreatedAt = n.CreatedAt,
                        NotificationType = structuredData.Type,
                        Title = structuredData.Title,
                        Icon = structuredData.Icon,
                        Priority = structuredData.Priority,
                        AppointmentToken = structuredData.AppointmentToken,
                        TimeAgo = GetTimeAgo(n.CreatedAt),
                        UserRole = n.User?.Role ?? userRole
                    };
                }).ToList();

                Response.Headers.Add("X-Total-Count", totalCount.ToString());
                Response.Headers.Add("X-Page", filter.Page.ToString());
                Response.Headers.Add("X-Page-Size", filter.PageSize.ToString());

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notifications");
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: api/Notification/send-appointment-notification
        [HttpPost("send-appointment-notification")]
        public async Task<IActionResult> SendAppointmentNotification([FromBody] int appointmentId)
        {
            try
            {
                var appointment = await _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .FirstOrDefaultAsync(a => a.AppointmentID == appointmentId);

                if (appointment == null)
                {
                    return NotFound("Appointment not found");
                }

                // Get doctor's UserID
                var doctor = await _context.Doctors
                    .Include(d => d.User)
                    .FirstOrDefaultAsync(d => d.DoctorID == appointment.DoctorID);

                if (doctor?.User == null)
                {
                    return NotFound("Doctor user not found");
                }

                // Create structured notification data
                var notificationData = new StructuredNotificationData
                {
                    Type = "new_appointment",
                    Title = "New Appointment Request",
                    Message = $"{appointment.Patient.FirstName} {appointment.Patient.LastName} requested an appointment for {appointment.AppointmentDateTime:MMM dd, yyyy} at {appointment.AppointmentDateTime:h:mm tt}",
                    Icon = "📅",
                    Priority = "high",
                    AppointmentToken = appointment.AppointmentID.ToString(),
                    AdditionalData = new Dictionary<string, object>
                    {
                        ["PatientName"] = $"{appointment.Patient.FirstName} {appointment.Patient.LastName}",
                        ["PatientPhone"] = appointment.Patient.Phone,
                        ["PatientEmail"] = appointment.Patient.Email,
                        ["AppointmentDateTime"] = appointment.AppointmentDateTime,
                        ["Reason"] = appointment.Reason ?? ""
                    }
                };

                // Create notification in your existing table structure
                var notification = new Notification
                {
                    Id = Guid.NewGuid(),
                    UserID = doctor.UserID,
                    Message = JsonSerializer.Serialize(notificationData),
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                // Send real-time notification via SignalR
                await SendRealTimeNotification(notification, notificationData);

                _logger.LogInformation($"Appointment notification sent to Doctor UserID {doctor.UserID} for appointment {appointmentId}");

                return Ok(new { message = "Appointment notification sent to doctor" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending appointment notification");
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: api/Notification/send-status-change
        [HttpPost("send-status-change")]
        public async Task<IActionResult> SendStatusChangeNotification([FromBody] dynamic request)
        {
            try
            {
                int appointmentId = request.appointmentId;
                string newStatus = request.newStatus;
                string oldStatus = request.oldStatus;

                var appointment = await _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .FirstOrDefaultAsync(a => a.AppointmentID == appointmentId);

                if (appointment == null)
                {
                    return NotFound("Appointment not found");
                }

                // Get patient's UserID
                var patient = await _context.Patients
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.PatientID == appointment.PatientID);

                if (patient?.User == null)
                {
                    return NotFound("Patient user not found");
                }

                // Determine notification details based on status
                string title = newStatus.ToLower() switch
                {
                    "accepted" => "Appointment Accepted ✅",
                    "rejected" => "Appointment Rejected ❌",
                    "completed" => "Appointment Completed ✔️",
                    "cancelled" => "Appointment Cancelled ❌",
                    _ => "Appointment Status Updated"
                };

                string icon = newStatus.ToLower() switch
                {
                    "accepted" => "✅",
                    "rejected" => "❌",
                    "completed" => "✔️",
                    "cancelled" => "❌",
                    _ => "📋"
                };

                string priority = newStatus.ToLower() switch
                {
                    "rejected" => "high",
                    "accepted" => "high",
                    "cancelled" => "high",
                    _ => "normal"
                };

                string message = newStatus.ToLower() switch
                {
                    "accepted" => $"Great! Your appointment for {appointment.AppointmentDateTime:MMM dd, yyyy} at {appointment.AppointmentDateTime:h:mm tt} has been accepted by Dr. {appointment.Doctor.FirstName} {appointment.Doctor.LastName}.",
                    "rejected" => $"Unfortunately, your appointment for {appointment.AppointmentDateTime:MMM dd, yyyy} at {appointment.AppointmentDateTime:h:mm tt} has been rejected. Please book another time slot.",
                    "completed" => $"Your appointment on {appointment.AppointmentDateTime:MMM dd, yyyy} at {appointment.AppointmentDateTime:h:mm tt} has been completed. Thank you for visiting!",
                    "cancelled" => $"Your appointment for {appointment.AppointmentDateTime:MMM dd, yyyy} at {appointment.AppointmentDateTime:h:mm tt} has been cancelled.",
                    _ => $"Your appointment status has been updated to {newStatus}"
                };

                // Create structured notification data
                var notificationData = new StructuredNotificationData
                {
                    Type = "appointment_status_change",
                    Title = title,
                    Message = message,
                    Icon = icon,
                    Priority = priority,
                    AppointmentToken = appointment.AppointmentID.ToString(),
                    AdditionalData = new Dictionary<string, object>
                    {
                        ["OldStatus"] = oldStatus,
                        ["NewStatus"] = newStatus,
                        ["DoctorName"] = $"Dr. {appointment.Doctor.FirstName} {appointment.Doctor.LastName}",
                        ["AppointmentDateTime"] = appointment.AppointmentDateTime
                    }
                };

                // Create notification in your existing table structure
                var notification = new Notification
                {
                    Id = Guid.NewGuid(),
                    UserID = patient.UserID,
                    Message = JsonSerializer.Serialize(notificationData),
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                // Send real-time notification
                await SendRealTimeNotification(notification, notificationData);

                _logger.LogInformation($"Status change notification sent to Patient UserID {patient.UserID} for appointment {appointmentId}");

                return Ok(new { message = "Status change notification sent to patient" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending status change notification");
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: api/Notification/mark-read
        [HttpPost("mark-read")]
        public async Task<IActionResult> MarkAsRead([FromBody] NotificationMarkReadDTO request)
        {
            try
            {
                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.Id == request.Id);

                if (notification == null)
                {
                    return NotFound("Notification not found");
                }

                // Verify ownership
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                if (notification.UserID != userId)
                {
                    return Forbid("You can only mark your own notifications as read");
                }

                if (!notification.IsRead)
                {
                    notification.IsRead = true;
                    await _context.SaveChangesAsync();
                }

                return Ok(new { message = "Notification marked as read" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read");
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: api/Notification/mark-all-read
        [HttpPost("mark-all-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                var notifications = await _context.Notifications
                    .Where(n => n.UserID == userId && !n.IsRead)
                    .ToListAsync();

                foreach (var notification in notifications)
                {
                    notification.IsRead = true;
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = $"Marked {notifications.Count} notifications as read" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read");
                return StatusCode(500, "Internal server error");
            }
        }

        // DELETE: api/Notification/clear-all
        [HttpDelete("clear-all")]
        public async Task<IActionResult> ClearAllNotifications()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                var notifications = await _context.Notifications
                    .Where(n => n.UserID == userId)
                    .ToListAsync();

                _context.Notifications.RemoveRange(notifications);
                await _context.SaveChangesAsync();

                return Ok(new { message = $"Cleared {notifications.Count} notifications" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing notifications");
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: api/Notification/test
        [HttpPost("test")]
        public async Task<IActionResult> SendTestNotification([FromBody] TestNotificationDTO request)
        {
            try
            {
                var testData = new StructuredNotificationData
                {
                    Type = "test",
                    Title = "Test Notification 🧪",
                    Message = request.Message,
                    Icon = "🧪",
                    Priority = "normal"
                };

                var notification = new Notification
                {
                    Id = Guid.NewGuid(),
                    UserID = request.UserID,
                    Message = JsonSerializer.Serialize(testData),
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                // Send real-time notification
                await SendRealTimeNotification(notification, testData);

                return Ok(new { message = "Test notification sent successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending test notification");
                return StatusCode(500, "Internal server error");
            }
        }

        // Private method to send real-time notifications via SignalR
        private async Task SendRealTimeNotification(Notification notification, StructuredNotificationData data)
        {
            try
            {
                var user = await _context.Users.FindAsync(notification.UserID);
                if (user == null) return;

                var signalRNotification = new
                {
                    Id = notification.Id.ToString(),
                    Type = data.Type,
                    Title = data.Title,
                    Message = data.Message,
                    Icon = data.Icon,
                    Priority = data.Priority,
                    Time = notification.CreatedAt,
                    Unread = !notification.IsRead,
                    Data = new
                    {
                        AppointmentToken = data.AppointmentToken,
                        AdditionalData = data.AdditionalData
                    }
                };

                // Send to user's SignalR group
                var groupName = $"{user.Role.ToLower()}_{notification.UserID}";
                await _hubContext.Clients.Group(groupName)
                    .SendAsync("ReceiveNotification", signalRNotification);

                _logger.LogInformation($"Real-time notification sent to {groupName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending real-time notification: {notification.Id}");
            }
        }

        // Helper method to parse notification message
        private StructuredNotificationData ParseNotificationMessage(string message)
        {
            try
            {
                var data = JsonSerializer.Deserialize<StructuredNotificationData>(message);
                return data ?? new StructuredNotificationData { Message = message };
            }
            catch
            {
                // If not JSON, treat as plain message
                return new StructuredNotificationData { Message = message };
            }
        }

        // Helper method to calculate time ago
        private string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime;

            if (timeSpan.TotalMinutes < 1)
                return "Just now";
            else if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} minutes ago";
            else if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} hours ago";
            else if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays} days ago";
            else
                return dateTime.ToString("MMM dd, yyyy");
        }
    }
}