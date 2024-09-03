using System;
using System.Threading;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;


namespace Homo.Bet.Api
{
    public class GitHubIssuesNotificationCronJob : CronJobService
    {
        private readonly ILogger<GitHubIssuesNotificationCronJob> _logger;
        private BargainingChipDBContext _dbContext;
        private readonly string _envName;
        private Api.AppSettings _appSettings;

        public GitHubIssuesNotificationCronJob(IScheduleConfig<GitHubIssuesNotificationCronJob> config, ILogger<GitHubIssuesNotificationCronJob> logger, IServiceProvider serviceProvider, Microsoft.AspNetCore.Hosting.IWebHostEnvironment env, IOptions<AppSettings> appSettings)
            : base(config.CronExpression, config.TimeZoneInfo, serviceProvider)
        {
            _logger = logger;
            _envName = env.EnvironmentName;
            _appSettings = appSettings.Value;
        }

        public override System.Threading.Tasks.Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("GitHubIssuesNotificationCronJob starts.");
            return base.StartAsync(cancellationToken);
        }

        public override System.Threading.Tasks.Task DoWork(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{DateTime.Now:hh:mm:ss} is working.");
            // 取得現有 ItemHub 所有的 Issues 
            string token = _appSettings.Secrets.GitHubToken;
            string url = $"https://api.github.com/graphql";
            _dbContext = _serviceProvider.GetService<BargainingChipDBContext>();
            using (HttpClient githubClient = new HttpClient())
            using (HttpClient betClient = new HttpClient())
            {

                githubClient.DefaultRequestHeaders.UserAgent.TryParseAdd("request");
                githubClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", token);
                var httpContent = new StringContent(@"{""query"":""{    organization(login: \""homo-tw\"") {      repositories(affiliations: [OWNER], last: 10) {        edges {          node {            issues(states: [OPEN], last: 100) {              edges {                node { createdAt title url number assignees(first:20){ nodes { login }} projectItems(first: 10) {   nodes {     fieldValueByName(name: \""Status\"") {       ... on ProjectV2ItemFieldSingleSelectValue {         name       }     }   } }                }              }            }          }        }      }    }  }""}", System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage response = githubClient.PostAsync(url, httpContent).GetAwaiter().GetResult();
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

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
                            status = item["node"]["projectItems"]["nodes"].Count > 0 ? item["node"]["projectItems"]["nodes"][0]["fieldValueByName"]["name"] : null,
                        };
                    }).ToList();

                    var githubIssueIds = issues.Select(item => (string)item.id).ToList();
                    var betTasks = TaskDataservice.GetAll(_dbContext, (long)2, (long)6, null, null, githubIssueIds);

                    var unClaimIssues = issues.Where(issue =>
                    {
                        if (issue.assignee == null)
                        {
                            return false;
                        }
                        var matchedBetTask = betTasks.FirstOrDefault(task => task.ExternalId == (string)issue.id);
                        if (matchedBetTask == null)
                        {
                            return false;
                        }
                        return matchedBetTask.Assignee == null && issue.assignee != null;
                    }).ToList();
                    var asignees = issues.GroupBy(item => item.assignee).Select(item => item.Key).ToList();
                    System.Console.WriteLine($"testing:{Newtonsoft.Json.JsonConvert.SerializeObject(asignees, Newtonsoft.Json.Formatting.Indented)}");
                    asignees.ForEach(asignee =>
                    {
                        var asigneeUnClaimIssues = unClaimIssues.Where(issue => issue.assignee == asignee).ToList();
                        var unClaimMessage = asigneeUnClaimIssues.Count() > 0 ? $"\n\n未認領的 Issues: \n--------------------\n\n {string.Join("", asigneeUnClaimIssues.Select(issue => $"{issue.title} \n{issue.url} \n\n").ToList())}" : "";

                        var thisWeekIssues = issues.Where(item => item.assignee == asignee && item.status == "This Week").ToList();
                        var thisWeekMessage = thisWeekIssues.Count() > 0 ? $"\n\n這週待處理事項: \n--------------------\n\n {string.Join("", thisWeekIssues.Select(item => $"{item.title} \n{item.url} \n\n"))}" : "";

                        var diffAsigneeIssues = issues.Where(item =>
                        {
                            var matchedBetTask = betTasks.Where(task => task.ExternalId == (string)item.id).FirstOrDefault();
                            return item.assignee == asignee && item.assignee != matchedBetTask?.Assignee?.Username && matchedBetTask?.Assignee != null && item.assignee != null;
                        }).ToList();
                        var reviewMessage = diffAsigneeIssues.Count() > 0 ? $"\n\n需要 Review 的 Issues: \n--------------------\n\n{string.Join("", diffAsigneeIssues.Select(item => $"{item.title} \n{item.url} \n\n"))}" : "";

                        var inProgressIssues = issues.Where(item =>
                        {
                            var matchedBetTask = betTasks.Where(task => task.ExternalId == (string)item.id).FirstOrDefault();
                            if (matchedBetTask.Assignee?.Username != asignee)
                            {
                                return false;
                            }
                            return item.assignee == asignee && item.status == "In Progress";
                        }).ToList();
                        var inProgressMessage = inProgressIssues.Count() > 0 ? $"\n\n正在執行的任務: \n--------------------\n\n {string.Join("", inProgressIssues.Select(item => $"{item.title} \n{item.url} \n\n"))}" : "";

                        if (string.IsNullOrEmpty(unClaimMessage) && string.IsNullOrEmpty(thisWeekMessage) && string.IsNullOrEmpty(reviewMessage) && string.IsNullOrEmpty(inProgressMessage))
                        {
                            _logger.LogInformation(asignee);
                            return;
                        }

                        isRock.LineBot.Utility.PushMessage(_appSettings.Secrets.LineGroupId,
                            $"{asignee}{unClaimMessage}{thisWeekMessage}{reviewMessage}{inProgressMessage}"
                            , _appSettings.Secrets.LineToken);
                    });
                }
                else
                {
                    Console.WriteLine($"Failed to fetch issues: {response.StatusCode}");
                }
            }

            return System.Threading.Tasks.Task.CompletedTask;
        }

        public override System.Threading.Tasks.Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("GitHubIssuesNotificationCronJob is stopping.");
            return base.StopAsync(cancellationToken);
        }
    }
}
