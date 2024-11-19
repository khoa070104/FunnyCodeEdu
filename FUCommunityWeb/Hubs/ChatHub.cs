using FuCommunityWebModels.Models;
using FuCommunityWebServices.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

public class ChatHub : Hub
{
    private readonly ChatService _chatService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ChatHub(ChatService chatService, UserManager<ApplicationUser> userManager)
    {
        _chatService = chatService;
        _userManager = userManager;
    }

    public async Task SendMessage(string receiverId, string message)
    {
        var sender = await _userManager.GetUserAsync(Context.User);
        
        // Lưu tin nhắn và tạo thông báo qua service
        var savedMessage = await _chatService.SendMessage(sender.Id, receiverId, message);

        // Gửi tin nhắn realtime đến người nhận
        await Clients.User(receiverId).SendAsync("ReceiveMessage", new
        {
            messageId = savedMessage.MessageId,
            senderId = sender.Id,
            senderName = sender.FullName,
            senderAvatar = sender.AvatarImage,
            content = message,
            createdDate = savedMessage.CreatedDate
        });

        // Gửi thông báo
        await Clients.User(receiverId).SendAsync("ReceiveNotification", 
            $"Bạn có tin nhắn mới từ {sender.FullName}");
    }

    public async Task MarkMessageAsRead(string senderId)
    {
        var receiverId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(receiverId))
        {
            await _chatService.MarkMessagesAsRead(senderId, receiverId);
        }
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
        }
        await base.OnDisconnectedAsync(exception);
    }
} 