using System;
using Homo.Api;

namespace Homo.Bet.Api
{
    public partial class Reward
    {
        public long Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public long CreatedBy { get; set; }
        public DateTime? EditedAt { get; set; }
        public long? EditedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public REWARD_TYPE Type { get; set; }
        public int Qty { get; set; }
        public DateTime? LeaveDate { get; set; }
    }
}
