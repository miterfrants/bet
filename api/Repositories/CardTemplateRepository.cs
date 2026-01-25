using System;
using System.Collections.Generic;
using System.Linq;

namespace Homo.Bet.Api
{
    public class CardTemplateRepository
    {
        private readonly BargainingChipDBContext _dbContext;

        public CardTemplateRepository(BargainingChipDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        // 取得所有可用的卡片模板
        public List<CardTemplate> GetAllTemplates()
        {
            return _dbContext.CardTemplate
                .Where(x => x.DeletedAt == null)
                .OrderBy(x => x.Id)
                .ToList();
        }

        // 根據機率隨機抽取 N 張卡片模板
        public List<CardTemplate> DrawRandomTemplates(int count)
        {
            var templates = GetAllTemplates();
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
        public Card CreateCardFromTemplate(CardTemplate template, bool saveChanges = true)
        {
            Card card = new Card();
            card.CardTemplateId = template.Id;
            card.Name = template.Name;
            card.Type = template.Type;
            card.Description = template.Description;
            card.Cost = template.Cost;
            card.IsAvailable = true;
            card.CreatedAt = DateTime.Now;

            _dbContext.Card.Add(card);

            if (saveChanges)
            {
                _dbContext.SaveChanges();
            }

            return card;
        }

        // 每週生成卡片（根據模板隨機抽 5 張）
        public List<Card> GenerateWeeklyCards()
        {
            // 先清除所有現有的可用卡片（將未售出的卡片 soft delete）
            var existingCards = _dbContext.Card
                .Where(x => x.DeletedAt == null && x.IsAvailable == true)
                .ToList();

            foreach (var card in existingCards)
            {
                // Soft delete 未售出的卡片
                card.DeletedAt = DateTime.Now;
                card.IsAvailable = false;
            }

            // 根據機率隨機抽取 5 個模板
            var templates = DrawRandomTemplates(5);

            // 根據模板生成卡片
            var newCards = new List<Card>();
            foreach (var template in templates)
            {
                var card = CreateCardFromTemplate(template, saveChanges: false);
                newCards.Add(card);
            }

            // 一次性儲存所有變更
            _dbContext.SaveChanges();

            return newCards;
        }
    }
}
