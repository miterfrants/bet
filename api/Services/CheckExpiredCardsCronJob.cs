using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Homo.Bet.Api
{
    public class CheckExpiredCardsCronJob : CronJobService
    {
        private readonly ILogger<CheckExpiredCardsCronJob> _logger;

        public CheckExpiredCardsCronJob(
            IScheduleConfig<CheckExpiredCardsCronJob> config,
            ILogger<CheckExpiredCardsCronJob> logger,
            IServiceProvider serviceProvider)
            : base(config.CronExpression, config.TimeZoneInfo, serviceProvider)
        {
            _logger = logger;
        }

        public override System.Threading.Tasks.Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("過期卡片檢查服務已啟動");
            return base.StartAsync(cancellationToken);
        }

        public override async System.Threading.Tasks.Task DoWork(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} 開始檢查過期卡片");

            try
            {
                // 使用 scope 來確保每次執行都有獨立的 DbContext
                using (var scope = _serviceProvider.CreateScope())
                {
                    var cardRepository = scope.ServiceProvider.GetRequiredService<CardRepository>();

                    // 移除過期的卡片（裝備超過 7 天）
                    var removedCount = cardRepository.RemoveExpiredCards(7);

                    _logger.LogInformation($"成功移除 {removedCount} 張過期卡片");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查過期卡片時發生錯誤");
            }

            await System.Threading.Tasks.Task.CompletedTask;
        }

        public override System.Threading.Tasks.Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("過期卡片檢查服務已停止");
            return base.StopAsync(cancellationToken);
        }
    }
}
