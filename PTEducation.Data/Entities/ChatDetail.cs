using System;
using System.Collections.Generic;

namespace PTEducation.Data.Entities;

public partial class ChatDetail
{
    public Guid Id { get; set; }

    public Guid ChatId { get; set; }

    public string UserId { get; set; } = null!;

    public long JoinedAt { get; set; }

    public Guid? LastReadMessageId { get; set; }

    public virtual Chat Chat { get; set; } = null!;

    public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();

    public virtual ChatMessage? LastReadMessage { get; set; }

    public virtual User User { get; set; } = null!;
}
