using Microsoft.EntityFrameworkCore;
using PTEducation.Data.Entities;

namespace PTEducation.API.HostedServices
{
    public class DatabaseMigrationHostedService : IHostedService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DatabaseMigrationHostedService> _logger;

        public DatabaseMigrationHostedService(IServiceScopeFactory scopeFactory, ILogger<DatabaseMigrationHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<PteducationContext>();

            _logger.LogInformation("Applying EF Core migrations on startup.");
            await dbContext.Database.MigrateAsync(cancellationToken);
            _logger.LogInformation("EF Core migrations applied successfully.");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}