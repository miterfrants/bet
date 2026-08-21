using System;
using System.Threading;
using Cronos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Homo.Bet.Api
{
    public abstract class CronJobService : IHostedService, IDisposable
    {
        private System.Timers.Timer _timer;
        private readonly CronExpression _expression;
        private readonly TimeZoneInfo _timeZoneInfo;
        protected readonly IServiceProvider _serviceProvider;

        protected CronJobService(string cronExpression, TimeZoneInfo timeZoneInfo, IServiceProvider serviceProvider)
        {
            _expression = CronExpression.Parse(cronExpression);
            _timeZoneInfo = timeZoneInfo;
            _serviceProvider = serviceProvider;
        }

        public virtual async System.Threading.Tasks.Task StartAsync(CancellationToken cancellationToken)
        {
            await ScheduleJob(cancellationToken);
        }

        protected virtual async System.Threading.Tasks.Task ScheduleJob(CancellationToken cancellationToken)
        {
            var next = _expression.GetNextOccurrence(DateTimeOffset.Now, _timeZoneInfo);
            if (next.HasValue)
            {
                var delay = next.Value - DateTimeOffset.Now;
                if (delay.TotalMilliseconds <= 0)   // prevent non-positive values from being passed into Timer
                {
                    await ScheduleJob(cancellationToken);
                    return;   // 已經由遞迴那次排好 timer，這裡不能再用非正數的 delay 建立 timer
                }
                _timer = new System.Timers.Timer(delay.TotalMilliseconds);
                _timer.Elapsed += async (sender, args) =>
                {
                    _timer.Dispose();  // reset and dispose timer
                    _timer = null;

                    // Elapsed 的 handler 是 async void，只要 DoWork 丟出例外就會變成
                    // unhandled exception 直接把整個 process 殺掉，所以一定要在這裡攔下來。
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            await DoWork(cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            LogUnhandled(nameof(DoWork), ex);
                        }
                    }

                    if (!cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            await ScheduleJob(cancellationToken);    // reschedule next
                        }
                        catch (Exception ex)
                        {
                            LogUnhandled(nameof(ScheduleJob), ex);
                        }
                    }
                };
                _timer.Start();
            }
            await System.Threading.Tasks.Task.CompletedTask;
        }

        private void LogUnhandled(string stage, Exception ex)
        {
            var loggerFactory = _serviceProvider?.GetService(typeof(ILoggerFactory)) as ILoggerFactory;
            var logger = loggerFactory?.CreateLogger(GetType().FullName);
            if (logger != null)
            {
                logger.LogError(ex, $"{GetType().Name}.{stage} 發生未預期的例外，已忽略並繼續排下一次執行");
            }
            else
            {
                Console.WriteLine($"{GetType().Name}.{stage} 發生未預期的例外，已忽略並繼續排下一次執行: {ex}");
            }
        }

        public virtual async System.Threading.Tasks.Task DoWork(CancellationToken cancellationToken)
        {
            await System.Threading.Tasks.Task.Delay(5000, cancellationToken);  // do the work
        }

        public virtual async System.Threading.Tasks.Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Stop();
            await System.Threading.Tasks.Task.CompletedTask;
        }

        public virtual void Dispose()
        {
            _timer?.Dispose();
        }
    }

    public interface IScheduleConfig<T>
    {
        string CronExpression { get; set; }
        TimeZoneInfo TimeZoneInfo { get; set; }
    }

    public class ScheduleConfig<T> : IScheduleConfig<T>
    {
        public string CronExpression { get; set; }
        public TimeZoneInfo TimeZoneInfo { get; set; }
    }

    public static class ScheduledServiceExtensions
    {
        public static IServiceCollection AddCronJob<T>(this IServiceCollection services, Action<IScheduleConfig<T>> options) where T : CronJobService
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options), @"Please provide Schedule Configurations.");
            }
            var config = new ScheduleConfig<T>();
            options.Invoke(config);
            if (string.IsNullOrWhiteSpace(config.CronExpression))
            {
                throw new ArgumentNullException(nameof(ScheduleConfig<T>.CronExpression), @"Empty Cron Expression is not allowed.");
            }

            services.AddSingleton<IScheduleConfig<T>>(config);
            services.AddHostedService<T>();
            return services;
        }
    }
}
