using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HurryUpHaul.Domain.Models.Database
{
    [Table("orders")]
    internal class Order
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("details")]
        public string Details { get; set; }

        [Column("status")]
        public OrderStatus Status { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("last_updated_at")]
        public DateTime LastUpdatedAt { get; set; }

        public ICollection<OrderEvent> Events { get; set; }
    }
}