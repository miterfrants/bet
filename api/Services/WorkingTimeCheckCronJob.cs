using System;
using System.Threading;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using Newtonsoft.Json;


namespace Homo.Bet.Api
{
    public class WorkingTimeCheckCronJob : CronJobService
    {
        private readonly ILogger<WorkingTimeCheckCronJob> _logger;
        private readonly string _envName;
        private Api.AppSettings _appSettings;
        private readonly string _discordWebhook;

        public WorkingTimeCheckCronJob(IScheduleConfig<WorkingTimeCheckCronJob> config, ILogger<WorkingTimeCheckCronJob> logger, IServiceProvider serviceProvider, Microsoft.AspNetCore.Hosting.IWebHostEnvironment env, IOptions<AppSettings> appSettings)
            : base(config.CronExpression, config.TimeZoneInfo, serviceProvider)
        {
            _logger = logger;
            _envName = env.EnvironmentName;
            _appSettings = appSettings.Value;
            _discordWebhook = appSettings.Value.Secrets.DiscordWebhook;
        }

        public override System.Threading.Tasks.Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("WorkingTimeCheckCronJob starts.");
            return base.StartAsync(cancellationToken);
        }

        public override async System.Threading.Tasks.Task DoWork(CancellationToken cancellationToken)
        {
            // 星期一和星期日跳過檢查
            // GMT+0 的星期日和星期六跳過
            if (DateTime.Today.DayOfWeek == DayOfWeek.Sunday || DateTime.Today.DayOfWeek == DayOfWeek.Saturday)
            {
                return;
            }

            _logger.LogInformation($"{DateTime.Now:hh:mm:ss} is working.");

            // 檢查當天的工時
            string url = $"https://api.track.toggl.com/reports/api/v3/workspace/8976470/search/time_entries/totals";
            var optionsBuilder = new DbContextOptionsBuilder<BargainingChipDBContext>();
            var serverVersion = new MySqlServerVersion(new Version(8, 0, 25));
            optionsBuilder.UseMySql(_appSettings.Secrets.DBConnectionString, serverVersion);
            var checkDate = System.DateTime.Now.AddDays(-1);
            var today = System.DateTime.Now;
            var users = new List<ToggleAndBetUserMapping>{
                new ToggleAndBetUserMapping{
                    BetUserId=4,
                    ToggleUserId=11540860
                },
                new ToggleAndBetUserMapping{
                    BetUserId=5,
                    ToggleUserId=11540872
                },
            };

            using (HttpClient toggleClient = new HttpClient())
            using (BargainingChipDBContext dbContext = new BargainingChipDBContext(optionsBuilder.Options))
            {
                var byteArray = Encoding.ASCII.GetBytes($"{_appSettings.Secrets.ToggleUsername}:{_appSettings.Secrets.TogglePassword}");
                toggleClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                int lastDay = DateTime.DaysInMonth(checkDate.Year, checkDate.Month);
                DateTime endOfMonth = new DateTime(checkDate.Year, checkDate.Month, lastDay);
                DateTime startOfMonth = new DateTime(checkDate.Year, checkDate.Month, 1);
                for (var i = 0; i < users.Count(); i++)
                {
                    var user = users[i];
                    var httpContent = new StringContent(@$"{{""start_date"":""{checkDate.ToString("yyyy-MM-dd")}"", ""end_date"": ""{today.ToString("yyyy-MM-dd")}"", ""user_ids"": [{user.ToggleUserId}]}}", System.Text.Encoding.UTF8, "application/json");
                    // 發送 GET 請求
                    var response = await toggleClient.PostAsync(url, httpContent);
                    // 讀取結果
                    var result = await response.Content.ReadAsStringAsync();
                    var timeRecords = JsonConvert.DeserializeObject<TimeRecord>(result);
                    var shouldBeCheckData = timeRecords.seconds < 5 * 60 * 60;
                    if (shouldBeCheckData)
                    {
                        // 取得今天是否有請假資料
                        var leaves = RewardDataService.GetLeaves(dbContext, user.BetUserId, checkDate, new List<REWARD_TYPE> {
                            REWARD_TYPE.SICK_LEAVE,
                            REWARD_TYPE.LEAVE,
                            REWARD_TYPE.MENSTRUATION_LEAVE,
                        });
                        if (leaves == 0) // 沒有請假時間又超過五個小時進行懲罰
                        {
                            CoinsLogDataService.Create(dbContext, user.BetUserId, null, 0, COIN_LOG_TYPE.PUNISHMENT_FOR_INSUFFICIENT_WORKING_HOURS, new DTOs.CoinLog
                            {
                                Qty = -5
                            });
                        }
                    }
                    if (checkDate.ToString("yyyy-MM-dd") == endOfMonth.AddDays(-7).ToString("yyyy-MM-dd"))
                    {

                        httpContent = new StringContent(@$"{{""start_date"":""{startOfMonth.ToString("yyyy-MM-dd")}"", ""end_date"": ""{endOfMonth.AddDays(1).ToString("yyyy-MM-dd")}"", ""user_ids"": [{user.ToggleUserId}]}}", System.Text.Encoding.UTF8, "application/json");
                        response = await toggleClient.PostAsync(url, httpContent);
                        result = await response.Content.ReadAsStringAsync();
                        timeRecords = JsonConvert.DeserializeObject<TimeRecord>(result);
                        if (timeRecords.seconds > 120 * 60 * 60)
                        {
                            CoinsLogDataService.Create(dbContext, user.BetUserId, null, 0, COIN_LOG_TYPE.EARN, new DTOs.CoinLog
                            {
                                Qty = 250
                            });
                        }
                    }
                }

            }
        }

        public override System.Threading.Tasks.Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("WorkingTimeCheckCronJob is stopping.");
            return base.StopAsync(cancellationToken);
        }
    }

    public class ToggleAndBetUserMapping
    {
        public long ToggleUserId { get; set; }
        public long BetUserId { get; set; }
    }

    public class TimeRecord
    {
        public long seconds { get; set; }

    }
}
