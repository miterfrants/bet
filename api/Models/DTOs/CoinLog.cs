using System;

namespace Homo.Bet.Api
{
    public abstract partial class DTOs
    {
        public partial class CoinLog : DTOs
        {
            public int Qty { get; set; }
        }
    }
}
