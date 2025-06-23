// File: NotificationHub.cs
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Hospital_Managemant_System
{
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = Context.User?.FindFirst(ClaimTypes.Role)?.Value;

                if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(userRole))
                {
                    // Add user to their specific group
                    var groupName = $"{userRole.ToLower()}_{userId}";
                    await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

                    // Notify the client they've joined
                    await Clients.Caller.SendAsync("JoinedGroup", groupName);

                    _logger.LogInformation($"User {userId} ({userRole}) connected with ConnectionId: {Context.ConnectionId}");
                }
                else
                {
                    _logger.LogWarning($"User connected but missing userId or role. ConnectionId: {Context.ConnectionId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnConnectedAsync");
                await Clients.Caller.SendAsync("Error", "Failed to join notification group");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = Context.User?.FindFirst(ClaimTypes.Role)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                _logger.LogInformation($"User {userId} ({userRole}) disconnected. ConnectionId: {Context.ConnectionId}");
            }

            if (exception != null)
            {
                _logger.LogError(exception, "User disconnected with error");
            }

            await base.OnDisconnectedAsync(exception);
        }

        // Method to join user-specific group (called by client after connection)
        public async Task JoinUserGroup()
        {
            try
            {
                var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = Context.User?.FindFirst(ClaimTypes.Role)?.Value;

                if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(userRole))
                {
                    var groupName = $"{userRole.ToLower()}_{userId}";
                    await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
                    await Clients.Caller.SendAsync("JoinedGroup", groupName);

                    _logger.LogInformation($"User {userId} joined group: {groupName}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining user group");
                await Clients.Caller.SendAsync("Error", "Failed to join user group");
            }
        }

        // Test method for sending messages
        public async Task SendTestMessage(string message)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation($"Test message from user {userId}: {message}");

            await Clients.Caller.SendAsync("TestMessageReceived", $"Echo: {message}");
        }

        // Send notification to specific user
        public async Task SendNotificationToUser(string userId, string userRole, object notification)
        {
            var groupName = $"{userRole.ToLower()}_{userId}";
            await Clients.Group(groupName).SendAsync("ReceiveNotification", notification);
        }
    }
}