using PTEducation.Data.DTO.Custom;
using PTEducation.Data.DTO.ResponseModel;
using PTEducation.Data.Entities;
using PTEducation.Data.Enums;
using PTEducation.Data.Repositories.ChatRepositories;
using PTEducation.Data.Repositories.ClassRepositories;
using PTEducation.Data.Repositories.StudentClassRepositories;
using PTEducation.Data.Repositories.StudentGuardianRepositories;
using PTEducation.Data.Repositories.UserRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PTEducation.Business.Services.ChatServices
{
    public class ChatServices : IChatServices
    {
        private readonly IChatRepositories _chatRepositories;
        private readonly IChatDetailRepositories _chatDetailRepositories;
        private readonly IChatMessageRepositories _chatMessageRepositories;
        private readonly IClassRepositories _classRepositories;
        private readonly IStudentClassRepositories _studentClassRepositories;
        private readonly IStudentGuardianRepositories _studentGuardianRepositories;
        private readonly IUserRepositories _userRepositories;

        public ChatServices(
            IChatRepositories chatRepositories,
            IChatDetailRepositories chatDetailRepositories,
            IChatMessageRepositories chatMessageRepositories,
            IClassRepositories classRepositories,
            IStudentClassRepositories studentClassRepositories,
            IStudentGuardianRepositories studentGuardianRepositories,
            IUserRepositories userRepositories)
        {
            _chatRepositories = chatRepositories;
            _chatDetailRepositories = chatDetailRepositories;
            _chatMessageRepositories = chatMessageRepositories;
            _classRepositories = classRepositories;
            _studentClassRepositories = studentClassRepositories;
            _studentGuardianRepositories = studentGuardianRepositories;
            _userRepositories = userRepositories;
        }

        public async Task<ListDataResultModel<ChatRoomResModel>> GetMyChats(string userId, string role)
        {
            var userRole = role.ToLower();
            List<Class> activeClasses = new List<Class>();

            // 1. Get relevant active classes based on user role
            if (userRole == "admin" || userRole == "manager")
            {
                var classes = await _classRepositories.GetList(c => c.Status == "Active");
                activeClasses = classes.ToList();
            }
            else if (userRole == "student")
            {
                var studentClasses = await _studentClassRepositories.GetList(
                    sc => sc.StudentId == userId && sc.Status == "Active",
                    includeProperties: "Class");
                activeClasses = studentClasses.Select(sc => sc.Class).Where(c => c != null && c.Status == "Active").ToList();
            }

            var classIds = activeClasses.Select(c => c.Id).Distinct().ToList();

            // 2. Ensure Chat rooms exist for these active classes
            var existingChats = await _chatRepositories.GetList(c => c.ClassId.HasValue && classIds.Contains(c.ClassId.Value));
            var existingChatClassIds = existingChats.Select(c => c.ClassId!.Value).ToHashSet();

            var unixNow = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            foreach (var activeClass in activeClasses)
            {
                if (!existingChatClassIds.Contains(activeClass.Id))
                {
                    var newChat = new Chat
                    {
                        Id = Guid.NewGuid(),
                        ClassId = activeClass.Id,
                        Title = activeClass.Name,
                        CreatedAt = unixNow
                    };
                    await _chatRepositories.Insert(newChat, saveChanges: false);
                }
            }
            await _chatRepositories.SaveChangesAsync();

            // 3. Re-query all relevant chats including newly created ones
            var allMyChats = await _chatRepositories.GetList(c => c.ClassId.HasValue && classIds.Contains(c.ClassId.Value));
            var allMyChatIds = allMyChats.Select(c => c.Id).ToHashSet();

            // 4. Ensure ChatDetail exists for this user in all their chats
            var existingDetails = await _chatDetailRepositories.GetList(cd => cd.UserId == userId);
            var existingDetailChatIds = existingDetails.Select(cd => cd.ChatId).ToHashSet();

            foreach (var chat in allMyChats)
            {
                if (!existingDetailChatIds.Contains(chat.Id))
                {
                    var newDetail = new ChatDetail
                    {
                        Id = Guid.NewGuid(),
                        ChatId = chat.Id,
                        UserId = userId,
                        JoinedAt = unixNow
                    };
                    await _chatDetailRepositories.Insert(newDetail, saveChanges: false);
                }
            }
            await _chatDetailRepositories.SaveChangesAsync();

            // 5. Build final list of chat rooms for this user
            var userChatDetails = await _chatDetailRepositories.GetList(
                cd => cd.UserId == userId, 
                includeProperties: "Chat,Chat.Class,Chat.ChatDetails,Chat.ChatDetails.User");

            // Filter out chats for classes that are inactive
            userChatDetails = userChatDetails.Where(cd => 
                !cd.Chat.ClassId.HasValue || 
                (cd.Chat.Class != null && cd.Chat.Class.Status == "Active")
            ).ToList();

            if (userRole == "guardian")
            {
                userChatDetails = userChatDetails.Where(cd => !cd.Chat.ClassId.HasValue).ToList();
            }

            var result = new List<ChatRoomResModel>();

            foreach (var detail in userChatDetails)
            {
                var lastMsg = await _chatMessageRepositories.GetLastMessage(detail.ChatId);
                var unreadCount = await _chatMessageRepositories.GetUnreadCount(detail.ChatId, detail.LastReadMessageId);

                string title = "Lớp học";
                if (detail.Chat.ClassId.HasValue)
                {
                    title = detail.Chat.Title ?? "Lớp học";
                }
                else
                {
                    var otherDetail = detail.Chat.ChatDetails.FirstOrDefault(cd => cd.UserId != userId);
                    title = otherDetail?.User?.Name ?? "Người dùng hệ thống";
                }

                int participantCount = detail.Chat.ChatDetails.Count();
                if (detail.Chat.ClassId.HasValue)
                {
                    var studentClasses = await _studentClassRepositories.GetList(sc => sc.ClassId == detail.Chat.ClassId.Value && sc.Status == "Active");
                    participantCount = studentClasses.Count();
                }

                result.Add(new ChatRoomResModel
                {
                    ChatId = detail.ChatId,
                    Title = title,
                    ClassId = detail.Chat.ClassId,
                    LastMessage = lastMsg?.Content,
                    LastMessageTime = lastMsg?.CreatedAt,
                    UnreadCount = unreadCount,
                    NumberOfParticipant = participantCount
                });
            }

            // Order chats by last message time, newest first, or creation time if no messages
            var sortedResult = result
                .OrderByDescending(r => r.LastMessageTime ?? 0)
                .ToList();

            return new ListDataResultModel<ChatRoomResModel> { Data = sortedResult };
        }

        public async Task<PagedListDataResultModel<ChatMessageResModel>> GetChatMessages(Guid chatId, string userId, int pageIndex = 1, int? limit = null)
        {
            // Verify if user is part of the chat room
            var detail = await _chatDetailRepositories.GetSingle(cd => cd.ChatId == chatId && cd.UserId == userId);
            if (detail == null)
            {
                // Verify if they are in the class corresponding to this chat to join them dynamically
                var chat = await _chatRepositories.GetSingle(c => c.Id == chatId);
                if (chat == null)
                {
                    throw new CustomException("Phòng chat không tồn tại.");
                }

                bool isAuthorized = await VerifyUserAccessToChat(chat, userId);
                if (!isAuthorized)
                {
                    throw new CustomException("Bạn không có quyền tham gia phòng chat này.");
                }

                // Dynamically join the user to this chat room
                detail = new ChatDetail
                {
                    Id = Guid.NewGuid(),
                    ChatId = chatId,
                    UserId = userId,
                    JoinedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                };
                await _chatDetailRepositories.Insert(detail);
            }

            int pageSize = limit ?? 50;
            // Get messages in descending order (newest first)
            var pagedMessages = await _chatMessageRepositories.GetPagedList(
                filter: m => m.ChatId == chatId,
                orderBy: q => q.OrderByDescending(m => m.CreatedAt),
                includeProperties: "SenderDetail,SenderDetail.User",
                pageIndex: pageIndex,
                pageSize: pageSize
            );

            // Map messages and reverse them to ascending order for display
            var result = pagedMessages.Data?.Select(m => new ChatMessageResModel
            {
                Id = m.Id,
                ChatId = m.ChatId,
                SenderId = m.SenderDetail.UserId,
                SenderName = m.SenderDetail.User.Name,
                SenderAvatarUrl = m.SenderDetail.User.AvatarUrl,
                SenderRole = m.SenderDetail.User.Role,
                Content = m.Content,
                MessageType = m.MessageType,
                CreatedAt = m.CreatedAt
            })
            .OrderBy(m => m.CreatedAt)
            .ToList() ?? new List<ChatMessageResModel>();

            return new PagedListDataResultModel<ChatMessageResModel> { 
                Data = result,
                PageNumber = pagedMessages.PageNumber,
                PageSize = pagedMessages.PageSize,
                TotalPages = pagedMessages.TotalPages
            };
        }

        public async Task<DataResultModel<ChatMessageResModel>> SendMessage(Guid chatId, string userId, string content, int messageType = 0)
        {
            var detail = await _chatDetailRepositories.GetSingle(cd => cd.ChatId == chatId && cd.UserId == userId);
            if (detail == null)
            {
                var chat = await _chatRepositories.GetSingle(c => c.Id == chatId);
                if (chat == null)
                {
                    throw new CustomException("Phòng chat không tồn tại.");
                }

                bool isAuthorized = await VerifyUserAccessToChat(chat, userId);
                if (!isAuthorized)
                {
                    throw new CustomException("Bạn không có quyền gửi tin nhắn trong phòng chat này.");
                }

                // Dynamically join the user
                detail = new ChatDetail
                {
                    Id = Guid.NewGuid(),
                    ChatId = chatId,
                    UserId = userId,
                    JoinedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                };
                await _chatDetailRepositories.Insert(detail);
            }

            var sender = await _userRepositories.GetSingle(u => u.Id == userId);
            if (sender == null)
            {
                throw new CustomException("Người dùng không tồn tại.");
            }

            var unixNow = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var message = new ChatMessage
            {
                Id = Guid.NewGuid(),
                ChatId = chatId,
                SenderDetailId = detail.Id,
                Content = content,
                MessageType = messageType,
                CreatedAt = unixNow
            };

            await _chatMessageRepositories.Insert(message);

            // Update sender's LastReadMessageId
            detail.LastReadMessageId = message.Id;
            await _chatDetailRepositories.Update(detail);

            var res = new ChatMessageResModel
            {
                Id = message.Id,
                ChatId = message.ChatId,
                SenderId = userId,
                SenderName = sender.Name,
                SenderAvatarUrl = sender.AvatarUrl,
                SenderRole = sender.Role,
                Content = message.Content,
                MessageType = message.MessageType,
                CreatedAt = message.CreatedAt
            };

            return new DataResultModel<ChatMessageResModel> { Data = res };
        }

        public async Task<MessageResultModel> MarkAsRead(Guid chatId, string userId)
        {
            var detail = await _chatDetailRepositories.GetSingle(cd => cd.ChatId == chatId && cd.UserId == userId);
            if (detail == null)
            {
                throw new CustomException("Bạn không là thành viên của cuộc trò chuyện này.");
            }

            var lastMsg = await _chatMessageRepositories.GetLastMessage(chatId);
            if (lastMsg != null)
            {
                detail.LastReadMessageId = lastMsg.Id;
                await _chatDetailRepositories.Update(detail);
            }

            return new MessageResultModel { Message = "Đã đánh dấu đã đọc." };
        }

        private async Task<bool> VerifyUserAccessToChat(Chat chat, string userId)
        {
            if (!chat.ClassId.HasValue) return false;

            var userObj = await _userRepositories.GetSingle(u => u.Id == userId);
            if (userObj == null) return false;

            var userRole = userObj.Role.ToLower();
            if (userRole == "admin" || userRole == "manager") return true;

            if (userRole == "student")
            {
                var sc = await _studentClassRepositories.GetSingle(sc => sc.StudentId == userId && sc.ClassId == chat.ClassId.Value && sc.Status == "Active");
                return sc != null;
            }



            return false;
        }

        public async Task<ListDataResultModel<ChatContactResModel>> GetSupportContacts(string userId)
        {
            var supportUsers = await _userRepositories.GetList(u =>
                (u.Role == RoleEnums.Admin.ToString() || u.Role == RoleEnums.Manager.ToString()) &&
                u.Status == AccountStatusEnums.Active.ToString());

            var myPrivateChatDetails = await _chatDetailRepositories.GetList(
                cd => cd.UserId == userId && !cd.Chat.ClassId.HasValue,
                includeProperties: "Chat,Chat.ChatDetails");

            var privateChatMap = new Dictionary<string, Guid>();
            foreach (var detail in myPrivateChatDetails)
            {
                var otherDetail = detail.Chat.ChatDetails.FirstOrDefault(cd => cd.UserId != userId);
                if (otherDetail != null)
                {
                    privateChatMap[otherDetail.UserId] = detail.ChatId;
                }
            }

            var contacts = supportUsers.Select(u => new ChatContactResModel
            {
                UserId = u.Id,
                Name = u.Name,
                AvatarUrl = u.AvatarUrl,
                Role = u.Role,
                ChatId = privateChatMap.TryGetValue(u.Id, out var chatId) ? chatId : null
            }).ToList();

            return new ListDataResultModel<ChatContactResModel> { Data = contacts };
        }

        public async Task<DataResultModel<Guid>> GetOrCreatePrivateChat(string userId, string targetUserId)
        {
            var targetUser = await _userRepositories.GetSingle(u => u.Id == targetUserId);
            if (targetUser == null)
            {
                throw new CustomException("Người nhận không tồn tại.");
            }

            var existingChatDetail = await _chatDetailRepositories.GetSingle(
                cd => cd.UserId == userId && !cd.Chat.ClassId.HasValue &&
                      cd.Chat.ChatDetails.Any(other => other.UserId == targetUserId),
                includeProperties: "Chat,Chat.ChatDetails"
            );

            if (existingChatDetail != null)
            {
                return new DataResultModel<Guid> { Data = existingChatDetail.ChatId };
            }

            var unixNow = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var newChat = new Chat
            {
                Id = Guid.NewGuid(),
                ClassId = null,
                Title = null,
                CreatedAt = unixNow
            };
            await _chatRepositories.Insert(newChat, saveChanges: false);

            var detailSelf = new ChatDetail
            {
                Id = Guid.NewGuid(),
                ChatId = newChat.Id,
                UserId = userId,
                JoinedAt = unixNow
            };
            await _chatDetailRepositories.Insert(detailSelf, saveChanges: false);

            var detailTarget = new ChatDetail
            {
                Id = Guid.NewGuid(),
                ChatId = newChat.Id,
                UserId = targetUserId,
                JoinedAt = unixNow
            };
            await _chatDetailRepositories.Insert(detailTarget, saveChanges: false);

            await _chatRepositories.SaveChangesAsync();

            return new DataResultModel<Guid> { Data = newChat.Id };
        }
    }
}
