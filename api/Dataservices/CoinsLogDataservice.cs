using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Homo.Bet.Api
{
    public class CoinsLogDataService
    {
        public static int GetEarnCoins(BargainingChipDBContext dbContext, long userId)
        {
            return dbContext.CoinLog
                .Where(x =>
                    x.DeletedAt == null
                    && x.OwnerId == userId
                    && (x.Type == COIN_LOG_TYPE.EARN || x.Type == COIN_LOG_TYPE.BUY || x.Type == COIN_LOG_TYPE.TRANSFER_TO)
                )
                .GroupBy(x => new { x.OwnerId, x.Type })
                .Select(g => new
                {
                    g.Key.OwnerId,
                    g.Key.Type,
                    Qty = g.Sum(x => x.Qty)
                })
                .Sum(x => x.Type == COIN_LOG_TYPE.EARN ? x.Qty : -x.Qty);
        }

        public static int GetBetCoins(BargainingChipDBContext dbContext, long userId)
        {
            return dbContext.CoinLog
                .Where(x =>
                    x.DeletedAt == null
                    && x.OwnerId == userId
                    && x.Type == COIN_LOG_TYPE.BET
                )
                .GroupBy(x => x.OwnerId)
                .Select(g => new
                {
                    g.Key,
                    Qty = g.Sum(x => x.Qty)
                })
                .Sum(x => x.Qty);
        }

        public static List<UserCoinLogBalance> GetFreeBetUsers(BargainingChipDBContext dbContext)
        {
            return dbContext.CoinLog
                .Where(x =>
                    x.DeletedAt == null
                    && x.Type == COIN_LOG_TYPE.BET
                )
                .GroupBy(x => x.OwnerId)
                .Select(g => new UserCoinLogBalance()
                {
                    OwnerId = g.Key,
                    Qty = g.Sum(x => x.Qty)
                }).ToList();
        }

        public static CoinLog GetFreeOneByTaskIdAndOwnerId(BargainingChipDBContext dbContext, long taskId, long ownerId, bool asNoTracking = false)
        {
            IQueryable<CoinLog> dbSet;
            if (asNoTracking)
            {
                dbSet = dbContext.CoinLog.AsNoTracking();
            }
            else
            {
                dbSet = dbContext.CoinLog;
            }
            return dbSet
                .Where(x =>
                    x.DeletedAt == null
                    && x.TaskId == taskId
                    && x.Type == COIN_LOG_TYPE.BET
                    && x.OwnerId == ownerId
                    && x.IsLock != true
                )
                .FirstOrDefault();
        }

        public static int GetTaskBetCoins(BargainingChipDBContext dbContext, long taskId)
        {
            return dbContext.CoinLog
                .Where(x =>
                    x.DeletedAt == null
                    && x.TaskId == taskId
                    && x.Type == COIN_LOG_TYPE.BET
                )
                .GroupBy(x => x.TaskId)
                .Select(g => new
                {
                    g.Key,
                    Qty = g.Sum(x => x.Qty)
                })
                .Sum(x => x.Qty);
        }

        public static int GetTaskOwnerLockedBetCoins(BargainingChipDBContext dbContext, long taskId, long ownerId)
        {
            return dbContext.CoinLog
                .Where(x =>
                    x.DeletedAt == null
                    && x.TaskId == taskId
                    && x.OwnerId == ownerId
                    && x.Type == COIN_LOG_TYPE.BET
                    && x.IsLock == true
                )
                .GroupBy(x => x.TaskId)
                .Select(g => new
                {
                    g.Key,
                    Qty = g.Sum(x => x.Qty)
                })
                .Sum(x => x.Qty);
        }

        public static int GetTaskOwnerFreeBetCoins(BargainingChipDBContext dbContext, long taskId, long ownerId)
        {
            return dbContext.CoinLog
                .Where(x =>
                    x.DeletedAt == null
                    && x.TaskId == taskId
                    && x.OwnerId == ownerId
                    && x.Type == COIN_LOG_TYPE.BET
                    && x.IsLock == false
                )
                .GroupBy(x => x.TaskId)
                .Select(g => new
                {
                    g.Key,
                    Qty = g.Sum(x => x.Qty)
                })
                .Sum(x => x.Qty);
        }

        public static List<CoinLog> GetAll(BargainingChipDBContext dbContext, long taskId, long ownerId, COIN_LOG_TYPE type)
        {
            return dbContext.CoinLog
                .Where(x =>
                    x.DeletedAt == null
                    && x.TaskId == taskId
                    && x.OwnerId == ownerId
                    && x.Type == type
                    && x.IsLock == false
                )
                .ToList();
        }

        public static CoinLog Create(BargainingChipDBContext dbContext, long ownerId, long? taskId, long createdBy, COIN_LOG_TYPE type, DTOs.CoinLog dto)
        {
            CoinLog record = new CoinLog();
            foreach (var propOfDTO in dto.GetType().GetProperties())
            {
                var value = propOfDTO.GetValue(dto);
                var prop = record.GetType().GetProperty(propOfDTO.Name);
                prop.SetValue(record, value);
            }
            record.TaskId = taskId;
            record.CreatedBy = createdBy;
            record.CreatedAt = DateTime.Now;
            record.OwnerId = ownerId;
            record.Type = type;
            dbContext.CoinLog.Add(record);
            dbContext.SaveChanges();
            return record;
        }

        public static void Update(BargainingChipDBContext dbContext, CoinLog log, long editedBy, DTOs.CoinLog dto)
        {
            log.Qty = dto.Qty;
            log.EditedAt = DateTime.Now;
            log.EditedBy = editedBy;
            dbContext.SaveChanges();
        }

        public static void ClearAll(BargainingChipDBContext dbContext)
        {
            List<UserCoinLogBalance> logs = dbContext.CoinLog
                .Where(x =>
                    x.DeletedAt == null
                    && x.Type == COIN_LOG_TYPE.BET
                ).GroupBy(x => x.OwnerId)
                .Select(g => new
                UserCoinLogBalance()
                {
                    OwnerId = g.Key,
                    Qty = g.Sum(x => x.Qty)
                }).ToList<UserCoinLogBalance>();
            logs.ForEach(item =>
            {
                if (item.Qty > 0)
                {
                    dbContext.CoinLog.Add(new CoinLog()
                    {
                        CreatedAt = DateTime.Now,
                        CreatedBy = 0,
                        Type = COIN_LOG_TYPE.BET,
                        Qty = -item.Qty,
                        OwnerId = item.OwnerId
                    });
                }

            });
            dbContext.SaveChanges();
        }
        public static void LockBetted(BargainingChipDBContext dbContext)
        {
            dbContext.CoinLog.Where(x => x.DeletedAt == null && x.IsLock != true).ToList<CoinLog>().ForEach(item =>
            {
                item.IsLock = true;
            });
            dbContext.SaveChanges();
        }

        public static void Give(BargainingChipDBContext dbContext, List<long> ownerIds, int qty)
        {
            for (int i = 0; i < ownerIds.Count(); i++)
            {
                CoinLog log = new CoinLog()
                {
                    Type = COIN_LOG_TYPE.BET,
                    OwnerId = ownerIds[i],
                    CreatedAt = DateTime.Now,
                    CreatedBy = 0,
                    Qty = qty
                };
                dbContext.CoinLog.Add(log);
            }
            dbContext.SaveChanges();
        }
        public static void Transfer(BargainingChipDBContext dbContext, long ownerId, long receiverId, int qty)
        {
            CoinLog from = new CoinLog()
            {
                Type = COIN_LOG_TYPE.TRANSFER_TO,
                OwnerId = ownerId,
                CreatedAt = DateTime.Now,
                CreatedBy = ownerId,
                Qty = qty
            };
            dbContext.CoinLog.Add(from);

            CoinLog to = new CoinLog()
            {
                Type = COIN_LOG_TYPE.EARN,
                OwnerId = receiverId,
                CreatedAt = DateTime.Now,
                CreatedBy = ownerId,
                Qty = qty
            };
            dbContext.CoinLog.Add(to);

            dbContext.SaveChanges();
        }
    }

    public class UserCoinLogBalance
    {
        public long OwnerId { get; set; }
        public int Qty { get; set; }
    }
}
