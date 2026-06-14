using System;
using System.Collections.Generic;

namespace PTEducation.Data.Entities;

public partial class ChatMessage
{
    public Guid Id { get; set; }

    public Guid ChatId { get; set; }

    public Guid SenderUserId { get; set; }

    public string Content { get; set; } = null!;

    public int MessageType { get; set; }

    public long CreatedAt { get; set; }

    public virtual Chat Chat { get; set; } = null!;

    public virtual ICollection<ChatDetail> ChatDetails { get; set; } = new List<ChatDetail>();

    public virtual ChatDetail SenderUser { get; set; } = null!;
}
