using PTEducation.Data.DTO.ResponseModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PTEducation.Business.Services.ChatServices
{
    public interface IChatServices
    {
        Task<ListDataResultModel<ChatRoomResModel>> GetMyChats(string userId, string role);
        Task<ListDataResultModel<ChatMessageResModel>> GetChatMessages(Guid chatId, string userId, int? limit);
        Task<DataResultModel<ChatMessageResModel>> SendMessage(Guid chatId, string userId, string content, int messageType = 0);
        Task<MessageResultModel> MarkAsRead(Guid chatId, string userId);
    }
}
