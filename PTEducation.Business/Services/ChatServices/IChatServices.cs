using PTEducation.Data.DTO.ResponseModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PTEducation.Business.Services.ChatServices
{
    public interface IChatServices
    {
        Task<ListDataResultModel<ChatRoomResModel>> GetMyChats(string userId, string role);
        Task<PagedListDataResultModel<ChatMessageResModel>> GetChatMessages(Guid chatId, string userId, int pageIndex = 1, int? limit = null);
        Task<DataResultModel<ChatMessageResModel>> SendMessage(Guid chatId, string userId, string content, int messageType = 0);
        Task<MessageResultModel> MarkAsRead(Guid chatId, string userId);
        Task<ListDataResultModel<ChatContactResModel>> GetSupportContacts(string userId);
        Task<DataResultModel<Guid>> GetOrCreatePrivateChat(string userId, string targetUserId);
    }
}
