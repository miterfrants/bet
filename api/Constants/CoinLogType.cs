using System.ComponentModel;
namespace Homo.Bet.Api
{
    public enum COIN_LOG_TYPE
    {
        [Description("Earn")]
        EARN,
        [Description("Bet")]
        BET,
        [Description("Buy")]
        BUY,
        [Description("Transfer To")]
        TRANSFER_TO,
        [Description("工時不足懲罰")]
        PUNISHMENT_FOR_INSUFFICIENT_WORKING_HOURS
    }
}