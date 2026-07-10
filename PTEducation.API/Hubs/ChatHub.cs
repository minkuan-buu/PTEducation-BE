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

        public async Task SendTypingIndicator(string chatId, string userName, string? avatarUrl, bool isTyping)
        {
            if (!Guid.TryParse(chatId, out var parsedChatId))
            {
                return;
            }

            // Get userId from claims to broadcast with the event
            var userId = Context.User?.Claims.FirstOrDefault(c => c.Type == "userid")?.Value;

            if (userId != null && !string.IsNullOrEmpty(userName))
            {
                await Clients.GroupExcept(GetChatGroupName(parsedChatId), Context.ConnectionId)
                    .SendAsync("ReceiveTypingIndicator", chatId, userId, userName, avatarUrl, isTyping);
            }
        }
    }
}
