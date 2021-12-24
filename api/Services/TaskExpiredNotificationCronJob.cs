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
    public class TaskExpiredNotification : CronJobService
    {
        private readonly ILogger<TaskExpiredNotification> _logger;
        private BargainingChipDBContext _dbContext;
        private readonly string _envName;
        private readonly string _systemEmail;
        private readonly string _sendGridAPIKey;

        public TaskExpiredNotification(IScheduleConfig<TaskExpiredNotification> config, ILogger<TaskExpiredNotification> logger, IServiceProvider serviceProvider, Microsoft.AspNetCore.Hosting.IWebHostEnvironment env, IOptions<AppSettings> appSettings)
            : base(config.CronExpression, config.TimeZoneInfo, serviceProvider)
        {
            _logger = logger;
            _envName = env.EnvironmentName;
            _systemEmail = appSettings.Value.Common.SystemEmail;
            _sendGridAPIKey = appSettings.Value.Secrets.SendGridApiKey;
        }

        public override System.Threading.Tasks.Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("TaskExpiredNotification starts.");
            return base.StartAsync(cancellationToken);
        }

        public override System.Threading.Tasks.Task DoWork(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{DateTime.Now:hh:mm:ss} is working.");
            _dbContext = _serviceProvider.GetService<BargainingChipDBContext>();
            List<Task> tasks = TaskDataservice.GetBeingExpiredTask(_dbContext, 2).ToList();
            List<long> userIds = tasks.Where(x => x.AssigneeId != null).Select(x => x.AssigneeId.GetValueOrDefault()).ToList<long>();
            List<User> users = UserDataservice.GetAllByIds(userIds, _dbContext);
            users.ForEach(user =>
            {
                List<Task> filteredTasks = tasks.Where(x => x.AssigneeId == user.Id).ToList();
                MailHelper.Send(MailProvider.SEND_GRID, new MailTemplate()
                {
                    Subject = $"Homo Bet 提醒你有快要到期的 Task 未完成",
                    Content = "<a href=\"https://github.com/miterfrants/itemhub/issues\">投注 Issues 連結</a>"
                }, _systemEmail, user.Email, _sendGridAPIKey);
            });

            return System.Threading.Tasks.Task.CompletedTask;
        }

        public override System.Threading.Tasks.Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("TaskExpiredNotification is stopping.");
            return base.StopAsync(cancellationToken);
        }
    }
}
