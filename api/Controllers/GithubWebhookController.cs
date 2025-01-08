using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;

namespace Homo.Bet.Api
{
    [Route("v1/github-webhook")]
    public class GithubWebhookController : ControllerBase
    {
        private readonly string _githubToken;
        private readonly BargainingChipDBContext _dbContext;
        private readonly Dictionary<string, int> _defaultCoinMapping = new Dictionary<string, int>() { { "planning", 4 }, { "plan", 4 }, { "r&d", 10 }, { "rd", 10 }, { "develop", 5 }, { "marketing", 3 }, { "operation", 3 }, { "bd", 10 }, { "bug", 2 } };
        public GithubWebhookController(BargainingChipDBContext dbContext, IOptions<AppSettings> appSettings)
        {
            _dbContext = dbContext;
            _githubToken = appSettings.Value.Secrets.GitHubToken;
        }

        [HttpPost]
        public ActionResult<dynamic> Webhook([FromBody] GithubIssueWebhook body)
        {
            if (body.action != "opened")
            {
                return new { };
            }
            var newTasks = TaskDataservice.BatchCreate(_dbContext, 6, 5, new List<DTOs.Task>{new DTOs.Task(){
                Name = "",
                Type = TASK_TYPE.GITHUB,
                ExternalId = body.issue.number.ToString()
            }});

            int defaultCoin = 0;
            var title = body.issue.title.ToLower();
            if (title.StartsWith("planning") || title.StartsWith("plan"))
            {
                defaultCoin = _defaultCoinMapping.Where(item => item.Key == "planning").FirstOrDefault().Value;
            }
            else if (title.StartsWith("r&d") || title.StartsWith("rd"))
            {
                defaultCoin = _defaultCoinMapping.Where(item => item.Key == "r&d").FirstOrDefault().Value;
            }
            else if (title.StartsWith("develop") || title.StartsWith("feat") || title.StartsWith("feature"))
            {
                defaultCoin = _defaultCoinMapping.Where(item => item.Key == "develop").FirstOrDefault().Value;
            }
            else if (title.StartsWith("operation") || title.StartsWith("oper"))
            {
                defaultCoin = _defaultCoinMapping.Where(item => item.Key == "operation").FirstOrDefault().Value;
            }
            else if (title.StartsWith("bd"))
            {
                defaultCoin = _defaultCoinMapping.Where(item => item.Key == "bd").FirstOrDefault().Value;
            }
            else if (title.StartsWith("bug"))
            {
                defaultCoin = _defaultCoinMapping.Where(item => item.Key == "bug").FirstOrDefault().Value;
            }
            else if (title.StartsWith("marketing"))
            {
                defaultCoin = _defaultCoinMapping.Where(item => item.Key == "marketing").FirstOrDefault().Value;
            }

            CoinsLogDataService.Create(_dbContext, 7, newTasks[0].Id, 7, COIN_LOG_TYPE.BET, new DTOs.CoinLog()
            {
                Qty = -defaultCoin
            });
            return new { };
        }

    }
}

public class GithubIssueWebhook
{
    public string action { get; set; }
    public GithubIssue issue { get; set; }
}

public class GithubIssue
{
    public int number { get; set; }
    public string title { get; set; }
}