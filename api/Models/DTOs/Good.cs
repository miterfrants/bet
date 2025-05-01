using System;
using Homo.Api;

namespace Homo.Bet.Api
{
    public abstract partial class DTOs
    {
        public partial class Good : DTOs
        {
            public string Name { get; set; }
            public int Value { get; set; }
            public DateTime? LeaveDate { get; set; }
        }
    }
}
