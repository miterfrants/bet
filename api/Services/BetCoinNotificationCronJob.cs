using System.Linq;
using System;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;

namespace Homo.Bet.Api
{
    public class BetCoinNotification : CronJobService
    {
        private readonly ILogger<BetCoinNotification> _logger;
        private BargainingChipDBContext _dbContext;
        private readonly string _discordWebhook;

        public BetCoinNotification(IScheduleConfig<BetCoinNotification> config, ILogger<BetCoinNotification> logger, IServiceProvider serviceProvider, Microsoft.AspNetCore.Hosting.IWebHostEnvironment env, IOptions<AppSettings> appSettings)
            : base(config.CronExpression, config.TimeZoneInfo, serviceProvider)
        {
            _logger = logger;
            _discordWebhook = appSettings.Value.Secrets.DiscordWebhook;
        }

        public override System.Threading.Tasks.Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("BetCoinNotification starts.");
            return base.StartAsync(cancellationToken);
        }

        public override async System.Threading.Tasks.Task DoWork(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{DateTime.Now:hh:mm:ss} is working.");
            _dbContext = _serviceProvider.GetService<BargainingChipDBContext>();
            List<UserCoinLogBalance> hasFreeBetCoinsUsers = CoinsLogDataService.GetFreeBetUsers(_dbContext).Where(x => x.Qty > 0).ToList();
            List<long> userIds = hasFreeBetCoinsUsers.Select(x => x.OwnerId).ToList<long>();
            List<User> users = UserDataservice.GetAllByIds(userIds, _dbContext);
            var _httpClient = new HttpClient();
            var userIdAndDiscordAccountMapping = new Dictionary<long, string> { { 4, "995514040572989451" }, { 5, "954391229993484368" } };
            for (int i = 0; i < users.Count; i++)
            {
                var user = users[i];
                UserCoinLogBalance targetUser = hasFreeBetCoinsUsers.Find(x => x.OwnerId == user.Id);
                int freeCoin = targetUser == null ? 0 : targetUser.Qty;
                if (!userIdAndDiscordAccountMapping.ContainsKey(targetUser.OwnerId))
                {
                    continue;
                }
                var discordUserId = userIdAndDiscordAccountMapping[targetUser.OwnerId];
                await _httpClient.PostAsync(_discordWebhook, new StringContent($@"{{""content"":""<@{userIdAndDiscordAccountMapping[targetUser.OwnerId]}> 提醒你還有未投注的籌碼 {freeCoin}""}}", Encoding.UTF8, "application/json"), CancellationToken.None);
            }
        }

        public override System.Threading.Tasks.Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("BetCoinNotification is stopping.");
            return base.StopAsync(cancellationToken);
        }
    }
}
