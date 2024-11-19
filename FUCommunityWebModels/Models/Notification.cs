using System;
using FuCommunityWebModels.Models;

public class Notification
{
    public int NotificationID { get; set; }
    
    public string UserID { get; set; } // Người nhận thông báo
    public ApplicationUser User { get; set; }
    
    public string FromUserID { get; set; } // Người tạo thông báo
    public ApplicationUser FromUser { get; set; }
    
    public int? PostID { get; set; }
    public Post Post { get; set; }
    
    public string Message { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public bool IsRead { get; set; } = false;
    public string NotificationType { get; set; } // Loại thông báo (comment, like, etc)
    public string Content { get; set; }
    public string Type { get; set; }
} 