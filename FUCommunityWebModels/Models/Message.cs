using System;
using FuCommunityWebModels.Models;

public class Message
{
    public int MessageId { get; set; }
    public string SenderId { get; set; }
    public string ReceiverId { get; set; }
    public string Content { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool IsRead { get; set; }

    public virtual ApplicationUser Sender { get; set; }
    public virtual ApplicationUser Receiver { get; set; }
} 