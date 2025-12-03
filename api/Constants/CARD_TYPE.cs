using System.ComponentModel;

namespace Homo.Bet.Api
{
    public enum CARD_TYPE
    {
        [Description("魔法卡")]
        MAGIC,

        [Description("陷阱卡")]
        TRAP,

        [Description("增益卡")]
        BUFF
    }
}
