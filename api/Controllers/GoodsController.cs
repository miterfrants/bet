using System.Runtime.InteropServices;
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
            else if (dto.Name == "每週取得")
            {
                RewardDataService.Create(_dbContext, extraPayload.Id, REWARD_TYPE.COIN_PER_WEEK, dto.Value);
                cost = 100;
            }
            else if (dto.Name == "病假")
            {
                var days = RewardDataService.GetSickLeaves(_dbContext, extraPayload.Id, dto.LeaveDate ?? System.DateTime.Now);
                System.Console.WriteLine($"testing:{days}");
                dto.Value = 1;
                if (days < 2) // 一個月可以無償請兩次病假
                {
                    cost = 0;
                }
                else
                {
                    cost = 1;
                }
                RewardDataService.Create(_dbContext, extraPayload.Id, REWARD_TYPE.SICK_LEAVE, 1, dto.LeaveDate ?? System.DateTime.Now);
            }
            else if (dto.Name == "事假")
            {
                RewardDataService.Create(_dbContext, extraPayload.Id, REWARD_TYPE.LEAVE, 1, dto.LeaveDate);
                cost = 1;
                dto.Value = 1;

            }
            else if (dto.Name == "生理假")
            {
                RewardDataService.Create(_dbContext, extraPayload.Id, REWARD_TYPE.MENSTRUATION_LEAVE, 1, dto.LeaveDate ?? System.DateTime.Now);
                var days = RewardDataService.GetMenstruationLeaves(_dbContext, extraPayload.Id, dto.LeaveDate ?? System.DateTime.Now);
                dto.Value = 1;
                if (days <= 1) // 一個月可以無償請一次生理假
                {
                    cost = 0;
                }
                else
                {
                    cost = 1;
                }
            }
            CoinsLogDataService.Create(_dbContext, extraPayload.Id, null, extraPayload.Id, COIN_LOG_TYPE.BUY, new DTOs.CoinLog() { Qty = cost * dto.Value });
            return new { status = Homo.Core.Constants.CUSTOM_RESPONSE.OK };
        }

    }
}
