using System;
using System.ComponentModel.DataAnnotations;
using Homo.Api;

namespace Homo.Bet.Api
{
    public abstract partial class DTOs
    {
        public partial class Card : DTOs
        {
            [System.ComponentModel.DataAnnotations.Required]
            [System.ComponentModel.DataAnnotations.MaxLength(128)]
            public string Name { get; set; }

            [System.ComponentModel.DataAnnotations.Required]
            public CARD_TYPE Type { get; set; }

            [System.ComponentModel.DataAnnotations.Required]
            [System.ComponentModel.DataAnnotations.MaxLength(512)]
            public string Description { get; set; }

            [System.ComponentModel.DataAnnotations.Required]
            public int Cost { get; set; }
        }

        public partial class BuyCard : DTOs
        {
            [System.ComponentModel.DataAnnotations.Required]
            public long CardId { get; set; }
        }

        public partial class EquipCard : DTOs
        {
            [System.ComponentModel.DataAnnotations.Required]
            public long UserCardId { get; set; }

            [System.ComponentModel.DataAnnotations.MaxLength(512)]
            public string TriggerCondition { get; set; }
        }
    }
}
