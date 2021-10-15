using System;
using System.Linq;

namespace Homo.Bet.Api
{
    public class VerifyCodeDataservice
    {
        public static int GetTodayCountByIp(BargainingChipDBContext dbContext, string ip)
        {
            DateTime startDate = DateTime.Now.AddDays(-1);
            return dbContext.VerifyCode
                .Where(
                    x => x.Ip == ip &&
                    x.Expiration >= startDate
                )
                .Count();
        }

        public static int GetTodayCountByPhone(BargainingChipDBContext dbContext, string phone)
        {
            DateTime startDate = DateTime.Now.AddDays(-1);
            return dbContext.VerifyCode
                .Where(
                    x => x.Phone == phone &&
                    x.Expiration >= startDate
                )
                .Count();
        }

        public static void UseVerifyCode(VerifyCode verifyCode, BargainingChipDBContext dbContext)
        {
            verifyCode.IsUsed = true;
            dbContext.SaveChanges();
        }

        public static int GetTodayCountByEmail(BargainingChipDBContext dbContext, string email)
        {
            DateTime startDate = DateTime.Now.AddDays(-1);
            return dbContext.VerifyCode
                .Where(
                    x => x.Email == email &&
                    x.Expiration >= startDate
                )
                .Count();
        }

        public static VerifyCode GetOneUnUsedByEmail(BargainingChipDBContext dbContext, string email, string code)
        {
            return dbContext.VerifyCode
                .Where(
                    x => x.Email == email
                    && x.Code == code
                    && x.Expiration >= DateTime.Now
                    && (x.IsUsed == null || x.IsUsed == false)
                ).OrderByDescending(x => x.CreatedAt).FirstOrDefault();
        }

        public static VerifyCode GetOneUnUsedByPhone(BargainingChipDBContext dbContext, string phone, string code)
        {
            return dbContext.VerifyCode
                .Where(
                    x => x.Phone == phone
                    && x.Code == code
                    && x.Expiration >= DateTime.Now
                    && (x.IsUsed == null || x.IsUsed == false)
                ).OrderByDescending(x => x.CreatedAt).FirstOrDefault();
        }

        public static VerifyCode Create(BargainingChipDBContext dbContext, DTOs.VerifyCode dto)
        {
            VerifyCode record = new VerifyCode();
            foreach (var propOfDTO in dto.GetType().GetProperties())
            {
                var value = propOfDTO.GetValue(dto);
                var prop = record.GetType().GetProperty(propOfDTO.Name);
                prop.SetValue(record, value);
            }
            record.CreatedAt = DateTime.Now;
            dbContext.VerifyCode.Add(record);
            dbContext.SaveChanges();
            return record;
        }
    }
}
