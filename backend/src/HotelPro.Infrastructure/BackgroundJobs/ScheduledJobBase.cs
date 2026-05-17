using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HotelPro.Infrastructure.BackgroundJobs;

public abstract class ScheduledJobBase : IHostedService, IDisposable
{
    protected abstract string JobName { get; }
    protected abstract TimeSpan Interval { get; }
    protected abstract Task ExecuteJobAsync(CancellationToken ct);

    private Timer? _timer;
    private readonly ILogger _logger;

    protected ScheduledJobBase(ILogger logger)
    {
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("[{JobName}] Starting scheduled job with interval {Interval}", JobName, Interval);
        _timer = new Timer(async _ => await ExecuteJobSafeAsync(), null, TimeSpan.Zero, Interval);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("[{JobName}] Stopping scheduled job", JobName);
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    private async Task ExecuteJobSafeAsync()
    {
        try
        {
            _logger.LogInformation("[{JobName}] Starting execution", JobName);
            using var cts = new CancellationTokenSource(Interval);
            await ExecuteJobAsync(cts.Token);
            _logger.LogInformation("[{JobName}] Completed successfully", JobName);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("[{JobName}] Cancelled (timeout or shutdown)", JobName);
        }
        catch (ObjectDisposedException)
        {
            _logger.LogInformation("[{JobName}] Host disposed", JobName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{JobName}] Error during execution: {Message}", JobName, ex.Message);
        }
    }

    public void Dispose() => _timer?.Dispose();
}
