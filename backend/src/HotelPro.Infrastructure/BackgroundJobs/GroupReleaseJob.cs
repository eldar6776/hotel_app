using HotelPro.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HotelPro.Infrastructure.BackgroundJobs;

public class GroupReleaseJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<GroupReleaseJob> _logger;

    public GroupReleaseJob(IServiceProvider serviceProvider, ILogger<GroupReleaseJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var groupService = scope.ServiceProvider.GetRequiredService<IBookingGroupService>();

                var released = await groupService.ProcessExpiredReleasesAsync();
                if (released > 0)
                {
                    _logger.LogInformation("Group release job released {Released} bookings", released);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Group release job failed");
            }

            try
            {
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}
