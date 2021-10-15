using System;
using Homo.Api;

namespace Homo.Bet.Api
{
    public partial class Project
    {
        public long Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public long CreatedBy { get; set; }
        public DateTime? EditedAt { get; set; }
        public long? EditedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public long OwnerId { get; set; }
        public long OrganizationId { get; set; }
        public Organization Organization { get; set; }
        public string Name { get; set; }
    }
}
