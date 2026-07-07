using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace PTEducation.API.Hubs
{
    [Authorize(AuthenticationSchemes = "PTEducationAuthentication")]
    public class ChatHub : Hub
    {
        public static string GetChatGroupName(Guid chatId) => $"chat:{chatId:D}";

        public async Task JoinChatRoom(string chatId)
        {
            if (!Guid.TryParse(chatId, out var parsedChatId))
            {
                throw new HubException("Invalid chat room id.");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, GetChatGroupName(parsedChatId));
        }

        public async Task LeaveChatRoom(string chatId)
        {
            if (!Guid.TryParse(chatId, out var parsedChatId))
            {
                throw new HubException("Invalid chat room id.");
            }

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetChatGroupName(parsedChatId));
        }
    }
}
