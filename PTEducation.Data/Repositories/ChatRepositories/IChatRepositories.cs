using PTEducation.Data.Entities;
using PTEducation.Data.Repositories.GenericRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTEducation.Data.Repositories.ChatRepositories
{
    public interface IChatRepositories : IGenericRepositories<Chat>
    {
    }

    public interface IChatDetailRepositories : IGenericRepositories<ChatDetail>
    {
    }

    public interface IChatMessageRepositories : IGenericRepositories<ChatMessage>
    {
    }
}
