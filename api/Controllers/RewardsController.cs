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

            return new { shareholding = RewardDataService.GetShareholding(_dbContext, extraPayload.Id) };
        }

        [HttpGet]
        [Route("coins-per-week")]
        public dynamic getCoinsPerWeek(DTOs.JwtExtraPayload extraPayload)
        {

            return new { coinsPerWeek = RewardDataService.GetRewardCoinPerWeek(_dbContext, extraPayload.Id) };
        }

    }
}
