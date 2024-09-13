using System;
using System.Threading;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Linq;
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

        public override System.Threading.Tasks.Task DoWork(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{DateTime.Now:hh:mm:ss} is working.");
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
                githubClient.DefaultRequestHeaders.UserAgent.TryParseAdd("request");
                githubClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", token);
                var httpContent = new StringContent(@"{""query"":""{    organization(login: \""homo-tw\"") {      repositories(affiliations: [OWNER], last: 10) {        edges {          node {            issues(states: [OPEN], last: 100) {              edges {                node { createdAt updatedAt title url number comments(first:100) { nodes { id createdAt author { login } } } assignees(first:20){ nodes { login }} projectItems(first: 10) {   nodes {     fieldValueByName(name: \""Status\"") {       ... on ProjectV2ItemFieldSingleSelectValue {         name       }     }   } }                }              }            }          }        }      }    }  }""}", System.Text.Encoding.UTF8, "application/json");
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
                            lastUpdate = item["node"]["updatedAt"],
                            lastCommentUsername = item["node"]["comments"]["nodes"].Count == 0 ? null : item["node"]["comments"]["nodes"][item["node"]["comments"]["nodes"].Count - 1]["author"]["login"],
                            lastCommentCreatedAt = item["node"]["comments"]["nodes"].Count == 0 ? null : item["node"]["comments"]["nodes"][item["node"]["comments"]["nodes"].Count - 1]["createdAt"]
                        };
                    }).ToList();

                    var githubIssueIds = issues.Select(item => (string)item.id).ToList();
                    var betTasks = TaskDataservice.GetAll(dbContext, (long)2, (long)6, null, null, githubIssueIds);

                    issues.ForEach(issue =>
                    {
                        var matchedTask = betTasks.Where(task => task.ExternalId == (string)issue.id).FirstOrDefault();
                        if (matchedTask == null)
                        {
                            return;
                        }
                        if (matchedTask.Assignee?.Username != issue.assignee && issue.assignee != null && issue.lastCommentUsername != null)
                        {
                            DateTime lastUpdateDateTime;

                            if (!DateTime.TryParse(issue.lastUpdate.ToString().Replace("Z", ""), out lastUpdateDateTime))
                            {
                                return;
                            }
                            if ((DateTime.Now - lastUpdateDateTime).TotalHours < ((int)lastUpdateDateTime.DayOfWeek > 0 && (int)lastUpdateDateTime.DayOfWeek < 5 ? 24 : (int)lastUpdateDateTime.DayOfWeek == 5 ? 96 : (int)lastUpdateDateTime.DayOfWeek == 6 ? 72 : 48))
                            {
                                System.Console.WriteLine($"testing:{Newtonsoft.Json.JsonConvert.SerializeObject((DateTime.Now - lastUpdateDateTime).TotalHours, Newtonsoft.Json.Formatting.Indented)}");
                                System.Console.WriteLine($"testing:{Newtonsoft.Json.JsonConvert.SerializeObject(((int)lastUpdateDateTime.DayOfWeek > 0 && (int)lastUpdateDateTime.DayOfWeek < 5 ? 24 : (int)lastUpdateDateTime.DayOfWeek == 5 ? 96 : (int)lastUpdateDateTime.DayOfWeek == 6 ? 72 : 48), Newtonsoft.Json.Formatting.Indented)}");
                                return;
                            }

                            httpContent = new StringContent($@"{{""body"": ""{issue.assignee} 違規""}}", System.Text.Encoding.UTF8, "application/json");
                            var response = githubClient.PostAsync($"https://api.github.com/repos/homo-tw/itemhub/issues/{issue.id}/comments", httpContent);
                            System.Console.WriteLine(response.GetAwaiter().GetResult().Content.ReadAsStringAsync().GetAwaiter().GetResult());
                        }
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
            _logger.LogInformation("GitHubAutoCommentViolationCronJob is stopping.");
            return base.StopAsync(cancellationToken);
        }
    }
}
