using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Homo.Bet.Api
{
    public class RewardDataService
    {
        public static Reward Create(BargainingChipDBContext dbContext, long ownerId, REWARD_TYPE type, int qty)
        {
            Reward record = new Reward();
            record.Type = type;
            record.Qty = qty;
            record.CreatedAt = System.DateTime.Now;
            record.CreatedBy = ownerId;
            dbContext.Reward.Add(record);
            dbContext.SaveChanges();
            return record;
        }

        public static int GetRewardCoinPerWeek(BargainingChipDBContext dbContext, long ownerId)
        {
            return dbContext.Reward.Where(item => item.DeletedAt == null && item.CreatedBy == ownerId && item.Type == REWARD_TYPE.COIN_PER_WEEK).Sum(item => item.Qty);
        }

        public static decimal GetShareholding(BargainingChipDBContext dbContext, long ownerId)
        {
            int totalBuyStock = dbContext.Reward.Where(item => item.DeletedAt == null && item.Type == REWARD_TYPE.STOCK).Sum(item => item.Qty);
            return dbContext.Reward.Where(item => item.DeletedAt == null && item.CreatedBy == ownerId && item.Type == REWARD_TYPE.STOCK).Sum(item => item.Qty) / totalBuyStock;
        }

    }

}
