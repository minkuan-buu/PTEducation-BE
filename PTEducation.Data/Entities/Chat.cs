using System;
using System.Collections.Generic;

namespace PTEducation.Data.Entities;

public partial class Chat
{
    public Guid Id { get; set; }

    public Guid? ClassId { get; set; }

    public string? Title { get; set; }

    public long CreatedAt { get; set; }

    public virtual ICollection<ChatDetail> ChatDetails { get; set; } = new List<ChatDetail>();

    public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();

    public virtual Class? Class { get; set; }
}
