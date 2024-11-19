using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FuCommunityWebServices.Services;
using Microsoft.Extensions.Logging;

namespace FUCommunityWeb.Controllers
{
    public class NotificationController : Controller
    {
        private readonly NotificationService _notificationService;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(
            NotificationService notificationService,
            ILogger<NotificationController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadNotifications()
        {
            try 
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new List<object>());
                }

                var notifications = await _notificationService.GetUnreadNotifications(userId);
                var notificationData = notifications.Select(n => new
                {
                    notificationID = n.NotificationID,
                    message = n.Message,
                    createdDate = n.CreatedDate,
                    postID = n.PostID,
                    fromUser = new
                    {
                        fullName = n.FromUser.FullName,
                        avatarImage = n.FromUser.AvatarImage
                    }
                });

                return Json(notificationData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread notifications");
                return Json(new List<object>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            try
            {
                await _notificationService.MarkAsRead(notificationId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read");
                return BadRequest();
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(0);
                }
                
                var count = await _notificationService.GetUnreadCount(userId);
                return Json(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread notification count");
                return Json(0);
            }
        }
    }
} 