namespace FuCommunityWebModels.Models
{
    public class ChatListItem
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string AvatarImage { get; set; }
        public string LastMessage { get; set; }
        public int UnreadCount { get; set; }
    }
} 