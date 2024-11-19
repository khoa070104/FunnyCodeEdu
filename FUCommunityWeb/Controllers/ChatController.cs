using FuCommunityWebModels.Models;
using FuCommunityWebServices.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;

namespace FUCommunityWeb.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly ChatService _chatService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatController(ChatService chatService, UserManager<ApplicationUser> userManager)
        {
            _chatService = chatService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string userId = null)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var chatList = (await _chatService.GetChatList(currentUserId)).ToList();

            if (!string.IsNullOrEmpty(userId) && !chatList.Any(c => c.UserId == userId))
            {
                var newChatUser = await _userManager.FindByIdAsync(userId);
                if (newChatUser != null)
                {
                    chatList.Insert(0, new ChatListItem
                    {
                        UserId = newChatUser.Id,
                        FullName = newChatUser.FullName,
                        AvatarImage = newChatUser.AvatarImage,
                        LastMessage = "",
                        UnreadCount = 0
                    });
                }
            }

            ViewBag.ChatList = chatList;
            ViewBag.InitialChatUserId = userId;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetMessages(string userId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var messages = await _chatService.GetMessagesBetweenUsers(currentUserId, userId);
            return Json(messages);
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var unreadCount = await _chatService.GetUnreadMessageCount(currentUserId);
            return Json(unreadCount);
        }

        [HttpGet]
        public async Task<IActionResult> GetChatList()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var chatList = await _chatService.GetChatList(currentUserId);
            return Json(chatList);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var message = await _chatService.SendMessage(currentUserId, model.ReceiverId, model.Content);
            return Json(message);
        }
    }

    public class SendMessageModel
    {
        public string ReceiverId { get; set; }
        public string Content { get; set; }
    }
} 