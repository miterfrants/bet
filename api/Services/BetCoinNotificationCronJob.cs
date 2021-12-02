using System.Linq;
using System;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Homo.Bet.Api
{
    public class BetCoinNotification : CronJobService
    {
        private readonly ILogger<BetCoinNotification> _logger;
        private BargainingChipDBContext _dbContext;
        private readonly string _envName;
        private readonly string _systemEmail;
        private readonly string _sendGridAPIKey;

        public BetCoinNotification(IScheduleConfig<BetCoinNotification> config, ILogger<BetCoinNotification> logger, IServiceProvider serviceProvider, Microsoft.AspNetCore.Hosting.IWebHostEnvironment env, IOptions<AppSettings> appSettings)
            : base(config.CronExpression, config.TimeZoneInfo, serviceProvider)
        {
            _logger = logger;
            _envName = env.EnvironmentName;
            _systemEmail = appSettings.Value.Common.SystemEmail;
            _sendGridAPIKey = appSettings.Value.Secrets.SendGridApiKey;
        }

        public override System.Threading.Tasks.Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("BetCoinNotification starts.");
            return base.StartAsync(cancellationToken);
        }

        public override System.Threading.Tasks.Task DoWork(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{DateTime.Now:hh:mm:ss} is working.");
            _dbContext = _serviceProvider.GetService<BargainingChipDBContext>();
            List<UserCoinLogBalance> hasFreeBetCoinsUsers = CoinsLogDataService.GetHasFreeBetCoinsUsers(_dbContext);
            List<long> userIds = hasFreeBetCoinsUsers.Select(x => x.OwnerId).ToList<long>();
            List<User> users = UserDataservice.GetAllByIds(userIds, _dbContext);
            users.ForEach(user =>
            {
                UserCoinLogBalance targetUser = hasFreeBetCoinsUsers.Find(x => x.OwnerId == user.Id);
                int freeCoin = targetUser == null ? 0 : targetUser.Qty;
                MailHelper.Send(MailProvider.SEND_GRID, new MailTemplate()
                {
                    Subject = $"Homo Bet 提醒你還有未投注的籌碼 {freeCoin}",
                    Content = "<a href=\"https://github.com/miterfrants/itemhub/issues\">投注 Issues 連結</a>"
                }, _systemEmail, user.Email, _sendGridAPIKey);
            });

            return System.Threading.Tasks.Task.CompletedTask;
        }

        public override System.Threading.Tasks.Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("BetCoinNotification is stopping.");
            return base.StopAsync(cancellationToken);
        }
    }
}
