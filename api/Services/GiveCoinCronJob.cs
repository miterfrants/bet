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
    public class RenewCoinLog : CronJobService
    {
        private readonly ILogger<RenewCoinLog> _logger;
        private BargainingChipDBContext _dbContext;
        private readonly string _envName;
        private readonly string _systemEmail;
        private readonly string _sendGridAPIKey;

        public RenewCoinLog(IScheduleConfig<RenewCoinLog> config, ILogger<RenewCoinLog> logger, IServiceProvider serviceProvider, Microsoft.AspNetCore.Hosting.IWebHostEnvironment env, IOptions<AppSettings> appSettings)
            : base(config.CronExpression, config.TimeZoneInfo, serviceProvider)
        {
            _logger = logger;
            _envName = env.EnvironmentName;
            _systemEmail = appSettings.Value.Common.SystemEmail;
        }

        public override System.Threading.Tasks.Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("GiveCoinCronJob starts.");
            return base.StartAsync(cancellationToken);
        }

        public override System.Threading.Tasks.Task DoWork(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{DateTime.Now:hh:mm:ss}  is working.");
            _dbContext = _serviceProvider.GetService<BargainingChipDBContext>();
            // clear free coin
            CoinsLogDataService.ClearAll(_dbContext);
            // lock coin log
            CoinsLogDataService.LockBetted(_dbContext);
            // give new coins
            List<long> userIds = UserDataservice.GetAllByIds(null, _dbContext).Select(x => x.Id).ToList<long>();
            CoinsLogDataService.Give(_dbContext, userIds, 10);
            return System.Threading.Tasks.Task.CompletedTask;
        }

        public override System.Threading.Tasks.Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("CronJob is stopping.");
            return base.StopAsync(cancellationToken);
        }
    }
}
