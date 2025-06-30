using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Homo.Bet.Api
{
    public class RewardDataService
    {
        public static Reward Create(BargainingChipDBContext dbContext, long ownerId, REWARD_TYPE type, int qty, DateTime? LeaveDate = null)
        {
            Reward record = new Reward();
            record.Type = type;
            record.Qty = qty;
            record.CreatedAt = System.DateTime.Now;
            record.CreatedBy = ownerId;
            record.LeaveDate = LeaveDate;
            dbContext.Reward.Add(record);
            dbContext.SaveChanges();
            return record;
        }

        public static int GetRewardCoinPerWeek(BargainingChipDBContext dbContext, long ownerId)
        {
            return dbContext.Reward.Where(item => item.DeletedAt == null && item.CreatedBy == ownerId && item.Type == REWARD_TYPE.COIN_PER_WEEK).Sum(item => item.Qty);
        }

        public static int GetAllStock(BargainingChipDBContext dbContext)
        {
            return dbContext.Reward.Where(item => item.DeletedAt == null && item.Type == REWARD_TYPE.STOCK).Sum(item => item.Qty);
        }

        public static int GetMyStock(BargainingChipDBContext dbContext, long ownerId)
        {
            return dbContext.Reward.Where(item => item.DeletedAt == null && item.CreatedBy == ownerId && item.Type == REWARD_TYPE.STOCK).Sum(item => item.Qty);
        }

        public static int GetSickLeaves(BargainingChipDBContext dbContext, long ownerId, DateTime LeaveDate)
        {
            var now = LeaveDate;
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var nextMonthStart = monthStart.AddMonths(1);
            return dbContext.Reward.Where(item =>
                item.DeletedAt == null
                && item.CreatedBy == ownerId
                && item.Type == REWARD_TYPE.SICK_LEAVE
                && item.LeaveDate >= monthStart
                && item.LeaveDate < nextMonthStart
            ).Sum(item => item.Qty);
        }

        public static int GetLeaves(BargainingChipDBContext dbContext, long ownerId, DateTime LeaveDate, List<REWARD_TYPE> RewardTypes)
        {
            var now = LeaveDate;
            var monthStart = new DateTime(now.Year, now.Month, now.Day);
            var nextMonthStart = monthStart.AddDays(1);
            return dbContext.Reward.Where(item =>
                item.DeletedAt == null
                && item.CreatedBy == ownerId
                && (RewardTypes == null || RewardTypes.Contains(item.Type))
                && item.LeaveDate >= monthStart
                && item.LeaveDate < nextMonthStart
            ).Sum(item => item.Qty);
        }

        public static int GetMenstruationLeaves(BargainingChipDBContext dbContext, long ownerId, DateTime LeaveDate)
        {
            var now = LeaveDate;
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var nextMonthStart = monthStart.AddMonths(1);
            return dbContext.Reward.Where(item =>
                item.DeletedAt == null
                && item.CreatedBy == ownerId
                && item.Type == REWARD_TYPE.MENSTRUATION_LEAVE
                && item.LeaveDate >= monthStart
                && item.LeaveDate < nextMonthStart
            ).Sum(item => item.Qty);
        }

    }

}
