using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using PTEducation.Business.Services.RedisServices;
using PTEducation.Data.Entities;
using PTEducation.Data.Repositories.ChatRepositories;

namespace PTEducation.API.HostedServices
{
    public class ChatWriteBehindHostedService : BackgroundService
    {
        private readonly ILogger<ChatWriteBehindHostedService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public ChatWriteBehindHostedService(
            ILogger<ChatWriteBehindHostedService> logger,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ChatWriteBehindHostedService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPendingMessagesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in ChatWriteBehindHostedService.");
                }

                // Check queue every 3 seconds
                await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
            }
        }

        private async Task ProcessPendingMessagesAsync(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var redisService = scope.ServiceProvider.GetRequiredService<IRedisService>();
            var chatMessageRepositories = scope.ServiceProvider.GetRequiredService<IChatMessageRepositories>();
            var chatDetailRepositories = scope.ServiceProvider.GetRequiredService<IChatDetailRepositories>();

            // Get up to 50 messages from Redis
            var rawMessages = await redisService.PopFromQueueBatchAsync("Chat_PendingMessages", 50);

            if (rawMessages.Count > 0)
            {
                var messagesToInsert = new List<ChatMessage>();
                foreach (var raw in rawMessages)
                {
                    var msg = JsonSerializer.Deserialize<ChatMessage>(raw);
                    if (msg != null)
                    {
                        messagesToInsert.Add(msg);
                    }
                }

                if (messagesToInsert.Count > 0)
                {
                    // Since it's a generic repository, we might have to insert them one by one if BulkInsert is not available
                    // or we can just iterate. Let's iterate for safety.
                    foreach (var msg in messagesToInsert)
                    {
                        await chatMessageRepositories.Insert(msg);
                        
                        // Update the sender's LastReadMessageId now that the message is in DB
                        if (msg.SenderDetailId != Guid.Empty)
                        {
                            var detail = await chatDetailRepositories.GetSingle(x => x.Id == msg.SenderDetailId);
                            if (detail != null)
                            {
                                detail.LastReadMessageId = msg.Id;
                                await chatDetailRepositories.Update(detail);
                            }
                        }
                    }
                    _logger.LogInformation($"Successfully saved {messagesToInsert.Count} pending chat messages to DB.");
                }
            }
        }
    }
}
