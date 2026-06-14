using PTEducation.Data.Entities;
using PTEducation.Data.Repositories.GenericRepositories;
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
    }
}
