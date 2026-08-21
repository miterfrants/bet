using System;
using System.Threading;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;


namespace Homo.Bet.Api
{
    public class GitHubAutoCommentViolationCronJob : CronJobService
    {
        private readonly ILogger<GitHubAutoCommentViolationCronJob> _logger;
        private readonly string _envName;
        private Api.AppSettings _appSettings;

        public GitHubAutoCommentViolationCronJob(IScheduleConfig<GitHubAutoCommentViolationCronJob> config, ILogger<GitHubAutoCommentViolationCronJob> logger, IServiceProvider serviceProvider, Microsoft.AspNetCore.Hosting.IWebHostEnvironment env, IOptions<AppSettings> appSettings)
            : base(config.CronExpression, config.TimeZoneInfo, serviceProvider)
        {
            _logger = logger;
            _envName = env.EnvironmentName;
            _appSettings = appSettings.Value;
        }

        public override System.Threading.Tasks.Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("GitHubAutoCommentViolationCronJob starts.");
            return base.StartAsync(cancellationToken);
        }

        public override async System.Threading.Tasks.Task DoWork(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{DateTime.Now:HH:mm:ss} is working.");
            // 取得現有 ItemHub 所有的 Issues 
            string token = _appSettings.Secrets.GitHubToken;
            string url = $"https://api.github.com/graphql";
            var optionsBuilder = new DbContextOptionsBuilder<BargainingChipDBContext>();
            var serverVersion = new MySqlServerVersion(new Version(8, 0, 25));
            optionsBuilder.UseMySql(_appSettings.Secrets.DBConnectionString, serverVersion);

            using (HttpClient githubClient = new HttpClient())
            using (HttpClient betClient = new HttpClient())
            using (BargainingChipDBContext dbContext = new BargainingChipDBContext(optionsBuilder.Options))
            {
                githubClient.Timeout = TimeSpan.FromSeconds(30);
                githubClient.DefaultRequestHeaders.UserAgent.TryParseAdd("request");
                githubClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", token);
                var httpContent = new StringContent(@"{""query"":""{    organization(login: \""homo-tw\"") {      repositories(affiliations: [OWNER], last: 10) {        edges {          node {            issues(states: [OPEN], last: 100) {              edges {                node { createdAt updatedAt title url number comments(first:100) { nodes { id createdAt author { login } } } assignees(first:20){ nodes { login }} projectItems(first: 10) {   nodes {     fieldValueByName(name: \""Status\"") {       ... on ProjectV2ItemFieldSingleSelectValue {         name       }     }   } }                }              }            }          }        }      }    }  }""}", System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage response;
                try
                {
                    response = await githubClient.PostAsync(url, httpContent, cancellationToken);
                }
                catch (Exception ex) when (ex is HttpRequestException || ex is System.Threading.Tasks.TaskCanceledException)
                {
                    // GitHub 偶爾會斷線 / timeout，這次跳過就好，下個整點會再跑一次
                    _logger.LogWarning(ex, "呼叫 GitHub GraphQL 失敗，本次略過");
                    return;
                }

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);

                    // 解析 JSON 回應
                    JObject graphqlResponse = JObject.Parse(jsonResponse);
                    var issues = graphqlResponse["data"]["organization"]["repositories"]["edges"][0]["node"]["issues"]["edges"].ToList<dynamic>().Select(item =>
                    {
                        var assignees = ((JArray)item["node"]["assignees"]["nodes"]).ToList<dynamic>().Select(item => (string)item["login"]);
                        return new
                        {
                            title = item["node"]["title"],
                            url = item["node"]["url"],
                            id = item["node"]["number"],
                            assignee = assignees.FirstOrDefault(),
                            status = item["node"]["projectItems"] != null && item["node"]["projectItems"]["nodes"] != null && item["node"]["projectItems"]["nodes"].Count > 0 && item["node"]["projectItems"]["nodes"][0]["fieldValueByName"] != null ? item["node"]["projectItems"]["nodes"][0]["fieldValueByName"]["name"] : null,
                            lastUpdate = item["node"]["updatedAt"],
                            lastCommentUsername = item["node"]["comments"] == null || item["node"]["comments"]["nodes"] == null || item["node"]["comments"]["nodes"].Count == 0 ? null : item["node"]["comments"]["nodes"][item["node"]["comments"]["nodes"].Count - 1]["author"]["login"],
                            lastCommentCreatedAt = item["node"]["comments"] == null || item["node"]["comments"]["nodes"] == null || item["node"]["comments"]["nodes"].Count == 0 ? null : item["node"]["comments"]["nodes"][item["node"]["comments"]["nodes"].Count - 1]["createdAt"]
                        };
                    }).ToList();

                    var githubIssueIds = issues.Select(item => (string)item.id).ToList();
                    var betTasks = TaskDataservice.GetAll(dbContext, (long)2, (long)6, null, null, githubIssueIds);

                    foreach (var issue in issues)
                    {
                        var matchedTask = betTasks.Where(task => task.ExternalId == (string)issue.id).FirstOrDefault();
                        if (matchedTask == null)
                        {
                            System.Console.WriteLine($"skip matched Task");
                            continue;
                        }
                        if (matchedTask.Assignee?.Username != issue.assignee && issue.assignee != null && issue.lastCommentUsername != null)
                        {
                            DateTime lastUpdateUtc;

                            // Newtonsoft 會把 updatedAt 轉成 DateTime，ToString() 之後只剩下 UTC 的時鐘時間、
                            // 沒有時區資訊，所以這裡要明確指定它是 UTC，再跟 DateTime.UtcNow 比才不會差 8 小時。
                            if (!DateTime.TryParse(issue.lastUpdate.ToString(), CultureInfo.CurrentCulture,
                                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out lastUpdateUtc))
                            {
                                continue;
                            }

                            DateTime nowUtc = DateTime.UtcNow;

                            // 週六日跳過檢查（以台北時間為準）
                            DateTime nowLocal = DateTime.Now;
                            if (nowLocal.DayOfWeek == DayOfWeek.Saturday || nowLocal.DayOfWeek == DayOfWeek.Sunday)
                            {
                                continue;
                            }

                            // 計算需要的小時數
                            double requiredHours = 24; // 預設 24 小時
                            if (lastUpdateUtc.ToLocalTime().DayOfWeek == DayOfWeek.Friday)
                            {
                                requiredHours = 96; // 週五延到下週二
                            }

                            if ((nowUtc - lastUpdateUtc).TotalHours < requiredHours)
                            {
                                continue;
                            }

                            var commentContent = new StringContent($@"{{""body"": ""{issue.assignee} 違規""}}", System.Text.Encoding.UTF8, "application/json");
                            try
                            {
                                // 一定要 await，之前沒 await 會在 HttpClient 被 dispose 後才送出，留言其實不會成功
                                var commentResponse = await githubClient.PostAsync($"https://api.github.com/repos/homo-tw/itemhub/issues/{issue.id}/comments", commentContent, cancellationToken);
                                if (!commentResponse.IsSuccessStatusCode)
                                {
                                    _logger.LogWarning($"issue #{issue.id} 留言失敗: {commentResponse.StatusCode}");
                                }
                            }
                            catch (Exception ex) when (ex is HttpRequestException || ex is System.Threading.Tasks.TaskCanceledException)
                            {
                                _logger.LogWarning(ex, $"issue #{issue.id} 留言失敗");
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"Failed to fetch issues: {response.StatusCode}");
                }
            }
        }

        public override System.Threading.Tasks.Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("GitHubAutoCommentViolationCronJob is stopping.");
            return base.StopAsync(cancellationToken);
        }
    }
}
