using System;
using Homo.Api;

namespace Homo.Bet.Api
{
    public abstract partial class DTOs
    {
        public partial class Transfer : DTOs
        {
            public long ReceiverId { get; set; }
            public int Qty { get; set; }
        }
    }
}
