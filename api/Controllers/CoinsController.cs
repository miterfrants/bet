using Microsoft.AspNetCore.Mvc;

namespace Homo.Bet.Api
{
    [Route("v1/coins")]
    [AuthorizeFactory]
    public class CoinsController : ControllerBase
    {

        private readonly BargainingChipDBContext _dbContext;
        public CoinsController(BargainingChipDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        [Route("earn")]
        public dynamic getEarnCoins(DTOs.JwtExtraPayload extraPayload)
        {
            return new { Qty = CoinsLogDataService.GetEarnCoins(_dbContext, extraPayload.Id) };
        }

        [HttpGet]
        [Route("bet")]
        public dynamic getBetCoins(DTOs.JwtExtraPayload extraPayload)
        {
            return new { Qty = CoinsLogDataService.GetBetCoins(_dbContext, extraPayload.Id) };
        }
    }
}
