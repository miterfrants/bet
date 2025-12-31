using System;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Homo.Bet.Api
{
    public class CheckExpiredCardsCronJob : CronJobService
    {
        private readonly ILogger<CheckExpiredCardsCronJob> _logger;
        private BargainingChipDBContext _dbContext;

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
                _dbContext = _serviceProvider.GetService<BargainingChipDBContext>();

                // 計算過期時間（7天前）
                var expiredDate = DateTime.Now.AddDays(-7);

                // 查詢過期的陷阱卡和增益卡（Type 1 和 2）
                var expiredCards = _dbContext.UserCard
                    .Where(x => x.DeletedAt == null
                        && x.IsEquipped == true
                        && x.EquippedAt != null
                        && x.EquippedAt < expiredDate
                        && (x.Card.Type == CARD_TYPE.TRAP || x.Card.Type == CARD_TYPE.BUFF))
                    .ToList();

                foreach (var userCard in expiredCards)
                {
                    // Soft delete 過期的卡片
                    userCard.DeletedAt = DateTime.Now;
                    userCard.IsEquipped = false;
                }

                _dbContext.SaveChanges();

                _logger.LogInformation($"成功移除 {expiredCards.Count} 張過期卡片");
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
