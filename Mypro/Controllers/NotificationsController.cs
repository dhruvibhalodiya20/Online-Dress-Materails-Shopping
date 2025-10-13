using iText.IO.Image;
using Microsoft.AspNetCore.Mvc;
using Mypro.Models;
using System;
using System.Linq;

namespace Mypro.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _abc;

        public NotificationsController(ApplicationDbContext context)
        {
            _abc = context;
        }

        // ✅ Get unread count for bell badge
        [HttpGet("unread-count")]
        public IActionResult GetUnreadCount()
        {
            var count = _abc.Notifications.Count(n => !n.IsRead);
            return Ok(new { count });
        }

        // ✅ Get recent notifications (top 10)
        [HttpGet("recent")]
        public IActionResult GetRecentNotifications()
        {
            var notifications = _abc.Notifications
                .OrderByDescending(n => n.CreatedDate)
                .Take(10)
                .ToList() 
                .Select(n => new
                {
                    n.NotificationId,
                    n.Title,
                    n.Message,
                    ImagePath = string.IsNullOrEmpty(n.ImagePath)
                    ? "/template/assets/img/noimage.png"
                   : (n.ImagePath.StartsWith("/") || n.ImagePath.StartsWith("http"))
                   ? n.ImagePath
                   : "/" + n.ImagePath.Replace("~", "").TrimStart('/'),


                    n.CreatedDate,
                    n.IsRead,
                    TimeAgo = GetTimeAgo(n.CreatedDate)
                })
                .ToList();

            return Ok(notifications);
        }

        // ✅ Mark one notification as read
        [HttpPost("mark-read/{id}")]
        public IActionResult MarkAsRead(int id)
        {
            var notification = _abc.Notifications.Find(id);
            if (notification != null)
            {
                notification.IsRead = true;
                _abc.SaveChanges();
            }
            return Ok();
        }

        // ✅ Mark all as read
        [HttpPost("mark-all-read")]
        public IActionResult MarkAllAsRead()
        {
            var unread = _abc.Notifications.Where(n => !n.IsRead).ToList();
            foreach (var n in unread)
                n.IsRead = true;

            _abc.SaveChanges();
            return Ok();
        }

        // ✅ Helper method for "time ago" display
        private static string GetTimeAgo(DateTime date)
        {
            var span = DateTime.Now - date;
            if (span.TotalMinutes < 1) return "just now";
            if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes}m ago";
            if (span.TotalHours < 24) return $"{(int)span.TotalHours}h ago";
            if (span.TotalDays < 7) return $"{(int)span.TotalDays}d ago";
            return date.ToString("MMM dd");
        }
    }
}
