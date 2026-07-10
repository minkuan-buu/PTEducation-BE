using PTEducation.Data.Entities;
using PTEducation.Data.Repositories.GenericRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTEducation.Data.Repositories.ChatRepositories
{
    public class ChatRepositories : GenericRepositories<Chat>, IChatRepositories
    {
        public ChatRepositories(PteducationContext context)
        : base(context)
        {
        }
    }

    public class ChatDetailRepositories : GenericRepositories<ChatDetail>, IChatDetailRepositories
    {
        public ChatDetailRepositories(PteducationContext context)
        : base(context)
        {
        }
    }

    public class ChatMessageRepositories : GenericRepositories<ChatMessage>, IChatMessageRepositories
    {
        public ChatMessageRepositories(PteducationContext context)
        : base(context)
        {
        }

        public async Task<int> GetUnreadCount(Guid chatId, Guid? lastReadMessageId)
        {
            if (lastReadMessageId == null)
            {
                return await context.ChatMessages.CountAsync(m => m.ChatId == chatId);
            }

            var lastReadMsg = await context.ChatMessages.FindAsync(lastReadMessageId);
            if (lastReadMsg == null)
            {
                return await context.ChatMessages.CountAsync(m => m.ChatId == chatId);
            }

            return await context.ChatMessages.CountAsync(m => m.ChatId == chatId && m.CreatedAt > lastReadMsg.CreatedAt);
        }

        public async Task<ChatMessage?> GetLastMessage(Guid chatId)
        {
            return await context.ChatMessages
                .Where(m => m.ChatId == chatId)
                .OrderByDescending(m => m.CreatedAt)
                .FirstOrDefaultAsync();
        }
    }
}
