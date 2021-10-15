using System;
using Homo.Api;

namespace Homo.Bet.Api
{
    public partial class Task
    {
        public long Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public long CreatedBy { get; set; }
        public DateTime? EditedAt { get; set; }
        public long? EditedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public long ProjectId { get; set; }
        public virtual Project Project { get; set; }
        public string Name { get; set; }
        public TASK_TYPE Type { get; set; }
        public string ExternalId { get; set; }
        public long? AssigneeId { get; set; }
        public virtual User Assignee { get; set; }
        public DateTime? ExpectedFinishAt { get; set; }
        public TASK_STATUS Status { get; set; }
        public DateTime? MarkFinishAt { get; set; }
    }
}
