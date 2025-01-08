using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace Homo.Bet.Api
{
    [AuthorizeFactory]
    [Route("v1/github-projects")]
    public class GithubProjectsController : ControllerBase
    {
        private readonly string _githubToken;
        public GithubProjectsController(IOptions<AppSettings> appSettings)
        {
            _githubToken = appSettings.Value.Secrets.GitHubToken;
        }

        [HttpGet]
        public async Task<dynamic> GetList(Homo.Bet.Api.DTOs.JwtExtraPayload extraPayload)
        {
            List<GithubProject> result = new List<GithubProject>();
            using (HttpClient githubClient = new HttpClient())
            {
                githubClient.DefaultRequestHeaders.UserAgent.TryParseAdd("request");
                githubClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", _githubToken);
                var httpContent = new StringContent(@"{""query"":""{    organization(login: \""homo-tw\"") { projectsV2(first:20) { nodes{ id, number, url, title, closed, field(name:\""Status\"") { ... on ProjectV2SingleSelectField { id, options { name, id } } } } } }  }""}", System.Text.Encoding.UTF8, "application/json");
                string url = $"https://api.github.com/graphql";
                HttpResponseMessage response = await githubClient.PostAsync(url, httpContent);
                var responseBody = await response.Content.ReadAsStringAsync();
                JObject graphqlResponse = JObject.Parse(responseBody);
                var projects = graphqlResponse["data"]["organization"]["projectsV2"]["nodes"].Where(item => (bool)item["closed"] == false).ToList<dynamic>().Select(item =>
                    {
                        List<dynamic> rawStatus = ((JArray)item["field"]["options"]).ToList<dynamic>();
                        string statusFieldId = item["field"]["id"].ToString();
                        var status = rawStatus.Select(option =>
                            {
                                return new GithubStatus
                                {
                                    Name = option["name"],
                                    Id = option["id"]
                                };
                            }).ToList<GithubStatus>();
                        return new GithubProject
                        {
                            Name = item["title"],
                            Id = item["id"],
                            Status = status,
			    StatusFieldId = statusFieldId
                        };
                    }).ToList(); ;
                result.AddRange(projects);
            }

            return result;
        }

        [HttpPost]
        [Route("add-to-project")]
        public async Task<dynamic> AddTo([FromBody] DTOs.AddToGithubProject dto, Homo.Bet.Api.DTOs.JwtExtraPayload extraPayload)
        {
            var result = new JObject();
            using (HttpClient githubClient = new HttpClient())
            {
                githubClient.DefaultRequestHeaders.UserAgent.TryParseAdd("request");
                githubClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", _githubToken);
                string url = $"https://api.github.com/graphql";

                var httpContent = new StringContent(@$"{{""query"":""mutation {{ deleteProjectV2Item (input: {{ projectId: \""{dto.OriginalProjectId}\"" itemId: \""{dto.OriginalConnectionId}\""}}){{ deletedItemId }} }}""}}", System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage response = await githubClient.PostAsync(url, httpContent);
                var testResponse = await response.Content.ReadAsStringAsync();

                httpContent = new StringContent(@$"{{""query"":""mutation {{ addProjectV2ItemById (input: {{ projectId: \""{dto.ProjectId}\"" contentId: \""{dto.IssueId}\""}}) {{item {{ id }} }}}}""}}", System.Text.Encoding.UTF8, "application/json");
                response = await githubClient.PostAsync(url, httpContent);

                var responseBody = await response.Content.ReadAsStringAsync();
		System.Console.WriteLine($"testing:{Newtonsoft.Json.JsonConvert.SerializeObject(responseBody, Newtonsoft.Json.Formatting.Indented)}");
                result = JObject.Parse(responseBody);
  	    }
            return new {ConnectionId = result["data"]["addProjectV2ItemById"]["item"]["id"].ToString()};
        }

        [HttpPost]
        [Route("update-status")]
        public async Task<dynamic> UpdateStatus([FromBody] DTOs.UpdateGithubIssuesStatus dto)
        {
            var result = new JObject();
            using (HttpClient githubClient = new HttpClient())
            {
                githubClient.DefaultRequestHeaders.UserAgent.TryParseAdd("request");
                githubClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", _githubToken);
                var httpContent = new StringContent(@$"{{""query"":""mutation {{ updateProjectV2ItemFieldValue (input: {{ projectId: \""{dto.ProjectId}\"", itemId: \""{dto.ConnectionId}\"", fieldId: \""{dto.StatusFieldId}\"", value: {{singleSelectOptionId: \""{dto.StatusId}\""}}  }}) {{ clientMutationId }}}}""}}", System.Text.Encoding.UTF8, "application/json");
                string url = $"https://api.github.com/graphql";
                HttpResponseMessage response = await githubClient.PostAsync(url, httpContent);
                var responseBody = await response.Content.ReadAsStringAsync();
                result = JObject.Parse(responseBody);
            }
            return result;
        }
    }
}

public class GithubProject
{
    public string Name { get; set; }
    public string Id { get; set; }
    public List<GithubStatus> Status { get; set; }
    public string StatusFieldId { get; set; }
}

public class GithubStatus
{
    public string Name { get; set; }
    public string Id { get; set; }
}
