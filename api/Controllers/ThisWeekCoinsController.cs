using Microsoft.AspNetCore.Mvc;
using Homo.Core.Constants;
using System.Linq;
namespace Homo.Bet.Api
{
    [Route("v1")]
    public class ThisWeekCoinsController : ControllerBase
    {

        private readonly BargainingChipDBContext _dbContext;
        public ThisWeekCoinsController(BargainingChipDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        [Route("this-week-coins/earn")]
        public dynamic getEarnCoins(DTOs.JwtExtraPayload extraPayload)
        {
            return CoinsLogDataService.GetAllEarnCoinsThisWeek(_dbContext).ToList().GroupBy(item => item.OwnerId).Select(item => new
            {
                OwnerId = item.Key,
                Coins = item.Sum(x => x.Qty)
            });
        }

    }
}
