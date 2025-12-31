using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Homo.Bet.Api
{
    public class CardDataservice
    {
        // 取得所有可用的卡片（商店卡片）
        public static List<Card> GetAvailableCards(BargainingChipDBContext dbContext)
        {
            return dbContext.Card
                .Where(x => x.DeletedAt == null && x.IsAvailable == true)
                .OrderBy(x => x.Id)
                .ToList();
        }

        // 取得單一卡片
        public static Card GetCardById(BargainingChipDBContext dbContext, long cardId)
        {
            return dbContext.Card
                .Where(x => x.DeletedAt == null && x.Id == cardId)
                .FirstOrDefault();
        }

        // 購買卡片（建立 UserCard 紀錄 + Soft Delete Card）
        public static UserCard BuyCard(BargainingChipDBContext dbContext, long userId, long cardId)
        {
            // 建立 UserCard 紀錄
            UserCard record = new UserCard();
            record.UserId = userId;
            record.CardId = cardId;
            record.IsEquipped = false;
            record.CreatedAt = DateTime.Now;
            dbContext.UserCard.Add(record);

            // Soft Delete Card（標記為已售出，不再出現在商店）
            var card = dbContext.Card.FirstOrDefault(x => x.Id == cardId);
            if (card != null)
            {
                card.DeletedAt = DateTime.Now;
            }

            dbContext.SaveChanges();
            return record;
        }

        // 取得使用者擁有的所有卡片
        public static List<UserCard> GetUserCards(BargainingChipDBContext dbContext, long userId)
        {
            return dbContext.UserCard
                .Include(x => x.Card)
                .Where(x => x.DeletedAt == null && x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToList();
        }

        // 取得使用者已裝備的卡片
        public static List<UserCard> GetEquippedCards(BargainingChipDBContext dbContext, long userId)
        {
            return dbContext.UserCard
                .Include(x => x.Card)
                .Where(x => x.DeletedAt == null && x.UserId == userId && x.IsEquipped == true)
                .OrderBy(x => x.EquippedAt)
                .ToList();
        }

        // 裝備卡片
        public static UserCard EquipCard(BargainingChipDBContext dbContext, long userId, long userCardId, string triggerCondition = null)
        {
            var userCard = dbContext.UserCard
                .Include(x => x.Card)
                .Where(x => x.DeletedAt == null && x.Id == userCardId && x.UserId == userId)
                .FirstOrDefault();

            if (userCard == null) return null;

            // 檢查陷阱卡是否在週一裝備
            if (userCard.Card.Type == CARD_TYPE.TRAP)
            {
                // 檢查今天是否為週一
                if (DateTime.Now.DayOfWeek != DayOfWeek.Monday)
                {
                    return null;  // 陷阱卡只能在週一裝備
                }

                // 陷阱卡必須填寫觸發條件
                if (string.IsNullOrWhiteSpace(triggerCondition))
                {
                    return null;
                }
            }

            // 檢查是否已達裝備上限（3張）
            var equippedCount = dbContext.UserCard
                .Count(x => x.DeletedAt == null && x.UserId == userId && x.IsEquipped == true);

            if (equippedCount >= 3) return null;

            userCard.IsEquipped = true;
            userCard.EquippedAt = DateTime.Now;
            userCard.TriggerCondition = triggerCondition;
            dbContext.SaveChanges();
            return userCard;
        }

        // 卸下卡片
        public static UserCard UnequipCard(BargainingChipDBContext dbContext, long userId, long userCardId)
        {
            var userCard = dbContext.UserCard
                .Where(x => x.DeletedAt == null && x.Id == userCardId && x.UserId == userId)
                .FirstOrDefault();

            if (userCard == null) return null;

            userCard.IsEquipped = false;
            userCard.EquippedAt = null;
            dbContext.SaveChanges();
            return userCard;
        }

        // 使用卡片（僅限魔法卡，使用後立即消失）
        public static UserCard UseCard(BargainingChipDBContext dbContext, long userId, long userCardId)
        {
            var userCard = dbContext.UserCard
                .Include(x => x.Card)
                .Where(x => x.DeletedAt == null && x.Id == userCardId && x.UserId == userId)
                .FirstOrDefault();

            if (userCard == null) return null;

            // 只有魔法卡可以使用（陷阱卡和增益卡是裝備）
            if (userCard.Card.Type != CARD_TYPE.MAGIC) return null;

            // 使用後立即 soft delete
            userCard.DeletedAt = DateTime.Now;
            dbContext.SaveChanges();
            return userCard;
        }

        // 建立卡片（管理員用）
        public static Card CreateCard(BargainingChipDBContext dbContext, DTOs.Card dto)
        {
            Card record = new Card();
            record.Name = dto.Name;
            record.Type = dto.Type;
            record.Description = dto.Description;
            record.Cost = dto.Cost;
            record.IsAvailable = true;
            record.CreatedAt = DateTime.Now;
            dbContext.Card.Add(record);
            dbContext.SaveChanges();
            return record;
        }
    }
}
