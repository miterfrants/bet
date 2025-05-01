using Microsoft.AspNetCore.Mvc;

namespace Homo.Bet.Api
{
    [Route("v1/rewards")]
    [AuthorizeFactory]
    public class RewardsController : ControllerBase
    {

        private readonly BargainingChipDBContext _dbContext;
        public RewardsController(BargainingChipDBContext dbContext)
        {
            _dbContext = dbContext;
        }


        [HttpGet]
        [Route("shareholding")]
        public dynamic getShareholding(DTOs.JwtExtraPayload extraPayload)
        {

            return new { all = RewardDataService.GetAllStock(_dbContext), mine = RewardDataService.GetMyStock(_dbContext, extraPayload.Id) };
        }

        [HttpGet]
        [Route("coins-per-week")]
        public dynamic getCoinsPerWeek(DTOs.JwtExtraPayload extraPayload)
        {

            return new { coinsPerWeek = RewardDataService.GetRewardCoinPerWeek(_dbContext, extraPayload.Id) };
        }


        [HttpGet]
        [Route("this-month-sick-leave-days")]
        public dynamic getSickLeaveDays(DTOs.JwtExtraPayload extraPayload)
        {

            return new { days = RewardDataService.GetSickLeaves(_dbContext, extraPayload.Id, System.DateTime.Now) };
        }



        [HttpGet]
        [Route("this-month-menstruation-leave-days")]
        public dynamic getMenstruationLeaveDays(DTOs.JwtExtraPayload extraPayload)
        {

            return new { days = RewardDataService.GetMenstruationLeaves(_dbContext, extraPayload.Id, System.DateTime.Now) };
        }

    }
}
