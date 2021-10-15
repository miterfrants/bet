using System.ComponentModel;
namespace Homo.Bet.Api
{
    public enum ROLE
    {
        [Description("Administrator")]
        ADMIN,
        [Description("User Manage")]
        USER,
        [Description("Only Sign In")]
        NO,
        [Description("Group Manage")]
        GROUP
    }
}