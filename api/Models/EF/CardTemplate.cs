using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homo.Bet.Api
{
    [Table("CardTemplate")]
    public partial class CardTemplate
    {
        [Key]
        [System.ComponentModel.DataAnnotations.Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Id")]
        public long Id { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; }

        [Column("DeletedAt")]
        public DateTime? DeletedAt { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MaxLength(128)]
        [Column("Name")]
        public string Name { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [Column("Type")]
        public CARD_TYPE Type { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MaxLength(512)]
        [Column("Description")]
        public string Description { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [Column("Cost")]
        public int Cost { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [Column("Probability")]
        public decimal Probability { get; set; }  // 出現機率 (0.0 ~ 1.0)
    }
}
