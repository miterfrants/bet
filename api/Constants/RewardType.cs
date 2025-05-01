using System.ComponentModel;
namespace Homo.Bet.Api
{
    public enum REWARD_TYPE
    {
        [Description("Stock")]
        STOCK,
        [Description("CoinPerWeek")]
        COIN_PER_WEEK,
        [Description("病假")]
        SICK_LEAVE,
        [Description("事假")]
        LEAVE,
        [Description("生理假")]
        MENSTRUATION_LEAVE,

    }
}