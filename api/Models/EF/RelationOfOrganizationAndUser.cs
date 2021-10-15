using System;
using Homo.Bet.Api;

namespace Homo.Bet.Api
{
    public partial class RelationOfOrganizationAndUser
    {
        public long Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public long CreatedBy { get; set; }
        public DateTime? EditedAt { get; set; }
        public long? EditedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public long UserId { get; set; }
        public long OrganizationId { get; set; }

        public User User { get; set; }
        public Organization Organization { get; set; }
    }
}
