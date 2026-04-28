using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PTEducation.Business.Services.UserServices;

namespace PTEducation.API.HostedServices
{
    public class AdminInitializerHostedService : IHostedService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public AdminInitializerHostedService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var userServices = scope.ServiceProvider.GetRequiredService<IUserServices>();
            await userServices.InitAdminIfNeeded();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
