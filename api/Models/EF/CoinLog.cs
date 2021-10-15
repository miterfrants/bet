using System;
using Homo.Api;

namespace Homo.Bet.Api
{
    public partial class CoinLog
    {
        public long Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public long CreatedBy { get; set; }
        public DateTime? EditedAt { get; set; }
        public long? EditedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public long OwnerId { get; set; }
        public long? TaskId { get; set; }
        public Task Task { get; set; }
        public int Qty { get; set; }
        public COIN_LOG_TYPE Type { get; set; }
        public bool IsLock { get; set; }
    }
}
