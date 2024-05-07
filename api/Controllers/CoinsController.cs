using Microsoft.AspNetCore.Mvc;

namespace Homo.Bet.Api
{
    [Route("v1")]
    [AuthorizeFactory]
    public class CoinsController : ControllerBase
    {

        private readonly BargainingChipDBContext _dbContext;
        public CoinsController(BargainingChipDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        [Route("coins/earn")]
        public dynamic getEarnCoins(DTOs.JwtExtraPayload extraPayload)
        {
            return new { Qty = CoinsLogDataService.GetEarnCoins(_dbContext, extraPayload.Id) };
        }

        [HttpGet]
        [Route("coins/bet")]
        public dynamic getBetCoins(DTOs.JwtExtraPayload extraPayload)
        {
            return new { Qty = CoinsLogDataService.GetBetCoins(_dbContext, extraPayload.Id) };
        }


    }
}
