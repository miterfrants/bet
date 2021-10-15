using System;

namespace Homo.Bet.Api
{
    public abstract partial class DTOs
    {
        public partial class Project : DTOs
        {
            public string Name { get; set; }
        }

        public partial class ProjectEngagement : DTOs
        {
            public long UserId { get; set; }
        }
    }
}
