using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homo.Bet.Api
{
    [Table("UserCard")]
    public partial class UserCard
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
        [Column("UserId")]
        public long UserId { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [Column("CardId")]
        public long CardId { get; set; }

        [Column("IsEquipped")]
        public bool IsEquipped { get; set; }

        [Column("EquippedAt")]
        public DateTime? EquippedAt { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("CardId")]
        public virtual Card Card { get; set; }
    }
}
