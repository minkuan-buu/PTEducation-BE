using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PTEducation.API.Hubs;
using PTEducation.Data.DTO.Custom;
using PTEducation.Business.Services.ChatServices;
using PTEducation.Data.DTO.ResponseModel;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PTEducation.API.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/chats")]
    [ApiVersion("2.0")]
    [Authorize(AuthenticationSchemes = "PTEducationAuthentication")]
    public class ChatController : ControllerBase
    {
        private readonly IChatServices _chatServices;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(IChatServices chatServices, IHubContext<ChatHub> hubContext)
        {
            _chatServices = chatServices;
            _hubContext = hubContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyChats()
        {
            try
            {
                var userId = User.FindFirst("userid")?.Value;
                var role = User.FindFirst(ClaimTypes.Role)?.Value;

                if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(role))
                {
                    return Unauthorized(new { message = "User is not authenticated." });
                }

                var result = await _chatServices.GetMyChats(userId, role);
                return Ok(result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{chatId}/messages")]
        public async Task<IActionResult> GetChatMessages(Guid chatId, [FromQuery] int? limit)
        {
            try
            {
                var userId = User.FindFirst("userid")?.Value;
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return Unauthorized(new { message = "User is not authenticated." });
                }

                var result = await _chatServices.GetChatMessages(chatId, userId, limit);
                return Ok(result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{chatId}/messages")]
        public async Task<IActionResult> SendMessage(Guid chatId, [FromBody] SendMessageReqModel req)
        {
            try
            {
                var userId = User.FindFirst("userid")?.Value;
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return Unauthorized(new { message = "User is not authenticated." });
                }

                var result = await _chatServices.SendMessage(chatId, userId, req.Content, req.MessageType);

                // Broadcast to the SignalR group in real-time
                if (result.Data != null)
                {
                    await _hubContext.Clients.Group(ChatHub.GetChatGroupName(chatId))
                        .SendAsync("ReceiveMessage", result.Data);
                }

                return Ok(result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{chatId}/read")]
        public async Task<IActionResult> MarkAsRead(Guid chatId)
        {
            try
            {
                var userId = User.FindFirst("userid")?.Value;
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return Unauthorized(new { message = "User is not authenticated." });
                }

                var result = await _chatServices.MarkAsRead(chatId, userId);
                return Ok(result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("contacts")]
        public async Task<IActionResult> GetSupportContacts()
        {
            try
            {
                var userId = User.FindFirst("userid")?.Value;
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return Unauthorized(new { message = "User is not authenticated." });
                }

                var result = await _chatServices.GetSupportContacts(userId);
                return Ok(result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("private")]
        public async Task<IActionResult> GetOrCreatePrivateChat([FromBody] CreatePrivateChatReqModel req)
        {
            try
            {
                var userId = User.FindFirst("userid")?.Value;
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return Unauthorized(new { message = "User is not authenticated." });
                }

                var result = await _chatServices.GetOrCreatePrivateChat(userId, req.TargetUserId);
                return Ok(result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public class SendMessageReqModel
    {
        public string Content { get; set; } = null!;
        public int MessageType { get; set; } = 0;
    }

    public class CreatePrivateChatReqModel
    {
        public string TargetUserId { get; set; } = null!;
    }
}
