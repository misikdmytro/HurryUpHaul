using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace HurryUpHaul.Domain.Models.Database
{
    [Table("orders")]
    [Index(nameof(CreatedBy))]
    internal class Order
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(2000)]
        [Column("details")]
        public string Details { get; set; }

        [Column("status")]
        public OrderStatus Status { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Required]
        [MaxLength(256)]
        [Column("created_by")]
        public string CreatedBy { get; set; }

        [Column("last_updated_at")]
        public DateTime LastUpdatedAt { get; set; }

        public ICollection<OrderEvent> Events { get; set; }
    }
}