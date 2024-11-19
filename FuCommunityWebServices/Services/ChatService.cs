using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FuCommunityWebServices.Services;
using Microsoft.AspNetCore.Identity;
using FuCommunityWebModels.Models;
using FuCommunityWebDataAccess.Repositories;

namespace FuCommunityWebServices.Services
{
    public class ChatService
    {
        private readonly MessageRepository _messageRepository;
        private readonly NotificationService _notificationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatService(
            MessageRepository messageRepository,
            NotificationService notificationService,
            UserManager<ApplicationUser> userManager)
        {
            _messageRepository = messageRepository;
            _notificationService = notificationService;
            _userManager = userManager;
        }

        public async Task<IEnumerable<Message>> GetMessagesBetweenUsers(string userId1, string userId2)
        {
            var messages = await _messageRepository.GetMessagesBetweenUsers(userId1, userId2);
            await _messageRepository.MarkMessagesAsRead(userId2, userId1);
            return messages;
        }

        public async Task<IEnumerable<ChatListItem>> GetChatList(string userId)
        {
            return await _messageRepository.GetChatListItems(userId);
        }

        public async Task<int> GetUnreadMessageCount(string userId)
        {
            return await _messageRepository.GetUnreadMessageCount(userId);
        }

        public async Task<Message> SendMessage(string senderId, string receiverId, string content)
        {
            var message = new Message
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = content,
                CreatedDate = DateTime.Now,
                IsRead = false
            };

            var savedMessage = await _messageRepository.CreateMessage(message);
            var sender = await _userManager.FindByIdAsync(senderId);

            var notification = new Notification
            {
                UserID = receiverId,
                FromUserID = senderId,
                Content = content,
                Message = $"{sender.FullName} đã gửi cho bạn một tin nhắn",
                NotificationType = "Message",
                Type = "Message",
                CreatedDate = DateTime.Now,
                IsRead = false
            };

            await _notificationService.CreateNotification(notification);

            return savedMessage;
        }

        public async Task MarkMessagesAsRead(string senderId, string receiverId)
        {
            await _messageRepository.MarkMessagesAsRead(senderId, receiverId);
        }
    }
} 