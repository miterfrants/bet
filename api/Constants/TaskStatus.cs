using System.ComponentModel;
namespace Homo.Bet.Api
{
    public enum TASK_STATUS
    {
        [Description("PENDING")]
        PENDING,
        [Description("PROCESS")]
        PROCESS,
        [Description("BE_MARK_FINSIH")]
        BE_MARK_FINSIH,
        [Description("DONE")]
        DONE
    }
}