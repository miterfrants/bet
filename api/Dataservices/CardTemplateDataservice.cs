using System;
using System.Collections.Generic;
using System.Linq;

namespace Homo.Bet.Api
{
    public class CardTemplateDataservice
    {
        // 取得所有可用的卡片模板
        public static List<CardTemplate> GetAllTemplates(BargainingChipDBContext dbContext)
        {
            return dbContext.CardTemplate
                .Where(x => x.DeletedAt == null)
                .OrderBy(x => x.Id)
                .ToList();
        }

        // 根據機率隨機抽取 N 張卡片模板
        public static List<CardTemplate> DrawRandomTemplates(BargainingChipDBContext dbContext, int count)
        {
            var templates = GetAllTemplates(dbContext);
            if (templates.Count == 0) return new List<CardTemplate>();

            var random = new Random();
            var drawnTemplates = new List<CardTemplate>();

            // 計算總機率
            decimal totalProbability = templates.Sum(t => t.Probability);

            for (int i = 0; i < count; i++)
            {
                // 生成隨機數（0.0 ~ totalProbability）
                decimal randomValue = (decimal)random.NextDouble() * totalProbability;

                // 根據機率選擇卡片模板
                decimal cumulativeProbability = 0;
                foreach (var template in templates)
                {
                    cumulativeProbability += template.Probability;
                    if (randomValue <= cumulativeProbability)
                    {
                        drawnTemplates.Add(template);
                        break;
                    }
                }
            }

            return drawnTemplates;
        }

        // 根據模板生成卡片
        public static Card CreateCardFromTemplate(BargainingChipDBContext dbContext, CardTemplate template)
        {
            Card card = new Card();
            card.CardTemplateId = template.Id;
            card.Name = template.Name;
            card.Type = template.Type;
            card.Description = template.Description;
            card.Cost = template.Cost;
            card.IsAvailable = true;
            card.CreatedAt = DateTime.Now;

            dbContext.Card.Add(card);
            dbContext.SaveChanges();

            return card;
        }

        // 每週生成卡片（根據模板隨機抽 5 張）
        public static List<Card> GenerateWeeklyCards(BargainingChipDBContext dbContext)
        {
            // 先清除所有現有的可用卡片（將未售出的卡片標記為不可用）
            var existingCards = dbContext.Card
                .Where(x => x.DeletedAt == null && x.IsAvailable == true)
                .ToList();

            foreach (var card in existingCards)
            {
                card.IsAvailable = false;
            }

            // 隨機抽取 5 個模板
            var templates = DrawRandomTemplates(dbContext, 5);

            // 根據模板生成卡片
            var newCards = new List<Card>();
            foreach (var template in templates)
            {
                var card = CreateCardFromTemplate(dbContext, template);
                newCards.Add(card);
            }

            return newCards;
        }
    }
}
