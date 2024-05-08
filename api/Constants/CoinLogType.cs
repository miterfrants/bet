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
        TRANSFER_TO
    }
}