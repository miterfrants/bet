using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Homo.Bet.Api
{
    public class WeeklyCardGenerationCronJob : CronJobService
    {
        private readonly ILogger<WeeklyCardGenerationCronJob> _logger;
        private BargainingChipDBContext _dbContext;

        public WeeklyCardGenerationCronJob(
            IScheduleConfig<WeeklyCardGenerationCronJob> config,
            ILogger<WeeklyCardGenerationCronJob> logger,
            IServiceProvider serviceProvider)
            : base(config.CronExpression, config.TimeZoneInfo, serviceProvider)
        {
            _logger = logger;
        }

        public override System.Threading.Tasks.Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("週卡片生成服務已啟動");
            return base.StartAsync(cancellationToken);
        }

        public override async System.Threading.Tasks.Task DoWork(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} 開始生成每週卡片");

            try
            {
                _dbContext = _serviceProvider.GetService<BargainingChipDBContext>();

                // 生成每週卡片
                var cards = CardTemplateDataservice.GenerateWeeklyCards(_dbContext);

                _logger.LogInformation($"成功生成 {cards.Count} 張每週卡片");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成每週卡片時發生錯誤");
            }

            await System.Threading.Tasks.Task.CompletedTask;
        }

        public override System.Threading.Tasks.Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("週卡片生成服務已停止");
            return base.StopAsync(cancellationToken);
        }
    }
}
