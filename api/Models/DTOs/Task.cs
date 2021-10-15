using System;
using Homo.Api;

namespace Homo.Bet.Api
{
    public abstract partial class DTOs
    {
        public partial class Task : DTOs
        {
            public string Name { get; set; }
            public TASK_TYPE Type { get; set; }
            public string ExternalId { get; set; }
        }

        public partial class TaskWorkDays : DTOs
        {
            public int WorkDays { get; set; }
        }
    }
}
