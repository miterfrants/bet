using System;

namespace Homo.Bet.Api
{
    public abstract partial class DTOs
    {
        public partial class AddToGithubProject : DTOs
        {
            public string ProjectId { get; set; }
            public string IssueId { get; set; }
            public string OriginalProjectId { get; set; }
            public string OriginalConnectionId { get; set; }
        }

        public partial class UpdateGithubIssuesStatus : DTOs
        {
            public string StatusFieldId { get; set; }
            public string ConnectionId { get; set; }
            public string ProjectId { get; set; }
            public string StatusId { get; set; }
        }
    }
}
