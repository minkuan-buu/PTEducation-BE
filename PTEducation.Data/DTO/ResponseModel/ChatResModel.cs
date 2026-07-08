using System;

namespace PTEducation.Data.DTO.ResponseModel
{
    public class ChatRoomResModel
    {
        public Guid ChatId { get; set; }
        public string Title { get; set; } = null!;
        public Guid? ClassId { get; set; }
        public string? LastMessage { get; set; }
        public long? LastMessageTime { get; set; }
        public int UnreadCount { get; set; }
    }

    public class ChatMessageResModel
    {
        public Guid Id { get; set; }
        public Guid ChatId { get; set; }
        public string SenderId { get; set; } = null!;
        public string SenderName { get; set; } = null!;
        public string? SenderAvatarUrl { get; set; }
        public string SenderRole { get; set; } = null!;
        public string Content { get; set; } = null!;
        public int MessageType { get; set; }
        public long CreatedAt { get; set; }
    }

    public class ChatContactResModel
    {
        public string UserId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? AvatarUrl { get; set; }
        public string Role { get; set; } = null!;
        public Guid? ChatId { get; set; }
    }
}

