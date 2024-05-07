using Microsoft.AspNetCore.Mvc;

namespace Homo.Bet.Api
{
    [Route("v1/goods")]
    [AuthorizeFactory]
    public class GoodsController : ControllerBase
    {

        private readonly BargainingChipDBContext _dbContext;
        public GoodsController(BargainingChipDBContext dbContext)
        {
            _dbContext = dbContext;
        }


        [HttpPost]
        [Route("buy")]
        public dynamic buy(DTOs.JwtExtraPayload extraPayload, [FromBody] DTOs.Good dto)
        {
            var cost = 1;
            if (dto.Name == "股票")
            {
                cost = 10;
                RewardDataService.Create(_dbContext, extraPayload.Id, REWARD_TYPE.STOCK, dto.Value);
            }
            else if (dto.Name == "假期")
            {
                cost = 50;
            }
            else if (dto.Name == "每週取得")
            {
                RewardDataService.Create(_dbContext, extraPayload.Id, REWARD_TYPE.COIN_PER_WEEK, dto.Value);
                cost = 100;
            }
            CoinsLogDataService.Create(_dbContext, extraPayload.Id, null, extraPayload.Id, COIN_LOG_TYPE.BUY, new DTOs.CoinLog() { Qty = cost * dto.Value });
            return new { status = Homo.Core.Constants.CUSTOM_RESPONSE.OK };
        }

    }
}
